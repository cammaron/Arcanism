using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;

namespace Arcanism
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    class Main : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raventhe.erenshor.Arcanism";
        public const string PLUGIN_NAME = "Arcanism";
        public const string PLUGIN_VERSION = "1.0.0";

        internal static new ManualLogSource Log;

        public void Awake()
        {
            Log = base.Logger;

            Logger.LogInfo("Arcanism initialising");

            new Harmony(PLUGIN_GUID).PatchAll();
        }
    }

    public static class Extensions
    {
        public static List<Hotkeys> GetHotkeysForSpell(this HotkeyManager hkm, Spell spell) => hkm.AllHotkeys.FindAll(hk => hk.AssignedSpell == spell || hk.AssignedItem?.MyItem?.ItemEffectOnClick == spell);
        public static List<Hotkeys> GetHotkeysForSkill(this HotkeyManager hkm, Skill skill) => hkm.AllHotkeys.FindAll(hk => hk.AssignedSkill == skill || hk.AssignedItem?.MyItem?.ItemSkillUse == skill);

        public static bool KnowsSkill(this UseSkill mySkills, string skillId) => mySkills.KnownSkills.Find(ks => ks.Id == skillId) != null;
        public static bool KnowsSkill(this UseSkill mySkills, Skill skill) => mySkills.KnowsSkill(skill.Id);
    }
}
