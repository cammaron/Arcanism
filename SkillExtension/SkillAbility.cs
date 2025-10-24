using System;
using System.Collections;
using UnityEngine;

namespace Arcanism
{
    abstract class SkillAbility : MonoBehaviour, HotkeyAbility
    {

        protected abstract UseSkill SkillSource { get; }

        protected abstract Skill Skill { get; }

        public bool IsPlayer
        {
            get
            {
                return this.SkillSource.isPlayer;
            }
        }

        public virtual void ApplyCooldown(float cooldownFactor = 1f)
        {
            if (!IsPlayer) return;

            var coolMan = SkillSource.GetComponent<CooldownManager>(); // Cool... man.
            if (coolMan != null)
                coolMan.AddCooldown(Skill, cooldownFactor);
        }
    }


}
