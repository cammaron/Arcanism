using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;

namespace BigDamage
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    class Main : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raventhe.erenshor.BigDamage";
        public const string PLUGIN_NAME = "BigDamage";
        public const string PLUGIN_VERSION = "1.0.0";

        internal static ConfigEntry<float> maxDistance;
        internal static ConfigEntry<float> baseSizeFactor;
        internal static ConfigEntry<float> distanceToDoubleSize;
        internal static ConfigEntry<bool> enableColourChange;
        internal static Color physColour;
        internal static Color magColour;
        internal static Color eleColour;
        internal static Color voidColour;
        internal static Color poisonColour;
        internal static ManualLogSource Log;

        private const float DEFAULT_MAX_DISTANCE = 55f;
        private const float DEFAULT_SIZE_FACTOR = 2.5f;
        private const float DEFAULT_DISTANCE_TO_DOUBLE_SIZE = 17f;

        private const string DEFAULT_COLOUR_PHYSICAL = "#8c8c8c";
        private const string DEFAULT_COLOUR_MAGIC = "#b500a6";
        private const string DEFAULT_COLOUR_ELEMENTAL = "#54ffeb";
        private const string DEFAULT_COLOUR_VOID = "#35026b";
        private const string DEFAULT_COLOUR_POISON = "#8cff50";

        public void Awake()
        {
            Log = base.Logger;

            Logger.LogInfo("Initialising.");
            maxDistance = Config.Bind("General", "maxDistance", DEFAULT_MAX_DISTANCE, $"The distance a damage popup can be at before being hidden from the player. Higher = view hits further away. Vanilla default is 20. Mod default is {DEFAULT_MAX_DISTANCE}.");
            baseSizeFactor = Config.Bind("General", "baseSizeFactor", DEFAULT_SIZE_FACTOR, $"An initial multiplier to damage popup font size. Just makin' it bigger regardless.. Mod default is {DEFAULT_SIZE_FACTOR}.");
            distanceToDoubleSize = Config.Bind("General", "distanceToDoubleSize", DEFAULT_DISTANCE_TO_DOUBLE_SIZE, $"Every X units of distance, the damage popup text scale is doubled. Scaling over distance is the main purpose of this mod, as damage occurring further away needs to be larger in world space to be visible. Mod default is {DEFAULT_DISTANCE_TO_DOUBLE_SIZE}.");
            enableColourChange = Config.Bind("General", "enableColourChange", true, $"{PLUGIN_NAME} attempts to increase damage readability by removing the colour gradient and choosing simple, bold colours. Set this to false if you prefer the original style.");

            ColorUtility.TryParseHtmlString(Config.Bind("Damage_Colours", "physical", DEFAULT_COLOUR_PHYSICAL, $"Text colour for PHYSICAL damage. Default ${DEFAULT_COLOUR_PHYSICAL} (medium grey)").Value, out physColour);
            ColorUtility.TryParseHtmlString(Config.Bind("Damage_Colours", "magical", DEFAULT_COLOUR_MAGIC, $"Text colour for MAGIC damage. Default ${DEFAULT_COLOUR_MAGIC} (light magenta)").Value, out magColour);
            ColorUtility.TryParseHtmlString(Config.Bind("Damage_Colours", "elemental", DEFAULT_COLOUR_ELEMENTAL, $"Text colour for ELEMENTAL damage. Default ${DEFAULT_COLOUR_ELEMENTAL} (sky blue)").Value, out eleColour);
            ColorUtility.TryParseHtmlString(Config.Bind("Damage_Colours", "void", DEFAULT_COLOUR_VOID, $"Text colour for VOID damage. Default ${DEFAULT_COLOUR_VOID} (deep purple)").Value, out voidColour);
            ColorUtility.TryParseHtmlString(Config.Bind("Damage_Colours", "poison", DEFAULT_COLOUR_POISON, $"Text colour for POISON damage. Default ${DEFAULT_COLOUR_POISON} (acid green)").Value, out poisonColour);

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }
    }
}
