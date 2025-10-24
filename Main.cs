#pragma warning disable CS0618 // to shut up the Harmony.DEBUG deprecation despite the alternative method mentioned in API docs not seeming to be available in the package
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Arcanism.Patches;

namespace Arcanism
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    class Main : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raventhe.erenshor.Arcanism";
        public const string PLUGIN_NAME = "Arcanism";
        public const string PLUGIN_VERSION = "1.0.0";

        internal static ManualLogSource Log;
        internal static Dictionary<string, Sprite> itemSpriteById;

        public void Awake()
        {
            Log = base.Logger;

            Logger.LogInfo("Initialising.");

            LoadSprites();
            new Harmony(PLUGIN_GUID).PatchAll();
        }

#if DEBUG
        bool done = false;
        void Update()
        {
            if (done) return;
            // Check if we are on the main menu before loading
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                Logger.LogInfo("** SKIPPING LOGIN SCREEN **"); // code nixed from another plugin just for quick testing, not part of Arcanism
                SceneManager.LoadScene("LoadScene"); // <- Boom, straight to character selection!
                Harmony.DEBUG = true;
                done = true;
            }
        }
#endif

        public static void LoadSprites()
        {
            itemSpriteById = new Dictionary<string, Sprite>();

            string nestedAssetsPath = Path.Combine(Paths.PluginPath, "Arcanism", "Assets");
            string rootAssetsPath = Path.Combine(Paths.PluginPath, "Assets"); // check the root plugins folder in case Arcanism wasn't placed in its own folder
            string assetsPath;
            if (Directory.Exists(nestedAssetsPath))
                assetsPath = nestedAssetsPath;
            else if (Directory.Exists(rootAssetsPath))
                assetsPath = rootAssetsPath;
            else
            {
                Main.Log.LogError($"Assets folder not found at {nestedAssetsPath} or {rootAssetsPath}. Ensure you've fully installed the mod!");
                return;
            }

            LoadSpriteSet(assetsPath, "Items", itemSpriteById);
        }

        private static void LoadSpriteSet(string assetsPath, string spriteSet, Dictionary<string, Sprite> map)
        {
            string spriteSetPath = Path.Combine(assetsPath, spriteSet);
            string[] filePaths = Directory.GetFiles(spriteSetPath);
            foreach (var filePath in filePaths)
            {
                string id = Path.GetFileNameWithoutExtension(filePath);
                byte[] imageData = File.ReadAllBytes(filePath);

                var texture = new Texture2D(default, default);
                texture.LoadImage(imageData);

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                map.Add(id, sprite);
            }
            Main.Log.LogInfo($"Loaded {map.Count} sprites for {spriteSet}.");
        }
    }

    public static class Extensions
    {

        // Helper methods for hotkey cooldown management -- there's a bit of a design inversion in Erenshor in which a *character* doesn't experience a cooldown on their spell/skill; instead any *hotkey in their bar*
        // which is assigned to that spell or skill gets cooldown applied to it, and separately, rather than 
        /*public static List<Hotkeys> GetHotkeysFor(this HotkeyManager hkm, Spell spell) => hkm.AllHotkeys.FindAll(hk => hk.AssignedSpell == spell || hk.AssignedItem?.MyItem?.ItemEffectOnClick == spell);
        public static List<Hotkeys> GetHotkeysFor(this HotkeyManager hkm, Skill skill) => hkm.AllHotkeys.FindAll(hk => hk.AssignedSkill == skill || hk.AssignedItem?.MyItem?.ItemSkillUse == skill);
        
        public static List<Hotkeys> AddCooldown(this HotkeyManager hkm, Character caster, Spell spell, float cooldownFactor = 1f)
        {
            var casterSkills = caster.MySkills;
            float cooldownReductionFactor = casterSkills != null ? 1f - (casterSkills.GetAscensionRank("7758218") * 0.1f) : 1f; // Arcanist Cooldown Reduction
            float cooldown = spell.Cooldown * 60f * cooldownFactor * cooldownReductionFactor;
            var hotkeys = hkm.GetHotkeysFor(spell);
            hotkeys.ForEach(hk => hk.AddCooldown(cooldown));
            return hotkeys;
        }

        public static List<Hotkeys> AddCooldown(this HotkeyManager hkm, Character caster, Skill skill, float cooldownFactor = 1f)
        {
            var casterSkills = caster.MySkills;
            float cooldown = skill.Cooldown * cooldownFactor;  // unlike spells, skills have the * 60 baked into their cooldown stat already, JUST to fuck with me
            var hotkeys = hkm.GetHotkeysFor(skill);
            hotkeys.ForEach(hk => hk.AddCooldown(cooldown));
            return hotkeys;
        }

        public static void AddCooldown(this Hotkeys hotkey, float cooldownDuration)
        {
            if (hotkey.Cooldown <= 20 || cooldownDuration < hotkey.Cooldown)
                hotkey.Cooldown = cooldownDuration;
        }*/
        
        public static float GetRelevantCooldown(this Hotkeys instance)
        {
            var coolMan = GameData.PlayerControl.Myself.GetComponent<CooldownManager>();
            if (coolMan == null) return 0f; // probably only possible very early on startup if execution order is such that this runs before Character.Awake

            if (instance.AssignedSkill != null)
                return coolMan.GetCooldown(instance.AssignedSkill);

            if (instance.AssignedSpell != null)
                return coolMan.GetCooldown(instance.AssignedSpell);

            // It *SEEMS* like the only time hotkeyed items have proper cooldown is when they're items that use spells, and the cooldown is set incidentally
            // by SpellVessel when it iterates HKs after casting a spell and finds a HK that has an item with the same spell.
            // So, I'll just make item cooldown=spell cooldown for now.
            if (instance.AssignedItem?.MyItem?.ItemEffectOnClick != null) 
                return coolMan.GetCooldown(instance.AssignedItem.MyItem.ItemEffectOnClick);

            return 0;
        }

        public static CooldownManager GetCooldownManager(this Character c)
        {
            return c.GetComponent<CooldownManager>();
        }

        public static bool KnowsSkill(this UseSkill mySkills, string skillId) => mySkills.KnownSkills.Find(ks => ks.Id == skillId) != null;
        public static bool KnowsSkill(this UseSkill mySkills, Skill skill) => mySkills.KnowsSkill(skill.Id);

        // Quick hack: wanna be able to manually specify colour of text for string popups. Popups are pooled and popIndex is incremented AFTER using one, so grab index and reference before,
        // run vanilla code which actually updates the DmgPop to world space+string, then return it.
        public static DmgPop GenPopupStringAndReturnPopup(this Misc misc, string msg, Transform t)
        {
            int popIndex = Traverse.Create(misc).Field<int>("popIndex").Value;
            var nextPopup = misc.DmgPopup[popIndex];
            misc.GenPopupString(msg, t);
            return nextPopup;
        }

        // NOTE: Removes from start line (inclusive) last of match lines (inclusive) -- i.e. all match line parameters will be removed.
        public static CodeMatcher RemovesLinesUntilMatchEndForward(this CodeMatcher matcher, params CodeMatch[] matches)
        {
            int startPos = matcher.Pos;
            matcher.MatchEndForward(matches);
            int endPos = matcher.Pos;
            int offset = endPos - startPos;
            matcher
                .Advance(-offset)
                .RemoveInstructions(offset + 1);

            return matcher;
        }

        public static CodeMatcher DebugPrint(this CodeMatcher matcher, string patchTitle)
        {
            Main.Log.LogInfo($"**************************************");
            Main.Log.LogInfo($"{patchTitle} patch code: ");
            foreach (var i in matcher.Instructions())
            {
                Main.Log.LogInfo(i.opcode + "    " + i.operand);
            }
            return matcher;
        }
    }
}
#pragma warning restore CS0618