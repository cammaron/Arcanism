using UnityEngine;
using System;
using System.Collections.Generic;
using Arcanism.Patches;

namespace Arcanism
{
    class TwinSpell : SkillAbility
    {
        public static bool AllowSameTarget = false;

        private SpellVessel vessel;
        private Character caster;
        private UseSkill skillSource;
        private Skill skill;

        private Queue<Character> targets = new Queue<Character>();
        float showTextUntilTime;
        int maxTwinTargets;
        bool coolingDown = false;

        protected override UseSkill SkillSource => this.skillSource;

        protected override Skill Skill => this.skill;
        public int MaxTwinTargets => this.maxTwinTargets;

        public int TwinnedTargetCount => this.targets.Count;

        public void Initialise(SpellVessel vessel, Character caster, UseSkill skillSource)
        {
            this.vessel = vessel;
            this.caster = caster;
            this.skillSource = skillSource;

            this.skill = GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.TWIN_SPELL_SKILL_ID);

            maxTwinTargets = 0;
            if (caster.MySkills.KnowsSkill(SkillDBStartPatch.TWIN_SPELL_SKILL_ID)) maxTwinTargets += 1;
            if (caster.MySkills.KnowsSkill(SkillDBStartPatch.TWIN_SPELL_MASTERY_SKILL_ID)) maxTwinTargets += 1;
        }

        void Update()
        {
            if (Time.time >= showTextUntilTime)
                GameData.CB.OCTxt.transform.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            GameData.CB.OCTxt.text = "-Controlled Chant-";
        }

        public bool AddTarget(Character c)
        {
            if (targets.Count == MaxTwinTargets)
            {
                UpdateSocialLog.LogAdd("Unable to twin this spell any further.", "yellow");
                return false;
            }

            int targetNumber = targets.Count + 2; // targets in queue plus original spell target and this new hypothetical target
            float costExponent = 1.75f * (1 - c.MySkills.GetAscensionRank(SkillDBStartPatch.MIND_SPLIT_ASCENSION_ID) * SkillDBStartPatch.MIND_SPLIT_COST_FACTOR); // at 1.75, 2 casts costs 37.5% more mana than casting twice normally. At 3 casts, it's around 94% more than 3 normal casts. With max Mind Split, it's a 1.225 exponent and only costs around 24% more.
            int additionalManaCost = (int) (Mathf.Pow(costExponent, targetNumber - 1) * vessel.spell.ManaCost);
            if (caster.MyStats.GetCurrentMana() - vessel.spell.ManaCost < additionalManaCost)
            {
                UpdateSocialLog.LogAdd("You need more mana!", "yellow");
                return false;
            }
            
            UpdateSocialLog.LogAdd($"Twinning spell to {c.name} (target {targetNumber}/{MaxTwinTargets + 1})");
            caster.MyStats.ReduceMana(additionalManaCost);

            targets.Enqueue(c);
            if (targets.Count == 1)
                GameData.CB.OCTxt.text = "-Twinned!-";
            else
                GameData.CB.OCTxt.text = "-TRIPLED!-";

            GameData.CB.OCTxt.transform.gameObject.SetActive(true);
            showTextUntilTime = Time.time + 0.6f;

            return true;
        }

        public override void ApplyCooldown(float cooldownFactor = 1f)
        {
            float ascensionCdReduction = caster.MySkills != null ? 1f - (caster.MySkills.GetAscensionRank(SkillDBStartPatch.REFRACTION_ASCENSION_ID) * SkillDBStartPatch.REFRACTION_COOLDOWN_FACTOR) : 1f;
            base.ApplyCooldown(cooldownFactor * ascensionCdReduction);
            coolingDown = true;
        }

        public Character NextTarget 
        { 
            get 
            {
                if (targets.Count > 0)
                {
                    var next = targets.Dequeue();
                    if (targets.Count == 0 && !coolingDown)
                        ApplyCooldown(); // The cooldown is triggered by normal skill usage ONCE max targets are acquired -- but in the event we still had a spare, instead apply cooldown as soon as all targets are removed (meaning the spell is executing)
                    return next;
                }
                return null;
            } 
        }

        public bool IsTargeted(Character c)
        {
            return targets.Contains(c);
        }

    }
}
