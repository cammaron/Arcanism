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
        static void CheckSetBonus(Inventory inventory)
        {
			bool updatedStats = false;

			var equippedItemsById = ConvertToSlotsByItemId(inventory);

			foreach(var catalystEntry in ItemDatabase_Start.setBonusesByItemId)
            {
				if (!equippedItemsById.ContainsKey(catalystEntry.Key.Id()))
					continue;

				// We've got a catalyst, now we have to check if they have the other item equipped -- a single catalyst can have multiple different pairing items, and give a different bonus to each one.
				// (Noting it's the pairing item that actually receives the bonus -- the only point of that being tha
				foreach (var pairingOption in catalystEntry.Value)
				{
					if (equippedItemsById.TryGetValue(pairingOption.Key.Id(), out var slotReceivingBonus))
					{
						AddSetBonuses(inventory, slotReceivingBonus, pairingOption.Value);
						updatedStats = true;
					}
				}
			}

			if (inventory.isPlayer && updatedStats)
            {
				inventory.GetComponent<Stats>().CalcStats();
				inventory.PlayerStatDisp.UpdateDisplayStats();
			}
        }

		private static Dictionary<string, Item.SlotType> ConvertToSlotsByItemId(Inventory inventory)
        {
			var map = new Dictionary<string, Item.SlotType>();
			var simPlayer = inventory.GetComponent<SimPlayer>();
			if (simPlayer == null)
				foreach (var slot in inventory.EquipmentSlots)
					map[slot.MyItem.Id] = slot.ThisSlotType;
			else
				foreach (var slot in simPlayer.MyEquipment)
					map[slot.MyItem.Id] = slot.ThisSlotType;

			return map;
		}

		static void AddSetBonuses(Inventory inv, Item.SlotType slotType, Item bonusStats)
		{
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
			if (slotType == Item.SlotType.Primary)
			{
				inv.MHDelay += bonusStats.WeaponDly;
				inv.MHDmg += bonusStats.WeaponDmg;
			}
			else if (slotType == Item.SlotType.Secondary)
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
			inv.MitScaleMod += bonusStats.MitigationScaling;
		}

		static void Postfix(Inventory __instance)
        {
			CheckSetBonus(__instance);
        }
    }
}