using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Arcanism
{
    public abstract class ExtendedSkill : MonoBehaviour
    {
        protected delegate bool Condition(Character caster, SpellVessel vessel, Character target);

        protected Character caster;
        protected SpellVessel vessel;
        protected Character target;
        protected Skill skill;

        private bool firstUse = true;
        private bool coolingDown = false;

        protected virtual void Update()
        {
            var finished = IsFinished();
            if (finished) TriggerCooldown();

            if (finished || IsInterrupted())
                Destroy(this);
        }

        /* Return a simple list of tuples where the first item of each is a Func testing a single viability condition for skill usage (and returning true if viable), 
         * and the second is an informative message that will be shown to a player if the condition fails. */
        protected abstract IEnumerable<(Condition, string)> GetUseConditions();

        protected abstract void ApplySkill(Character target);

        /* Since the skill extension system inherently controls the adding of its components to gameobjects, it must be responsible for removing them, too.
        * Thusly should any inheritor be forced to implement this to make this explicit. */
        protected abstract bool IsFinished();
        protected abstract bool IsInterrupted();

        protected virtual float CalculateCooldownFactor()
        {
            return 1f;
        }

        protected virtual bool ShouldApplyCooldownOnUse()
        {
            return true;
        }

        private bool CanBeUsed(Character target, out string failureReason)
        {
            foreach (var c in GetUseConditions())
            {
                if (!c.Item1(caster, vessel, target))
                {
                    failureReason = c.Item2;

                    // If we're only determining viability for the very first usage and we're returning false, this component is now redundant and should remove itself.
                    if (firstUse)
                    {
                        // Honestly this isn't really an ideal way to handle things from a design perspective, as it's weird having to instantiate this
                        // component to check whether it's allowed to exist and then having it kill itself if it decides not to...
                        // But it's self-contained, and not static, so that's a plus!
                        // I think the thing is the condition checks *have to be* on the instance because for both Control Chant and Twin Spell, they can be used additional times,
                        // but successive usage depends on conditions determined by instance variables (e.g. how many targets have I twinned to)
                        // I could have UseSkillPatch handle destruction of hte unnecessary extension on returning false here, but since it doesn't "add" the component itself,
                        // again it's preferable to have the instantiation+destruction be contained within the extension system itself. Which probs means the ExtensionManager
                        // handling it. Food for thought.
                        Destroy(this);
                    }
                    
                    return false;
                }
            }
        
            failureReason = null;
            firstUse = false;
            return true;
        }

        /* Code to execute when the skill has passed CanBeUsed and is now being triggered against the given target. */
        public bool UseSkill(Character target, out string failureReason)
        {
            if (!CanBeUsed(target, out failureReason))
                return false;

            failureReason = null;

            ApplySkill(target);

            if (ShouldApplyCooldownOnUse())
            {
                if (caster.MySkills.isPlayer) Main.Log.LogInfo(skill.SkillName + " applying cooldown on use.");
                TriggerCooldown();
            }

            return true;
        }


        public virtual void TriggerCooldown()
        {
            if (coolingDown) return;

            caster.GetCooldownManager().AddCooldown(skill, CalculateCooldownFactor());
            coolingDown = true;
        }
    }

}
