using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanism
{
    public class SkillBehaviour
    {
        private static Dictionary<string, SkillBehaviour> behaviourBySkillId = new Dictionary<string, SkillBehaviour>();

        public delegate bool Condition(Character caster, SpellVessel vessel, Character target);
        public delegate bool CooldownSpecifier(Character caster, SpellVessel vessel, Character target, out float cooldownFactor);
        private (Condition, string)[] useConditions;
        private CooldownSpecifier applyCooldownImmediately;
        private Action<Character, SpellVessel, Character> onSuccessfulUse;

        public static void AddSkillBehaviour(Skill skill, Action<Character, SpellVessel, Character> onSuccessfulUse, CooldownSpecifier applyCooldownImmediately, params (Condition, string)[] useConditions)
        {
            var behaviour = new SkillBehaviour(onSuccessfulUse, applyCooldownImmediately, useConditions);
            behaviourBySkillId[skill.Id] = behaviour;
        }

        public static bool GetBehaviour(Skill skill, out SkillBehaviour behaviour)
        {
            return behaviourBySkillId.TryGetValue(skill.Id, out behaviour);
        }

        private SkillBehaviour(Action<Character, SpellVessel, Character> onSuccessfulUse, CooldownSpecifier applyCooldownImmediately, (Condition, string)[] useConditions)
        {
            this.onSuccessfulUse = onSuccessfulUse;
            this.useConditions = useConditions;
            this.applyCooldownImmediately = applyCooldownImmediately;
        }

        // void OnSuccessfulUse() -- this would call a stored Action customisable for different skills, for example Twin Spell's one would use this to apply the twin spell component or set next target

        public bool CanBeUsed(Character caster, SpellVessel vessel, Character target, out string failureReason)
        {
            if (useConditions != null)
            {
                foreach(var c in useConditions)
                {
                    if (!c.Item1(caster, vessel, target))
                    {
                        failureReason = c.Item2;
                        return false;
                    }
                }
            }

            failureReason = null;
            return true;
        }

        public bool ShouldApplyCooldownImmediately(Character caster, SpellVessel vessel, Character target, out float cooldownFactor)
        {
            if (applyCooldownImmediately == null)
            {
                cooldownFactor = 1f;
                return true;
            }

            return applyCooldownImmediately(caster, vessel, target, out cooldownFactor);
        }

        public void OnSuccessfulUse(Character caster, SpellVessel vessel, Character target)
        {
            onSuccessfulUse?.Invoke(caster, vessel, target);
        }
    }

}
