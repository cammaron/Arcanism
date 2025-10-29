using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;


namespace Arcanism.Patches
{

    /* Delegating bless/quality determination to a new MonoBehaviour so I can keep stateful data without guesswork */
    [HarmonyPatch(typeof(LootTable), "InitLootTable")]
    public class LootTableInitLootTablePatch
    {

        static void Prefix(LootTable __instance)
        {
            var helper = __instance.gameObject.AddComponent<LootHelper>();
            helper.lootTable = __instance;

            helper.PopulateExtraItems();
        }

        static void Postfix(LootTable __instance)
        {
            __instance.special = __instance.GetComponent<LootHelper>().UpdateLootQuality();
        }
    }
}