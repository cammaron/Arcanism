using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;


namespace Arcanism.Patches
{
    public static class LootTableExtensions
    {
        public const string INIT_LOOT_TABLE_METHOD = "InitLootTable";
        public static void RegenerateLoot(this LootTable lt, float lootRate = 1f)
        {
            var origServerLootRate = GameData.ServerLootRate;
            GameData.ServerLootRate = lootRate;
            Traverse.Create(lt).Method(INIT_LOOT_TABLE_METHOD).GetValue();
            GameData.ServerLootRate = origServerLootRate;
        }
    }
    /* Delegating bless/quality determination to a new MonoBehaviour so I can keep stateful data without guesswork */
    [HarmonyPatch(typeof(LootTable), LootTableExtensions.INIT_LOOT_TABLE_METHOD)]
    public class LootTable_InitLootTable
    {
        static void Prefix(LootTable __instance)
        {
            var helper = __instance.gameObject.GetOrAddComponent<LootHelper>();
            helper.lootTable = __instance;
            helper.PopulateExtraItems();
        }

        static void Postfix(LootTable __instance)
        {
            __instance.special = false;
            var stats = __instance.GetComponent<Stats>();
            if (stats == null || __instance.ActualDrops == null) return;

            // Replace treasure maps with whichever piece is most appropriate based on level
            var mapPieceIds = new List<string>() { ItemId.MAP_PIECE_1.Id(), ItemId.MAP_PIECE_2.Id(), ItemId.MAP_PIECE_3.Id(), ItemId.MAP_PIECE_4.Id() };
            for(var i = 0; i < __instance.ActualDrops.Count; i ++)
            {
                var drop = __instance.ActualDrops[i];
                if (mapPieceIds.Contains(drop.Id))
                {
                    if (stats.Level <= 9)
                        __instance.ActualDrops[i] = GameData.ItemDB.GetItemByID(ItemId.MAP_PIECE_1);
                    else if (stats.Level <= 19)
                        __instance.ActualDrops[i] = GameData.ItemDB.GetItemByID(ItemId.MAP_PIECE_2);
                    else if (stats.Level <= 29)
                        __instance.ActualDrops[i] = GameData.ItemDB.GetItemByID(ItemId.MAP_PIECE_3);
                    else
                        __instance.ActualDrops[i] = GameData.ItemDB.GetItemByID(ItemId.MAP_PIECE_4);
                }
            }
        }
    }
}