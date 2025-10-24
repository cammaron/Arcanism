using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;

namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(HotkeyManager), nameof(HotkeyManager.ResetAllCooldowns))]
    public class HotkeyManager_ResetAllCooldowns
    {
        static void Postfix()
        {
            GameData.PlayerControl.Myself.GetCooldownManager().ResetAllCooldowns();
        }
    }
}
