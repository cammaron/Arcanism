using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using Arcanism.Patches;
using Arcanism.SkillExtension;
using UnityEngine.UI;

namespace Arcanism.Skills
{
    class ControlChant : ExtendedSkill, ISpellDamageModifier, ISpellCooldownModifier
    {
        // These values represent *additional* factors, i.e. a value of 0.5 means +50% compared to normal
        public static readonly float EXTRA_CAST_TIME_FACTOR = 0.6f;
        public static readonly float EXTRA_MANA_COST_FACTOR = 1.5f;
        public static readonly float EXTRA_DAMAGE_FACTOR = 1f;
        public static readonly float EXTRA_COOLDOWN_FACTOR = 0.5f;
        private readonly static Color32 PERFECT_RELEASE_COLOR = new Color32(255, 215, 0, 255);

        private enum State
        {
            INITIALISING,
            ACTIVATED,
            FAILED,
            FINISHED
        }

        private State state;
        private Stats spellTarget;

        Traverse<float> effectLife;
        Traverse<float> totalLife;
        Traverse<float> overChantLifeField;
        Traverse<float> overChantTotalField;

        private int totalManaDrain;
        private float manaDrainPerSecond;
        private float drainProgress = 0f;

        private float damageMulti = 1f;
        private float cooldownMulti = 1f;

        private Image castBar;
        private Image overchantBar;
        private Color originalCBColor;
        private Color originalOverchantColor;
        private Spell exhaustionEffect;

        public static void CreateExtension(Skill coreSkill)
        {
            ExtensionManager.AddExtension(coreSkill, (caster, vessel) => {
                var component = caster.gameObject.GetComponent<ControlChant>();
                if (component == null)
                {
                    component = caster.gameObject.AddComponent<ControlChant>();

                    component.state = State.INITIALISING;
                    component.skill = coreSkill;
                    component.caster = caster;
                    component.vessel = vessel;

                    if (vessel != null)
                    {
                        var vesselTraversal = Traverse.Create(vessel);
                        component.spellTarget = vesselTraversal.Field<Stats>("targ").Value;
                        component.effectLife = vesselTraversal.Field<float>("EffectLife");
                        component.totalLife = vesselTraversal.Field<float>("totalLife");
                        component.overChantLifeField = vesselTraversal.Field<float>("overChantLife");
                        component.overChantTotalField = vesselTraversal.Field<float>("overChantTotal");

                        // For overchanting, we now drain additional mana per second to sustain the process, making it powerful but a little less cost effective -- though ideal for things with high cooldown or for pre-casting
                        // A full overchant should drain an additional 150% of the spell's normal mana cost for up to 100% more damage
                        component.totalManaDrain = Mathf.RoundToInt(vessel.spell.ManaCost * (1 + EXTRA_MANA_COST_FACTOR));
                        component.manaDrainPerSecond = component.totalManaDrain / (component.overChantTotalField.Value / 60f); //    The "/ 60f" is because SpellChargeTime/castTime/overChantTotal is measured in 60ths of a second
                    }

                }


                return component;
            });
        }

        protected override IEnumerable<(Condition, string)> GetUseConditions()
        {
            return new List<(Condition, string)>() {
                ((caster, vessel, target) => caster.MySpells != null && vessel != null && caster.MySpells.isCasting(),  "You must be channeling a spell to use this skill."),
                ((caster, vessel, target) => caster.MySpells.GetCurrentCast().Type == Spell.SpellType.Damage,           "This skill only works on DAMAGE spells."),
                ((caster, vessel, target) => caster.GetComponent<TwinSpell>() == null,                                  "You WISH you could stack this with Twin Spell... but no. You can't."),
                ((caster, vessel, target) => vessel.UseMana,                                                            "Can't be used with spells that don't channel mana.")
            };
        }

        protected override void ApplySkill(Character target)
        {
            // this could be the original use (beginning chant) or the final use (ending chant)
            // and that logic could all be handled here, however it's all hardcoded into vanilla UseSkill and SpellVessel
            // so... just leaving this blank for now?
            if (state == State.INITIALISING)
            {
                overChantTotalField.Value = vessel.spell.SpellChargeTime * EXTRA_CAST_TIME_FACTOR;
                state = State.ACTIVATED;
            }

            // SpellVessel already internally handles (and depends on) some chant control state so we delegate back there and just intercept at the points when establishing damage and cooldown
            vessel.DoControlledChant();
        }

        protected override bool ShouldApplyCooldownOnUse()
        {
            return false; // Never want to apply cooldown on 1st use, and beyond that -- cooldown is applied automatically as part of spell finishing anyway.
        }
        
        void Awake()
        {
            castBar = GameData.CB.TopBar.GetComponent<Image>();
            overchantBar = GameData.CB.OverchantBar.GetComponent<Image>();
            originalCBColor = castBar.color;
            originalOverchantColor= overchantBar.color;
            exhaustionEffect = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.EXHAUSTION_SPELL_ID);
        }

        void ResetCastBarColors()
        {
            if (castBar != null) castBar.color = originalCBColor;
            if (overchantBar != null) overchantBar.color = originalOverchantColor;
        }

        void OnDestroy()
        {
            ResetCastBarColors();
        }

        protected override void Update()
        {
            base.Update();
            if (state == State.ACTIVATED)
            {
                if (IsPerfectReleaseReady(CalculateCompletionFactor()))
                    castBar.color = overchantBar.color = PERFECT_RELEASE_COLOR;
                else if (castBar.color == PERFECT_RELEASE_COLOR)
                    ResetCastBarColors();

                if (overChantLifeField.Value > 0)
                {
                    drainProgress += manaDrainPerSecond * Time.deltaTime; // mana is an int, so we build tiny increments per frame until we get a whole number to deduct
                    if (drainProgress > 1f)
                    {
                        drainProgress -= 1f;
                        caster.MyStats.ReduceMana(1);
                        if (caster.MyStats.CurrentMana <= vessel.spell.ManaCost) // At least enough mana to finish casting the spell (which isn't deducted 'til execution time) must remain in the bank
                        {
                            Backfire(totalManaDrain); // NB totalManaDrain already includes base spell mana cost, so this is more damage than a controlled backfire
                            state = State.FAILED;
                            vessel.ResolveEarly();
                        }
                    }
                }
            }
        }

        protected bool IsPerfectReleaseReady(float completionFactor)
        {
            const float perfectReleaseFactor = .03f;
            if (!caster.MySkills.KnowsSkill(SkillDB_Start.PERFECT_RELEASE_SKILL_ID))
                return false;

            if (caster.MyStats.CheckForStatus(exhaustionEffect))
                return false;

            return completionFactor >= 1 - perfectReleaseFactor && completionFactor <= 1 + perfectReleaseFactor;
        }

        private float CalculateCompletionFactor()
        {
            return effectLife.Value / totalLife.Value;
        }

        private void HandlePerfectRelease()
        {
            var dmgPop = GameData.Misc.CreateDmgPopClone("PERFECT RELEASE!!", caster.transform);
            dmgPop.Num.color = PERFECT_RELEASE_COLOR;

            ResetCastBarColors();

            if (caster.MySkills.isPlayer) UpdateSocialLog.CombatLogAdd("PERFECT RELEASE! Mana cost and cooldown not applied to spell! You cannot do this again whilst exhausted.", "green");

            vessel.UseMana = false;
            cooldownMulti = 0f;

            var twinSpellSkill = GameData.SkillDatabase.GetSkillByID(SkillDB_Start.TWIN_SPELL_SKILL_ID);
            if (caster.MySkills.KnowsSkill(SkillDB_Start.PERFECT_RELEASE_2_SKILL_ID) && caster.MySkills.KnowsSkill(twinSpellSkill))
            {
                caster.GetCooldownManager().ResetCooldown(twinSpellSkill);
                if (caster.MySkills.isPlayer) UpdateSocialLog.CombatLogAdd("Twin Spell cooldown reset by Perfect Release!", "green");
            }

            caster.MyStats.AddStatusEffectNoChecks(exhaustionEffect, true, 0, caster);
        }

        private void HandleEarlyRelease(float completionFactor)
        {
            /*- Release a spell early with reduced cooldown and mana cost but at *least* the same damage
            - Chance of backfire proportionate to chant length. Backfire damage to self  = 2 * mana cost
            - Skill book 1: Backfired spells still damage the enemy (but only half)
            - Skill book 2: Backfired spells do full damage to enemy*/

            // early release: reduced cooldown and mana cost, full damage... but the chance of a backfire!
            cooldownMulti = completionFactor;

            vessel.UseMana = false;
            caster.MyStats.ReduceMana(Mathf.RoundToInt(vessel.spell.ManaCost * completionFactor));

            float backfireChance = (1 - (completionFactor)) * 100f;
            float targetDamageFactorOnBackfire = 0f;

            foreach (var expertControlSkillId in new string[] { SkillDB_Start.EXPERT_CONTROL_SKILL_ID, SkillDB_Start.EXPERT_CONTROL_2_SKILL_ID })
            {
                if (caster.MySkills.KnowsSkill(expertControlSkillId))
                {
                    backfireChance = (backfireChance - 7.5f) * .85f;
                    targetDamageFactorOnBackfire += .5f; // 50% for havign 1st skill
                }
            }

            if (Random.Range(0, 100) < Mathf.Max(3, backfireChance))
            {
                Backfire(Mathf.RoundToInt(vessel.spell.ManaCost));
                damageMulti = targetDamageFactorOnBackfire;
            }
        }

        private void HandleOverChant()
        {
            float completionFactor = overChantLifeField.Value / overChantTotalField.Value;
            damageMulti = 1 + Mathf.Lerp(0f, EXTRA_DAMAGE_FACTOR, completionFactor);
            cooldownMulti = 1 + Mathf.Lerp(0f, EXTRA_COOLDOWN_FACTOR, completionFactor);
            // No need to increase mana cost here -- the additional mana is drained per second already in Update
        }

        public float GetSpellDamageMulti()
        {
            if (state == State.FAILED)
                return 0;

            float completionFactor = CalculateCompletionFactor();

            if (IsPerfectReleaseReady(completionFactor))
                HandlePerfectRelease();

            else if (completionFactor < 1)
                HandleEarlyRelease(completionFactor);

            else
                HandleOverChant();

            return this.damageMulti;
        }

        public float GetSpellCooldownMulti()
        {
            if (state == State.FAILED)
                cooldownMulti = 1;

            state = State.FINISHED; // Being finished means we're ready to destroy which will trigger skill cooldown.
            return cooldownMulti;
        }

        protected void Backfire(int manaBurned)
        {
            int damage = manaBurned * 2;
            var dmgPop = GameData.Misc.CreateDmgPopClone("BACKFIRE!!", caster.transform);
            dmgPop.Num.color = Color.red;
            if (caster.MySkills.isPlayer) UpdateSocialLog.CombatLogAdd($"BACKFIRE!! You fail to control the chaotic energy in time, and your own mana hurts you for {damage} damage!", "red");

            caster.DamageMe(damage, true, vessel.spell.MyDamageType, caster, true, false);
            caster.MyAudio.PlayOneShot(vessel.spell.CompleteSound, GameData.SFXVol * caster.MyAudio.volume * 0.8f);
        }

        protected override bool IsInterrupted() {
            return vessel == null || spellTarget == null || spellTarget.Myself == null || spellTarget.Myself.IsDead();
        }

        protected override bool IsFinished() => state == State.FINISHED;
    }
}
