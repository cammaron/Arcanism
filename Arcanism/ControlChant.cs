using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using Arcanism.Patches;

namespace Arcanism
{
    class ControlChant : SpellAbility // Counter-intuitively, ControlChant implements SpellAbility because it modifies the cooldown of the spell being cast
    {
        // These values represent *additional* factors, i.e. a value of 0.5 means +50% compared to normal
        public static readonly float EXTRA_CAST_TIME_FACTOR = 0.6f;
        public static readonly float EXTRA_MANA_COST_FACTOR = 1.5f;
        public static readonly float EXTRA_DAMAGE_FACTOR = 1f;
        public static readonly float EXTRA_COOLDOWN_FACTOR = 0.5f;

        SpellVessel vessel;
        CastSpell spellSource;
        Character caster;
        Traverse<float> overChantLifeField;
        Traverse<float> overChantTotalField;

        private int totalManaDrain;
        private float manaDrainPerSecond;
        private float drainProgress = 0f;

        protected override SpellVessel Vessel => vessel;

        protected override UseSkill CasterSkills => caster.MySkills;

        protected override CastSpell SpellSource => this.spellSource;

        public void Initialise(SpellVessel vessel, Character caster, CastSpell spellSource, Traverse<float> overChantLifeField, Traverse<float> overChantTotalField)
        {
            this.vessel = vessel;
            this.caster = caster;
            this.spellSource = spellSource;
            this.overChantLifeField = overChantLifeField;
            this.overChantTotalField = overChantTotalField;

            if (caster.MyStats.CharacterClass == GameData.ClassDB.Arcanist)
                overChantTotalField.Value = vessel.spell.SpellChargeTime * EXTRA_CAST_TIME_FACTOR;

            // For overchanting, we now drain additional mana per second to sustain the process, making it powerful but a little less cost effective -- though ideal for things with high cooldown or for pre-casting
            // A full overchant should drain an additional 150% of the spell's normal mana cost for up to 100% more damage
            totalManaDrain = Mathf.RoundToInt(vessel.spell.ManaCost * (1 + EXTRA_MANA_COST_FACTOR));
            manaDrainPerSecond = totalManaDrain / (overChantTotalField.Value / 60f); //    The "/ 60f" is because SpellChargeTime/castTime/overChantTotal is measured in 60ths of a second
        }

        void Update()
        {
            if (overChantLifeField.Value > 0)
            {
                drainProgress += manaDrainPerSecond * Time.deltaTime; // mana is an int, so we build tiny increments per frame until we get a whole number to deduct
                if (drainProgress > 1f)
                {
                    drainProgress -= 1f;
                    caster.MyStats.ReduceMana(1);
                    if (caster.MyStats.CurrentMana <= vessel.spell.ManaCost) // At least enough mana to finish casting the spell (which isn't deducted 'til execution time) must remain in the bank
                    {
                        Backfire(totalManaDrain);
                    }
                }
            }
        }

        public void EndChant(float completionFactor, Traverse<float> damageMultiField)
        {
            float cooldownFactor;
            bool allowTargetDamage = true;
            if (completionFactor < 1)
            {
                /*- Release a spell early with reduced cooldown and mana cost but at *least* the same damage
		        - Chance of backfire proportionate to chant length. Backfire damage to self  = 2 * mana cost
		        - Skill book 1: Backfired spells still damage the enemy (but only half)
		        - Skill book 2: Backfired spells do full damage to enemy*/

                // early release: reduced cooldown and mana cost, full damage... but the chance of a backfire!

                damageMultiField.Value = 1f;
                cooldownFactor = completionFactor;
                vessel.UseMana = false;
                caster.MyStats.ReduceMana(Mathf.RoundToInt(vessel.spell.ManaCost * completionFactor));

                float backfireChance = (1 - (completionFactor)) * 100f;
                float targetDamageFactorOnBackfire = 0f;
                bool endSpellOnBackfire = true;

                if (caster.MySkills.KnowsSkill(SkillDBStartPatch.EXPERT_CONTROL_SKILL_ID))
                {
                    endSpellOnBackfire = false;
                    backfireChance *= .7f;
                    targetDamageFactorOnBackfire += .5f; // 50% for havign 1st skill
                }
                if (caster.MySkills.KnowsSkill(SkillDBStartPatch.EXPERT_CONTROL_2_SKILL_ID))
                {
                    endSpellOnBackfire = false;
                    backfireChance *= .7f;
                    targetDamageFactorOnBackfire += .5f; // total 100% normal damage if you have both
                }

                if (Random.Range(0, 100) < backfireChance)
                {
                    Backfire(Mathf.RoundToInt(vessel.spell.ManaCost));

                    if (endSpellOnBackfire)
                        allowTargetDamage = false;
                    else
                        damageMultiField.Value = targetDamageFactorOnBackfire;
                }
                // otherwise no backfire, so continue to as normal to ResolveSpell
            }
            else
            {
                completionFactor = overChantLifeField.Value / overChantTotalField.Value;
                damageMultiField.Value = 1 + Mathf.Lerp(0f, EXTRA_DAMAGE_FACTOR, completionFactor);
                cooldownFactor = 1 + Mathf.Lerp(0f, EXTRA_COOLDOWN_FACTOR, completionFactor);
                // No need to increase mana cost here -- the additional mana is drained per second already in Update
            }

            if (allowTargetDamage) { 
                vessel.ResolveEarly();
                ApplyCooldown(cooldownFactor); // override the default cooldown
            }
            else
                EndSpell(cooldownFactor);
        }

        public void Backfire(int manaBurned)
        {
            int damage = Mathf.RoundToInt(manaBurned * 2f);
            UpdateSocialLog.CombatLogAdd("BACKFIRE!! You fail to control the chaotic energy in time, and your own mana hurts you for " + damage + " damage!", "red");

            caster.DamageMe(damage, true, vessel.spell.MyDamageType, caster, true, false);
            caster.MyAudio.PlayOneShot(vessel.spell.CompleteSound, GameData.SFXVol * caster.MyAudio.volume * 0.8f);
        }

        public override void ApplyCooldown(float cooldownFactor = 1f)
        {
            base.ApplyCooldown(cooldownFactor);
            // Somewhat uniquely, ControlChant can be responsible for triggering both the spell's cooldown AND its own skill cooldown
            var skill = GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.CONTROL_CHANT_SKILL_ID);
            GameData.HKMngr.GetHotkeysForSkill(skill).ForEach(hk => hk.Cooldown = skill.Cooldown);
        }
    }
}
