#pragma warning disable CS0618 // to shut up the Harmony.DEBUG deprecation despite the alternative method mentioned in API docs not seeming to be available in the package
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Arcanism.Patches;
using Arcanism.Skills;
using BepInEx.Configuration;
using UnityEngine.Networking;
using HarmonyLib.Tools;

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
        internal static Dictionary<string, AudioClip> sfxByName;

        internal static ConfigEntry<float> auctionHouseGreedFactor;
        internal static ConfigEntry<int> minPricePerItemLevel;
        internal static ConfigEntry<float> minPriceLevelExponent;

        private const float DEFAULT_GREED_FACTOR = 1.4f;
        private const int DEFAULT_MIN_PRICE_PER_ITEM_LEVEL = 180;
        private const float DEFAULT_MIN_PRICE_LEVEL_EXPONENT = 1.04f;

        Harmony harmonyPatcher;

        public static Main Instance { get; private set; }

        public void Awake()
        {
            Instance = this;

            Log = base.Logger;

            Logger.LogInfo("Initialising.");

            auctionHouseGreedFactor = Config.Bind("Auction House Changes", "greedFactor", DEFAULT_GREED_FACTOR, $"This multiplies the asking price of all goods in the Auction House. Its purpose is to make it a little less easy to buy equipment rather than farming it. Arcanism's default is {DEFAULT_GREED_FACTOR}.");
            minPricePerItemLevel = Config.Bind("Auction House Changes", "minPricePerItemLevel", DEFAULT_MIN_PRICE_PER_ITEM_LEVEL, $"This, and the variable below, set a floor on base item prices for SimPlayer AH sale purposes, both to make the game more challenging and address pricing inconsistencies in some higher level items--especially if you're playing a lower level alt and the market is saturated with items you couldn't farm yet which were too easy to buy. This value sets an absolute base value per item level (e.g. a value of 200 per level on a level 10 item would cause a base price of 2000, then multiplied by Sim greed for AH sale). Arcanism's default is {DEFAULT_MIN_PRICE_PER_ITEM_LEVEL}.");
            minPriceLevelExponent= Config.Bind("Auction House Changes", "minPriceLevelExponent", DEFAULT_MIN_PRICE_LEVEL_EXPONENT, $"This is an additional exponential modifier per item level to ensure prices scale up as they should. If the minPricePerItemLevel is 200, item level is 10, and minPriveLevelExponent is 1.1, the item's base price will effectively be 200 * 10 * 1.1 ^ 10 = 5187. Don't change this unless you understand the maths -- but it's fine to set to 1 if you don't want the prices jacked up! Arcanism's default is {DEFAULT_MIN_PRICE_LEVEL_EXPONENT}.");

            LoadFiles();

#if DEBUG
            HarmonyFileLog.FileWriterPath = "D:\\Games\\HarmonyLog.txt";
            HarmonyFileLog.Enabled = true;
#else
            HarmonyFileLog.Enabled = false;
#endif

            harmonyPatcher = new Harmony(PLUGIN_GUID);
            harmonyPatcher.PatchAll();

            if (GameData.SkillDatabase != null && GameData.SkillDatabase.GetSkillByID(SkillDB_Start.CONTROL_CHANT_SKILL_ID) != null)
                ControlChant.CreateExtension(GameData.SkillDatabase.GetSkillByID(SkillDB_Start.CONTROL_CHANT_SKILL_ID));
            if (GameData.SkillDatabase != null && GameData.SkillDatabase.GetSkillByID(SkillDB_Start.TWIN_SPELL_SKILL_ID) != null)
                TwinSpell.CreateExtension(GameData.SkillDatabase.GetSkillByID(SkillDB_Start.TWIN_SPELL_SKILL_ID));


            // Normally this would run before all the things in the world have spawned, but on a hot reload, their Start/Awake patches won't run due to already being spawned, so manually re-adding destroyed components
            foreach (var stats in FindObjectsOfType<Stats>())
            {
                if (stats.Myself.isNPC)
                {
                    var npc = stats.Myself.MyNPC;
                    npc.gameObject.GetOrAddComponent<CharacterUI.CharacterHoverUI>();
                    var lootHelper = npc.gameObject.GetOrAddComponent<LootHelper>();
                    lootHelper.lootTable = npc.gameObject.GetComponent<LootTable>();
                    npc.gameObject.GetOrAddComponent<TheyMightBeGiants>();
                }

                var shield = stats.gameObject.GetOrAddComponent<SpellShieldVisual>();
                shield.stats = stats;
                shield.Init();
            }

            if (GameData.ItemDB != null && GameData.ItemDB.GetItemByID(ItemId.VIZIERS_LAMENT) != null) // if a custom Arcanism item exists in DB at this point, it means we've hot reloaded, so must recreate sets
                ItemDatabase_Start.RegisterSets(GameData.ItemDB);
        }

#if DEBUG
        /* bool done = false;
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
         }*/
#endif

        void OnDestroy()
        {
            Logger?.LogInfo("Destroying Arcanism.");
            DestroyAllOfType<CooldownManager>();
            DestroyAllOfType<ExtendedSkill>();
            DestroyAllOfType<CharacterUI.CharacterHoverUI>();
            DestroyAllOfType<ItemIconVisuals>();
            DestroyAllOfType<LootHelper>();
            DestroyAllOfType<TheyMightBeGiants>();
            DestroyAllOfType<SpellShieldVisual>();
            Logger?.LogInfo("Destroyed components. Unpatching.");

            BepInEx.Logging.Logger.Sources?.Remove(Log);
            harmonyPatcher?.UnpatchSelf();
        }

        void DestroyAllOfType<T>() where T : UnityEngine.Object
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll<T>())
                Destroy(obj);
        }

        public void LoadFiles()
        {
            itemSpriteById = new Dictionary<string, Sprite>();
            skillSpriteById = new Dictionary<string, Sprite>();
            miscSpritesByName = new Dictionary<string, Sprite>();
            sfxByName = new Dictionary<string, AudioClip>();

            string nestedAssetsPath = Path.Combine(Paths.PluginPath, "Arcanism", "Assets");
            string rootAssetsPath = Path.Combine(Paths.PluginPath, "Assets"); // check the root plugins folder in case Arcanism wasn't placed in its own folder
            string assetsPath;
            if (Directory.Exists(nestedAssetsPath))
                assetsPath = nestedAssetsPath;
            else if (Directory.Exists(rootAssetsPath))
                assetsPath = rootAssetsPath;
            else
            {
                Log.LogError($"Assets folder not found at {nestedAssetsPath} or {rootAssetsPath}. Ensure you've fully installed the mod!");
                return;
            }

            LoadSpriteSet(assetsPath, "Items", itemSpriteById);
            LoadSpriteSet(assetsPath, "Skills", skillSpriteById);
            LoadSpriteSet(assetsPath, "Misc", miscSpritesByName);
            StartCoroutine(LoadSounds(assetsPath));
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

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
                
                foreach(var id in ids)
                    map.Add(id, sprite);
            }
            Log.LogInfo($"Loaded {map.Count} sprites for {spriteSet}.");
        }

        private IEnumerator LoadSounds(string assetsPath)
        {
            string soundsPath = Path.Combine(assetsPath, "Sounds");
            string[] filePaths = Directory.GetFiles(soundsPath);
            Log.LogInfo("Found " + filePaths.Length + " sound files in path " + soundsPath);
            foreach (var filePath in filePaths)
            {
                string soundName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                AudioType audioType;
                
                if (extension.ToLower().EndsWith("wav")) // I am suuuuuure there's a better existing API for working out the audiotype automatically by looking at the file's encoding but I ain't got time to find that rn
                    audioType = AudioType.WAV;
                else 
                    audioType = AudioType.MPEG;

                var fileUri = "file:///" + filePath.Replace('\\', '/');
                using (var www = UnityWebRequestMultimedia.GetAudioClip(fileUri, audioType))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        sfxByName[soundName] = DownloadHandlerAudioClip.GetContent(www);
                        Log.LogInfo("Loaded sound file: " + soundName);
                    }
                    else
                        Log.LogError("Error occurred loading sound file at " + filePath + ". Response was " + www.result + " and error message was " + www.error);
                }
            }
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

        public static int DestroyAllChildren(this Transform t)
        {
            int count = t.childCount;
            for (var i = 0; i < t.childCount; i++)
            {
                GameObject.Destroy(t.GetChild(i).gameObject);
            }

            return count;
        }

        public static bool DestroyIfPresent<T>(this Transform t) where T : Component
        {
            var toBeEviscerated = t.GetComponent<T>();
            if (toBeEviscerated != null)
            {
                GameObject.Destroy(toBeEviscerated);
                return true;
            }

            return false;
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