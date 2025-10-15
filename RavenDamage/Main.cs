using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace RavenDamage
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    class Main : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raventhe.erenshor.RavenDamage";
        public const string PLUGIN_NAME = "Raven Damage";
        public const string PLUGIN_VERSION = "1.0.0";

        internal static new ManualLogSource Log;

        public void Awake()
        {
            Log = base.Logger;

            Logger.LogInfo("RavenDamage initialising");

            // Define a damage text component: whenever the DmgPop is used (i suspect they're pooled, but use = LoadInfo), spawn one of these


            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }
    }
}
