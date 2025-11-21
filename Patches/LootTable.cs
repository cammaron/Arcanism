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
        static bool Prefix(LootTable __instance, ref List<int> ___ActualDropsQual, ref bool ___special,
			ref List<Item> ___GuaranteeOneDrop,
			ref List<Item> ___ActualDrops,
			ref List<Item> ___LegendaryDrop,
			ref List<Item> ___RareDrop,
			ref List<Item> ___UncommonDrop,
			ref List<Item> ___CommonDrop,
			ref int ___nonCommonDropDone,
			ref int ___MaxNonCommonDrops,
			ref int ___MaxNumberDrops,
			ref int ___MinGold,
			ref int ___MaxGold,
			ref int ___MyGold
			)
		{
			var helper = __instance.gameObject.GetOrAddComponent<LootHelper>();
            helper.lootTable = __instance;
            helper.PopulateExtraItems();

			var npc = __instance.GetComponent<NPC>();
			if (npc == null || !System.Enum.TryParse(npc.NPCName.ToUpper().Replace(' ', '_'), out NpcName npcName))
				return true; // go on normally

			Main.Log.LogInfo("Generating loot for " + npcName);
			foreach (var drop in ___GuaranteeOneDrop) Main.Log.LogInfo("Guarantee one drop: " + drop.ItemName); 
			foreach (var drop in ___LegendaryDrop) Main.Log.LogInfo("Legendary drop: " + drop.ItemName);
			foreach (var drop in ___RareDrop) Main.Log.LogInfo("Rare drop: " + drop.ItemName);
			foreach (var drop in ___UncommonDrop) Main.Log.LogInfo("Uncommon drop: " + drop.ItemName);
			foreach (var drop in ___CommonDrop) Main.Log.LogInfo("Common drop: " + drop.ItemName);

			// TEMP HACK: FIXME! Added this to check my loot is actually working
			___ActualDropsQual.Clear();
			___special = false;
			if (___GuaranteeOneDrop.Count > 0)
			{
				var gdrop = ___GuaranteeOneDrop[Random.Range(0, ___GuaranteeOneDrop.Count)];
				___ActualDrops.Add(gdrop);
				Main.Log.LogInfo("Added guaranteed drop " + gdrop.ItemName);
			}
			Main.Log.LogInfo("Max drops: " + ___MaxNumberDrops + " (effectively this + 1 because <=)");
			Main.Log.LogInfo("Max uncommon drops: " + ___MaxNonCommonDrops);
			Main.Log.LogInfo("Loot rate: " + GameData.ServerLootRate);
			for (int i = 0; i <= ___MaxNumberDrops; i++)
			{
				Main.Log.LogInfo("Drop " + i + ": Uncommon drops done: " + ___nonCommonDropDone + "/" + ___MaxNonCommonDrops);
				___MyGold = Random.Range(___MinGold, ___MaxGold);
				float num = Random.Range(0f, 100f);
				Main.Log.LogInfo("Rolled " + num + " / " + GameData.ServerLootRate + " = " + (num / GameData.ServerLootRate));
				num /= GameData.ServerLootRate;
				
				if (num <= 2.3f && ___LegendaryDrop.Count > 0 && ___nonCommonDropDone < ___MaxNonCommonDrops)
				{
					var legDrop = ___LegendaryDrop[Random.Range(0, ___LegendaryDrop.Count)];
					___ActualDrops.Add(legDrop);
					___nonCommonDropDone++;
					Main.Log.LogInfo("Added legendary drop " + legDrop.ItemName);
				}
				else if (num <= 7f && ___RareDrop.Count > 0 && ___nonCommonDropDone < ___MaxNonCommonDrops)
				{
					var rareDrop = ___RareDrop[Random.Range(0, ___RareDrop.Count)];
					___ActualDrops.Add(rareDrop);
					___nonCommonDropDone++;
					Main.Log.LogInfo("Added rare drop " + rareDrop.ItemName);
				}
				else if (num <= 15f && ___UncommonDrop.Count > 0 && ___nonCommonDropDone < ___MaxNonCommonDrops)
				{
					var uncdrop = ___UncommonDrop[Random.Range(0, ___UncommonDrop.Count)];
					___ActualDrops.Add(uncdrop);
					___nonCommonDropDone++;
					Main.Log.LogInfo("Added uncommon drop " + uncdrop.ItemName);
				}
				else if (num <= 70f && ___CommonDrop.Count > 0)
				{
					if (Random.Range(0, 10) > 8)
					{
						var comdrop = GameData.GM.CommonWorldItems[Random.Range(0, GameData.GM.CommonWorldItems.Count)];
						___ActualDrops.Add(comdrop);
						Main.Log.LogInfo("Added world common drop " + comdrop.ItemName);
					}
					else
					{
						var comdrop = ___CommonDrop[Random.Range(0, ___CommonDrop.Count)];
						___ActualDrops.Add(comdrop);
						Main.Log.LogInfo("Added personal common drop " + comdrop.ItemName);
					}
				}
			}
			Stats component = __instance.GetComponent<Stats>();
			float serverLootRate = GameData.ServerLootRate;
			if (component != null && component.Level > 15 && Random.value < 0.001f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.Sivak);
				___special = true;
			}
			if (Random.value < 0.005f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.WorldDropMolds[Random.Range(0, GameData.GM.WorldDropMolds.Count)]);
				___special = true;
			}
			if (Random.value < 0.0125f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.Maps[Random.Range(0, GameData.GM.Maps.Count)]);
				___special = true;
			}
			if (component != null && component.Level > 12 && Random.value < 0.002f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.XPPot);
				___special = true;
			}
			if (component != null && component.Level > 20 && Random.value < 0.008f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.InertDiamond);
				___special = true;
			}
			if (component != null && component.Level > 15 && Random.value < 0.001f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.PlanarShard);
				___special = true;
			}
			if (Random.value < 0.001f && GameData.GM.DropMasks)
			{
				if (Random.Range(0, 100) > 1)
				{
					___ActualDrops.Add(GameData.Misc.Masks[Random.Range(0, GameData.Misc.Masks.Count)]);
				}
				else if (GameData.Misc.MoloraiMask != null)
				{
					___ActualDrops.Add(GameData.Misc.MoloraiMask);
				}
				___special = true;
			}
			if (GameData.GM.DemoBuild && Random.value < 0.001f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.Empty2);
				___special = true;
			}
			if (component != null && component.Level > 30 && Random.value < 0.00066666666f * serverLootRate)
			{
				___ActualDrops.Add(GameData.GM.Planar);
				___special = true;
			}
			Main.Log.LogInfo("Finished initial drop gen.");
			foreach (var drop in ___ActualDrops) Main.Log.LogInfo("Actual drop: " + drop.ItemName);
			if (___ActualDrops.Count > 0)
			{
				for (int j = 0; j < ___ActualDrops.Count; j++)
				{
					int num2 = Random.Range(0, 100);
					if (___ActualDrops[j] != null && ___ActualDrops[j].RequiredSlot != Item.SlotType.General && ___ActualDrops[j].RequiredSlot != Item.SlotType.Aura)
					{
						___ActualDropsQual.Add(num2);
					}
					else
					{
						___ActualDropsQual.Add(1);
					}
					if (num2 < 1 && ___ActualDrops[j] != null && ___ActualDrops[j].RequiredSlot != Item.SlotType.General && ___ActualDrops[j].RequiredSlot != Item.SlotType.Aura)
					{
						___special = true;
					}
				}
				for (int k = 0; k < ___ActualDrops.Count; k++)
				{
					if (___ActualDrops[k] != null && ___ActualDrops[k].Unique && GameData.PlayerInv.HasItem(___ActualDrops[k], false))
					{
						___ActualDrops[k] = GameData.GM.CommonWorldItems[Random.Range(0, GameData.GM.CommonWorldItems.Count)];
						___special = false;
					}
				}
				___special = false;
				for (int l = 0; l < ___ActualDrops.Count; l++)
				{
					if (!(___ActualDrops[l] == null) && (___ActualDrops[l] == GameData.GM.Sivak || ___ActualDrops[l] == GameData.GM.PlanarShard || ___ActualDrops[l] == GameData.GM.XPPot || ___ActualDrops[l] == GameData.GM.InertDiamond || GameData.GM.Maps.Contains(___ActualDrops[l]) || GameData.GM.WorldDropMolds.Contains(___ActualDrops[l]) || ___ActualDrops[l] == GameData.GM.Planar || (___ActualDropsQual.Count > l && ___ActualDropsQual[l] == 0)))
					{
						___special = true;
						return false;
					}
				}
			}
			return false;
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