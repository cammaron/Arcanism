using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;

namespace Arcanism.Patches
{
    // To make Hotkeys work with the CooldownManager, I don't really need to mess with their internal logic that sets their cooldowns e.g. on assigning spell to hotkey.
    // I'll simply disregard their internal variable, overriding their display and whether click is enabled based on the cooldown in the manager.
    [HarmonyPatch(typeof(Hotkeys), "Update")]
    public class Hotkeys_Update
    {
        static void Postfix(Hotkeys __instance, Text ___MyHKText, Color32 ___greyOut, string ___savedStr)
        {
            float cooldown = __instance.GetRelevantCooldown();

            if (cooldown > 0f)
            {
                // vanilla rounds to int, but Ceil works much better for cooldowns because the int represents the "current second ticking down".
                // Practically, it will now say "1" when you're anywhere from 0.0 - 1.0, and then straight to off cooldown --
                // with Round, it will say 1 from 0.5 - 1.49, and actually say 0 for half a second below 0.5, despite still being on cooldown
                ___MyHKText.text = Mathf.CeilToInt(cooldown).ToString();
                __instance.MyImage.color = ___greyOut;
            }
            else
            {
                ___MyHKText.text = ___savedStr;
                __instance.MyImage.color = Color.white;
            }
        }
    }

    /* Just prevent hotkeys working if their relevant CD is active, otherwise force their internval var to 0 and let them to their business firing off skills and so on */
    [HarmonyPatch(typeof(Hotkeys), "DoHotkeyTask")]
    public class Hotkeys_DoHotkeyTask
    {
        static bool Prefix(Hotkeys __instance)
        {
            if (__instance.GetRelevantCooldown() > 0) return false;
            __instance.Cooldown = 0;
            return true;
        }
    }
}
