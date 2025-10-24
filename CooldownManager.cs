using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    /*
     *  CooldownManager is to fix a design inversion re separation of concerns in Erenshor's base code in which cooldown on a spell, skill or item is not a property of the caster, 
     *  but of individual hotkeys which have their own cooldown variable.
     *  
     *  The idea is to unify cooldown state and, in another patch, change hotkeys to display their cooldown--and be enabled or disabled--based on this manager for each Character. This way,
     *  when an ability is used, it doesn't have to know anything about hotkeys -- they're just a UI element, and will sort themselves out, however many there may or may not be.
     *  
     *  This also removes the need to do things like starting cooldown whenever you add a hotkey to the bar -- I presume that was originally a workaround to prevent somebody from simply clearing a hotkey and re-assigning
     *  the ability and thus erasing cooldown record, considering the hotkey itself is the thing responsible for it.
     *  
     *  It ALSO also means the logic can be applied universally across player, simplayers, and enemies -- since NPCs don't have hotkeys, theirs was all handled in its own place before...
     */
    public class CooldownManager : MonoBehaviour
    {
        public enum CooldownType
        {
            SPELL,
            SKILL,
            //ITEM
        }

        // These are maps not of "cooldown duration remaining" but of actual fixed timestamps reflecting the point at which cooldown would end
        // This allows us to avoid having an arbitrary update method iterating across aaaaaall cooldowns and deducting them toward zero.
        private Dictionary<CooldownType, Dictionary<string, float>> cooldownByIdByCooldownType = new Dictionary<CooldownType, Dictionary<string, float>>();

        private Character character;

        public static CooldownManager AddToCharacter(Character c)
        {
            var manager = c.gameObject.AddComponent<CooldownManager>();
            manager.character = c;
            return manager;
        }

        public float AddCooldown(Skill skill, float cooldownFactor = 1f, bool allowOverride = false)
        {
            // skill cooldowns as measured in 60ths of seconds, as if cd is 1s the value will be 60, so... gotta divide.
            if (character.MySkills.isPlayer) Main.Log.LogInfo($"CooldownManager: AddCoolown called for skill {skill.SkillName}.");
            return AddCooldown(CooldownType.SKILL, skill.Id, (skill.Cooldown / 60f) * cooldownFactor, allowOverride);
        }

        public float AddCooldown(Spell spell, float cooldownFactor = 1f, bool allowOverride = false)
        {
            float additionalCooldownFactor = 1f;

            if (character.MySkills != null)
                additionalCooldownFactor -= character.MySkills.GetAscensionRank("7758218") * 0.1f; // Arcanist Cooldown Reduction

            float cooldown = spell.Cooldown * cooldownFactor * additionalCooldownFactor;
            return AddCooldown(CooldownType.SPELL, spell.Id, cooldown);
        }

        /*public float AddCooldown(Item item, float cooldownFactor = 1f)
        {
            if (item.ItemSkillUse != null)
                return AddCooldown(CooldownType.ITEM, item.Id, item.ItemSkillUse.Cooldown * cooldownFactor);
            if (item.ItemEffectOnClick != null)
                return AddCooldown(CooldownType.ITEM, item.Id, item.ItemEffectOnClick.Cooldown * cooldownFactor);
            return 0;
        }*/

        private float AddCooldown(CooldownType type, string id, float durationInSeconds, bool allowOverride = false)
        {
            float proposedEndTime = Time.realtimeSinceStartup + durationInSeconds;

            var map = GetCooldownsByType(type);

            if (!allowOverride && map.TryGetValue(id, out float existingEndTime) && existingEndTime > proposedEndTime) // Don't allow overriding a longer existing cooldown (unless allowOverride explicitly)
                return existingEndTime;
            if (character.MySkills.isPlayer) Main.Log.LogInfo($"CooldownManager: Adding {durationInSeconds}s cooldown for {id}");
            map[id] = proposedEndTime;
            return proposedEndTime;
        }

        private Dictionary<string, float> GetCooldownsByType(CooldownType type)
        {
            if (!cooldownByIdByCooldownType.TryGetValue(type, out var map))
            {
                map = new Dictionary<string, float>();
                cooldownByIdByCooldownType.Add(type, map);
            }

            return map;
        }

        // Whilst dealing w/ timestamps internally, all methods return remaining cooldown duration in seconds (to a min of 0), consistent with the rest of the codebase
        public float GetCooldown(Skill skill) => skill == null ? 0 : GetCooldown(CooldownType.SKILL, skill.Id);
        public float GetCooldown(Spell spell) => spell == null ? 0 : GetCooldown(CooldownType.SPELL, spell.Id);
        //public float GetCooldown(Item item) => item == null ? 0 : GetCooldown(CooldownType.ITEM, item.Id); // doesn't really matter whether it's for a skill or spell, or the item doesn't even have one -- result is same.

        public float GetCooldown(CooldownType type, string id)
        {
            var map = GetCooldownsByType(type);
            if (!map.TryGetValue(id, out float cooldownEndTime))
                return 0;

            if (Time.realtimeSinceStartup >= cooldownEndTime)
                return 0f;

            return cooldownEndTime - Time.realtimeSinceStartup;
        }

        public bool IsReady(Skill skill) => GetCooldown(skill) <= 0;
        public bool IsReady(Spell spell) => GetCooldown(spell) <= 0;
        //public bool IsReady(Item item) => GetCooldown(item) <= 0;

        public void ResetCooldown(Skill skill) => ResetCooldown(CooldownType.SKILL, skill.Id);
        public void ResetCooldown(Spell spell) => ResetCooldown(CooldownType.SPELL, spell.Id);
        //public void ResetCooldown(Item item) => ResetCooldown(CooldownType.ITEM, item.Id);

        public void ResetCooldown(CooldownType type, string id)
        {
            var map = GetCooldownsByType(type);
            map[id] = 0;
        }

        public void ResetAllCooldowns()
        {
            cooldownByIdByCooldownType.Clear();
        }
    }
}
