using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(LootWindow), nameof(LootWindow.LoadWindow))]
	public class LootWindow_LoadWindow
	{
		public static float QUALITY_CHANCE_FACTOR = 1f;
		static void Postfix(LootWindow __instance, LootTable _incoming)
		{
			for(var i = 0; i < __instance.LootSlots.Count; i ++)
            {
				var slot = __instance.LootSlots[i];
				if (slot.MyItem.RequiredSlot == Item.SlotType.General || slot.MyItem.RequiredSlot == Item.SlotType.Aura)
					continue;

				if (!ItemExtensions.IsItemQualityUpdated(slot.Quantity))
                {
					// Firstly, get it to the right blessing level
					slot.Quantity *= 10;
					var chance = Random.Range(0, 100);
					if (chance < 1.5f * QUALITY_CHANCE_FACTOR)
						slot.Quantity += (int)ItemExtensions.Quality.MASTERWORK;
					else if (chance < 8f * QUALITY_CHANCE_FACTOR)
						slot.Quantity += (int)ItemExtensions.Quality.SUPERIOR;
					else if (chance < 28)
						slot.Quantity += (int)ItemExtensions.Quality.JUNK;
					else
						slot.Quantity += (int)ItemExtensions.Quality.NORMAL;

					_incoming.qualUps[i] = slot.Quantity;
					_incoming.ActualDropsQual[i] = slot.Quantity;
				}
            }
		}
	}
}
