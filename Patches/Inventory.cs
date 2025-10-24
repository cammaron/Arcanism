using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Arcanism.Patches
{
    /* Facilitate item sets: when calculating stats for all equipped items, checks each item to see if it's the key to a set, and if so,
     * whether any matching items are also equipped, then applies bonus stats to the total inv calculation. */
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UpdateInvStats))]
    public class Inventory_UpdateInvStats
    {
        static void CheckSetBonus(Inventory inventory, ItemIcon currentSlot)
        {
			if (currentSlot == null || inventory == null) return;

			ItemId currentSlotId = (ItemId)int.Parse(currentSlot.MyItem.Id);
			if (!ItemDatabase_Start.setBonusesByItemId.TryGetValue(currentSlotId, out var allBonusStats))
				return;

            foreach(var otherSlot in inventory.EquipmentSlots)
            {
				string otherSlotIdStr = otherSlot?.MyItem?.Id;
                if (otherSlot == currentSlot || otherSlotIdStr == null) continue;

				ItemId otherSlotId = (ItemId)int.Parse(otherSlotIdStr);
				if (!allBonusStats.TryGetValue(otherSlotId, out var bonusStatsForItem))
					continue;

				AddSetBonuses(inventory, currentSlot, bonusStatsForItem);
            }
        }

		static void AddSetBonuses(Inventory inv, ItemIcon sourceItemSlot, Item bonusStats)
		{
			Item sourceItem = sourceItemSlot.MyItem;
			inv.ItemHP += bonusStats.HP;
			inv.ItemAC += bonusStats.AC;
			inv.ItemMana += bonusStats.Mana;
			inv.ItemStr += bonusStats.Str;
			inv.ItemEnd += bonusStats.End;
			inv.ItemDex += bonusStats.Dex;
			inv.ItemAgi += bonusStats.Agi;
			inv.ItemInt += bonusStats.Int;
			inv.ItemWis += bonusStats.Wis;
			inv.ItemCha += bonusStats.Cha;
			inv.ItemRes += bonusStats.Res;
			inv.ItemMR += bonusStats.MR;
			inv.ItemER += bonusStats.ER;
			inv.ItemPR += bonusStats.PR;
			inv.ItemVR += bonusStats.VR;
			if (sourceItem.RequiredSlot == Item.SlotType.Primary)
			{
				inv.MHDelay += bonusStats.WeaponDly;
				inv.MHDmg += bonusStats.WeaponDmg;
			}
			else if (sourceItem.RequiredSlot == Item.SlotType.Secondary)
			{
				inv.OHDelay += bonusStats.WeaponDly;
				inv.OHDmg += bonusStats.WeaponDmg;
			}

			inv.StrScaleMod += (int)bonusStats.StrScaling;
			inv.EndScaleMod += (int)bonusStats.EndScaling;
			inv.DexScaleMod += (int)bonusStats.DexScaling;
			inv.AgiScaleMod += (int)bonusStats.AgiScaling;
			inv.IntScaleMod += (int)bonusStats.IntScaling;
			inv.WisScaleMod += (int)bonusStats.WisScaling;
			inv.ChaScaleMod += (int)bonusStats.ChaScaling;
			inv.MitScaleMod += (float)((int)bonusStats.MitigationScaling);
		}


		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(CodeInstruction.StoreField(typeof(Inventory), nameof(Inventory.ItemVR)))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0), // "this" as arg
                    new CodeInstruction(OpCodes.Ldloc_1), // itemIcon
                    CodeInstruction.Call(() => CheckSetBonus(default, default)) //CheckSetBonus(this, itemIcon)
                )
                .Instructions();
        }
    }
}