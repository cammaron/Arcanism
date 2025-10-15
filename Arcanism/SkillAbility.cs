using System;
using System.Collections.Generic;
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

            float cooldown = Skill.Cooldown * cooldownFactor; // unlike spells, skills have the * 60 baked into their cooldown stat already
            GameData.HKMngr.GetHotkeysForSkill(Skill).ForEach(hk =>
            {
                if (hk.Cooldown < 20f)
                    hk.Cooldown = cooldown;
            });
        }
    }


}
