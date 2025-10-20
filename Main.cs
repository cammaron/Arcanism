#pragma warning disable CS0618 // to shut up the Harmony.DEBUG deprecation despite the alternative method mentioned in API docs not seeming to be available in the package
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

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
                Logger.LogInfo("** SKIPPING LOGIN SCRENE **"); // code nixed from another plugin just for quick testing, not part of Arcanism
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

        public static List<Hotkeys> GetHotkeysForSpell(this HotkeyManager hkm, Spell spell) => hkm.AllHotkeys.FindAll(hk => hk.AssignedSpell == spell || hk.AssignedItem?.MyItem?.ItemEffectOnClick == spell);
        public static List<Hotkeys> GetHotkeysForSkill(this HotkeyManager hkm, Skill skill) => hkm.AllHotkeys.FindAll(hk => hk.AssignedSkill == skill || hk.AssignedItem?.MyItem?.ItemSkillUse == skill);

        public static bool KnowsSkill(this UseSkill mySkills, string skillId) => mySkills.KnownSkills.Find(ks => ks.Id == skillId) != null;
        public static bool KnowsSkill(this UseSkill mySkills, Skill skill) => mySkills.KnowsSkill(skill.Id);

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