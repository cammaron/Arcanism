using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    abstract class SpellAbility : MonoBehaviour, HotkeyAbility
    {
        protected abstract CastSpell SpellSource { get; }
        protected abstract SpellVessel Vessel { get; }
        protected abstract UseSkill CasterSkills { get; }

        public bool IsPlayer
        {
            get
            {
                return this.SpellSource.isPlayer;
            }
        }

        public void EndSpell(float cooldownFactor)
        {
            GameData.CB.CloseBar();
            Vessel.EndSpellNoCD(); // // skipping the standard cooldown code because it applies damage multi as a factor to cooldown, which... complicates everything
            ApplyCooldown(cooldownFactor);
        }

        public virtual void ApplyCooldown(float cooldownFactor = 1f)
        {
            if (!IsPlayer) return;

            float cooldownReductionFactor = CasterSkills != null ? 1f - (CasterSkills.GetAscensionRank("7758218") * 0.1f) : 1f; // Arcanist Cooldown Reduction
            float cooldown = Vessel.spell.Cooldown * 60f * cooldownFactor * cooldownReductionFactor; 
            GameData.HKMngr.GetHotkeysForSpell(Vessel.spell).ForEach(hk => 
            {
                hk.Cooldown = cooldown;
            });
        }
    }


}
