using UnityEngine;
using System;
using System.Collections.Generic;
using Arcanism.Patches;
using HarmonyLib;
using Arcanism.SkillExtension;

namespace Arcanism.Skills
{
    class TwinSpell : ExtendedSkill, IRetargetingSkill, ISpellDamageModifier
    {
        public static bool AllowSameTarget = false;

        private Traverse<Stats> vesselTargetField;
        private Character originalSpellTarget; 
        private readonly List<Character> allTargets = new List<Character>();

        int currentTargetIndex = 0;
        float showTextUntilTime;
        int maxTargets;
        bool hasAttacked = false;
        bool appliedParasiticTwin = false;
        bool isFinished = false;

        HashSet<Character> npcsDiedWhileCasting;

        public static void CreateExtension(Skill coreSkill)
        {
            ExtensionManager.AddExtension(coreSkill, (caster, vessel) => {
                var component = caster.gameObject.GetComponent<TwinSpell>();
                if (component == null)
                {
                    component = caster.gameObject.AddComponent<TwinSpell>();
                    component.skill = coreSkill;
                    component.caster = caster;
                    component.vessel = vessel;

                    if (vessel != null)
                    {
                        var traversal = Traverse.Create(vessel);
                        component.vesselTargetField = traversal.Field<Stats>("targ");
                        component.originalSpellTarget = component.vesselTargetField.Value.Myself;
                        component.allTargets.Add(component.originalSpellTarget);
                    }

                    component.maxTargets = 1; // original target, plus one per skill v
                    if (caster.MySkills.KnowsSkill(SkillDB_Start.TWIN_SPELL_SKILL_ID)) component.maxTargets += 1;
                    if (caster.MySkills.KnowsSkill(SkillDB_Start.TWIN_SPELL_MASTERY_SKILL_ID)) component.maxTargets += 1;
                    if (caster.MySkills.KnowsSkill(SkillDB_Start.TWIN_SPELL_MASTERY_2_SKILL_ID)) component.maxTargets += 1;
                }

                return component;
            });
        }

        protected override void Update()
        {
            base.Update();

            if (caster.MySkills.isPlayer && Time.time >= showTextUntilTime)
                GameData.CB.OCTxt.transform.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (caster.MySkills.isPlayer)
                GameData.CB.OCTxt.text = "-Controlled Chant-";
        }

        protected override IEnumerable<(Condition, string)> GetUseConditions()
        {
            return new List<(Condition, string)>()
            {
                ((caster, vessel, target) => caster.MySpells != null && vessel != null && caster.MySpells.isCasting(),                          "You must be channeling a spell to use this skill."),
                ((caster, vessel, target) => caster.MySpells.GetCurrentCast().Type == Spell.SpellType.Damage,                                   "This skill only works on DAMAGE spells."),
                ((caster, vessel, target) => target != null && target != caster,                                                                "Invalid target!"),
                ((caster, vessel, target) => caster.GetComponent<ControlChant>() == null,                                                       "Cannot be used while controlling a chant."),
                ((caster, vessel, target) => AllowSameTarget || !allTargets.Contains(target),                                                   "Cannot be used on the same target twice."),
                ((caster, vessel, target) => caster.MyStats.GetCurrentMana() - vessel.spell.ManaCost >= GetNextTargetManaCost(),                "You need more mana!"),
                ((caster, vessel, target) => CanAddMoreTargets(),                                                                               "Unable to twin this spell any further."),
                ((caster, vessel, target) => vessel.spell.SpellRange >= Vector3.Distance(caster.transform.position, target.transform.position), "Target is too far away!"),
            };
        }

        protected override bool ShouldApplyCooldownOnUse()
        {
            return false; // return !CanAddMoreTargets(); -- actually let's stick w/ cooling down only on spell release, that way we can cancel cast without punishment aside from last mana.
        }

        protected override void ApplySkill(Character target)
        {
            caster.MyStats.ReduceMana(GetNextTargetManaCost());
            
            allTargets.Add(target);

            if (caster.MySkills.isPlayer)
            {
                UpdateSocialLog.LogAdd($"Twinning spell to {target.name} (target {allTargets.Count}/{maxTargets})");
                GameData.CB.OCTxt.text = allTargets.Count == 2 ? "-Twinned!-" : allTargets.Count == 3 ? "-TRIPLED!-" : "-FULL STACK!!-";
                GameData.CB.OCTxt.transform.gameObject.SetActive(true);
                GameData.CB.OCTxt.fontSize = 48;
                showTextUntilTime = Time.time + 0.6f;
            }
        }

        public float GetSpellDamageMulti()
        {
            float multi = 1;
            if (!hasAttacked) // This being called means SpellVessel is about to begin cycling targets. Whoever is already dead has died not by this spell's hand, so we process them for Vanishing Twin.
            {
                npcsDiedWhileCasting = new HashSet<Character>(allTargets.FindAll(t => t == IsTargetDead(t)));
                
                if (caster.MySkills.KnowsSkill(SkillDB_Start.VANISHING_TWIN_SKILL_ID))
                    multi += npcsDiedWhileCasting.Count;

            }

            hasAttacked = true;


            if (caster.MySkills.KnowsSkill(SkillDB_Start.SIBLING_SYNGERY_SKILL_ID))
                multi *= 1 + (SkillDB_Start.SIBLING_SYNERGY_POWER * allTargets.Count);

            return multi;
        }

        public Character GetNextTarget()
        {
            var lastTarget = allTargets[currentTargetIndex];

            // If the prior target is now dead, and wasn't already among those who died while waiting for cast, it means they were killed by Twin Spell, so proc Parasitic Twin
            if (!appliedParasiticTwin && IsTargetDead(lastTarget) && (npcsDiedWhileCasting == null || !npcsDiedWhileCasting.Contains(lastTarget)))
                ApplyParasiticTwin();

            // If we've just processed the final target, it's time to trigger cooldown for the skill (if it hasn't already been done)
            if (currentTargetIndex == allTargets.Count - 1)
            {
                isFinished = true;
                return null;
            }

            return allTargets[++currentTargetIndex];
        }

        private void ApplyParasiticTwin()
        {
            UpdateSocialLog.CombatLogAdd("The dead target's soul saps the life of those connected to it by Twin Spell!");
            var parasiticTwinSpell = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.PARASITIC_TWIN_SPELL_ID);
            // Parasitic Twin *has* no base target damage, thus all its damage is the "bonus damage" -- it's completely calculated by int/prof, scaling with character growth. With a max int around 353ish (before blessings) and max int prof, this will hit for 1700 base (*BASE*, as in, before other int/prof scaling increases it. so, same as damage listing on any spell desc)
            int baseDamage = Mathf.RoundToInt(caster.MyStats.GetCurrentInt() * 5 * (caster.MyStats.IntScaleMod / 40));
            int modifiedDamage = SpellVessel_CalcDmgBonus.CalculateSpellDamage(caster, baseDamage, false);

            foreach (var targ in allTargets)
            {
                if (!IsTargetDead(targ))
                    targ.MyStats.AddStatusEffect(parasiticTwinSpell, caster.MySkills.isPlayer, modifiedDamage);
            }

            appliedParasiticTwin = true;
        }

        private int GetNextTargetManaCost()
        {
            // 1.3x exponent means 15% more mana than 2 casts for 2 targets; 33% more for 3; 54% more for 4. Remember, that's 54% more than *4 casts* which is already a HUGE amount of mana usage.
            float costExponent = 1.3f * (1 - caster.MySkills.GetAscensionRank(SkillDB_Start.MIND_SPLIT_ASCENSION_ID) * SkillDB_Start.MIND_SPLIT_COST_FACTOR);
            int additionalManaCost = (int)(Mathf.Pow(costExponent, allTargets.Count) * vessel.spell.ManaCost);

            return additionalManaCost;
        }

        protected override float CalculateCooldownFactor()
        {
            int refractionLevel = caster.MySkills.GetAscensionRank(SkillDB_Start.REFRACTION_ASCENSION_ID);
            return 1f - (refractionLevel * SkillDB_Start.REFRACTION_COOLDOWN_FACTOR);
        }

        private bool CanAddMoreTargets() => allTargets.Count < maxTargets;

        private bool IsTargetDead(Character target) => target == null || target.IsDead(); // it seems like NPCs are not destroyed, they just become dead bodies then respawn so in theory shouldn't null. Still...


        protected override bool IsInterrupted() => vessel == null;

        protected override bool IsFinished() => isFinished;

        public IEnumerable<Character> GetAllTargets()
        {
            return allTargets;
        }
    }
}
