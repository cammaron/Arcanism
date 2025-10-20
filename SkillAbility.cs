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

            float cooldown = Skill.Cooldown * cooldownFactor; // unlike spells, skills have the * 60 baked into their cooldown stat already

            // This is suuuuuch a hack but when I originally wrote this I didn't realise Hotkeys.DoHotkeyTask would override my manually applied cooldowns whether returning true or false from DoSkill,
            // and I am *absolutely* over making this mod by now and just want to play the damn game, so... eat lead, proper designs!
            SkillSource.StartCoroutine(DoCooldownNextFrame(cooldown)); // aaaaaand to make it even more hacky, TwinSpell gets destroyed when the cast finishes so I also have to spawn the coroutine on another game object before it dies. 
        }

        private IEnumerator DoCooldownNextFrame(float cooldown)
        {
            yield return new WaitForSeconds(0.2f);

            var hks = GameData.HKMngr.GetHotkeysForSkill(Skill);
            hks.ForEach(hk =>
            {
                if (hk.Cooldown <= 20f || cooldown < hk.Cooldown)
                    hk.Cooldown = cooldown;
            });
        }
    }


}
