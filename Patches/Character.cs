using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(Character), "Awake")]
    public class Character_Awake
    {
        static void Postfix(Character __instance)
        {
            CooldownManager.AddToCharacter(__instance); // I really, really like that when I type CooldownManager into my IDE, I autocomplete it by typing CoolMan and pressing ctrl + space. It's the little things.
        }
    }
}
