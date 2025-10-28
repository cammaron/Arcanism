using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Arcanism.Patches
{
    /*
    [HarmonyPatch]
    static class LootRarityPatches_ScaleStatFields
    {

        static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("Recks.Tools.Patch_ItemDatabase_Start_CloneInject");
            var target = AccessTools.DeclaredMethod(type, "ScaleStatFields");
            return target;
        }

        static void Prefix(Item target, float mul, ref float __state)
        {
            __state = target.WeaponDly;
        }

        static void Postfix(Item target, float mul, ref float __state)
        {
            target.WeaponDly = __state; // for now, let's not even change attack speed, because DPS already gets a buff via the weapon's raw hit damage.
        }
    }

    /* Fix variable name bug in scaleprice, whilst also changing from linear to exponential scaling based on rarity multiplier 
    //[HarmonyPatch(]
    class LootRarityPatches_TryScalePrice
    {
        static bool Prefix(Item v, float mul)
        {
            // ORDER:
            // Arcanism has to load and add items to the databse BEFORE loot rarity runs
            // but Arcanism loads the loot rarity patch, which also needs to happen before loot rarity runs,  but loot rarity needs to be LOADED before it can do it
             
            v.ItemValue = Mathf.RoundToInt(v.ItemValue * Mathf.Pow(mul, 3));
            return false;
        }
    }

    /* Prevent sims getting recks rarity items when finding low level items 
    [HarmonyPatch(typeof(SimPlayer), "FindLowLevelItem")]
    /* And also prevent sims getting recks items offline  
    [HarmonyPatch(typeof(SimPlayer), nameof(SimPlayer.CheckGearProgress))]
    class SimPlayer_FindLowLevelItem
    {
        static void Prefix(ref Item[] __state)
        {
            __state = GameData.ItemDB.ItemDB;
            GameData.ItemDB.ItemDB = ItemDatabase_Start.GetItemDBWithoutLootRarity();
        }

        static void Postfix(Item[] __state)
        {
            GameData.ItemDB.ItemDB = __state;
        }
    }

    
    [HarmonyPatch(typeof(SimPlayer), nameof(SimPlayer.CheckGearProgress))]
    class SimPlayer_CheckGearProgress
    {
        static void Prefix(ref Item[] __state)
        {
            __state = GameData.ItemDB.ItemDB;
            GameData.ItemDB.ItemDB = ItemDatabase_Start.GetItemDBWithoutLootRarity();
        }

        static void Postfix(Item[] __state)
        {
            GameData.ItemDB.ItemDB = __state;
        }
    }*/
}
