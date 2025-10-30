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
using Arcanism.Skills;

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
        internal static Dictionary<string, Sprite> skillSpriteById;
        internal static Dictionary<string, Sprite> miscSpritesByName;

        Harmony harmonyPatcher;

        public void Awake()
        {
            Log = base.Logger;

            Logger.LogInfo("Initialising.");

            LoadSprites();
            harmonyPatcher = new Harmony(PLUGIN_GUID);
            harmonyPatcher.PatchAll();

            if (GameData.SkillDatabase != null && GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.CONTROL_CHANT_SKILL_ID) != null)
                ControlChant.CreateExtension(GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.CONTROL_CHANT_SKILL_ID));
            if (GameData.SkillDatabase != null && GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.TWIN_SPELL_SKILL_ID) != null)
                TwinSpell.CreateExtension(GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.TWIN_SPELL_SKILL_ID));

            // This is to ensure any existing NPCs have the requisite hover UI even after a hot reload
            foreach(var npc in FindObjectsOfType<NPC>())
            {
                npc.gameObject.GetOrAddComponent<CharacterHoverUI>();
                var lootHelper = npc.gameObject.GetOrAddComponent<LootHelper>();
                lootHelper.lootTable = npc.GetComponent<LootTable>();
                lootHelper.UpdateLootQuality();
            }
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
        void OnDestroy()
        {
            Logger?.LogInfo("Destroying Arcanism.");
            DestroyAllOfType<CooldownManager>();
            DestroyAllOfType<ExtendedSkill>();
            DestroyAllOfType<CharacterHoverUI>();
            DestroyAllOfType<ItemIconVisuals>();
            DestroyAllOfType<LootHelper>();

            BepInEx.Logging.Logger.Sources?.Remove(Log);
            harmonyPatcher?.UnpatchSelf();
        }

        void DestroyAllOfType<T>() where T : UnityEngine.Object
        {
            foreach (var obj in FindObjectsOfType<T>())
                Destroy(obj);
        }

        public static void LoadSprites()
        {
            itemSpriteById = new Dictionary<string, Sprite>();
            skillSpriteById = new Dictionary<string, Sprite>();
            miscSpritesByName = new Dictionary<string, Sprite>();

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
            LoadSpriteSet(assetsPath, "Skills", skillSpriteById);
            LoadSpriteSet(assetsPath, "Misc", miscSpritesByName);
        }

        private static void LoadSpriteSet(string assetsPath, string spriteSet, Dictionary<string, Sprite> map)
        {
            string spriteSetPath = Path.Combine(assetsPath, spriteSet);
            string[] filePaths = Directory.GetFiles(spriteSetPath);
            foreach (var filePath in filePaths)
            {
                string[] ids = Path.GetFileNameWithoutExtension(filePath).Split('-'); // to re-use sprite for multiple IDs, e.g. 90000-90001.png
                byte[] imageData = File.ReadAllBytes(filePath);

                var texture = new Texture2D(default, default);
                texture.LoadImage(imageData);

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                
                foreach(var id in ids)
                    map.Add(id, sprite);
            }
            Main.Log.LogInfo($"Loaded {map.Count} sprites for {spriteSet}.");
        }
    }

    public static class Extensions
    {

        
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
            // Everyone needs a cool man. Previously I patched the AddComponent into Character.Awake but to fix issues w/ reloading plugin at runtime, now doing it here
            return CooldownManager.GetOrCreate(c);
        }

        public static bool IsDead(this Character c)
        {
            return c.MyStats.GetCurrentHP() <= 0;
        }

        public static bool KnowsSkill(this UseSkill mySkills, string skillId) => mySkills.KnownSkills.Find(ks => ks.Id == skillId) != null;
        public static bool KnowsSkill(this UseSkill mySkills, Skill skill) => mySkills.KnowsSkill(skill.Id);

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var c = gameObject.GetComponent<T>();
            if (c == null) c = gameObject.AddComponent<T>();
            return c;
        }

        /* Quick hack: wanna be able to manually tweak DmgPops without messing up the pool, so this clones and returns one which gets cleaned up after.
         * Obviously, only for sporadic use considering from a performance perspective it defeats the purpose of the pool.
        */
        public static DmgPop CreateDmgPopClone(this Misc misc, string msg, Transform t)
        {
            int popIndex = Traverse.Create(misc).Field<int>("popIndex").Value;
            var nextPopup = misc.DmgPopup[popIndex];
            
            var clone = GameObject.Instantiate(nextPopup);
            clone.gameObject.SetActive(true);
            clone.LoadInfo(msg, t);
            clone.transform.position = t.transform.position + Vector3.up * 5f;
            clone.Num.fontSize = 4;
            clone.speed = 2.5f;
            clone.Num.fontStyle = TMPro.FontStyles.Normal;
            GameObject.Destroy(clone.gameObject, 2);
            
            return clone;
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