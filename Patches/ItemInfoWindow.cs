using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(ItemInfoWindow), nameof(ItemInfoWindow.DisplayItem))]
	public class ItemInfoWindow_DisplayItem
	{

		static void Postfix(ItemInfoWindow __instance, Item item, int _quantity)
		{
			if (item.Aura == null && item.TeachSpell == null && item.TeachSkill == null && item.RequiredSlot != Item.SlotType.General)
            {
				switch (ItemExtensions.GetBlessLevel(_quantity))
				{
					case ItemExtensions.Blessing.NONE:
					default:
						__instance.ItemName.color = __instance.NormalText;
						break;
					case ItemExtensions.Blessing.BLESSED:
						__instance.ItemName.color = __instance.BlessedText;
						break;
					case ItemExtensions.Blessing.GODLY:
						__instance.ItemName.color = __instance.GodlyText;
						break;
				}

				string prefix;
				string prefixColor;
				switch (ItemExtensions.GetQualityLevel(_quantity))
				{
					case ItemExtensions.Quality.JUNK:
						prefix = "Junk";
						prefixColor = "#bbbbbb";
						break;
					case ItemExtensions.Quality.SUPERIOR:
						prefix = "Superior";
						prefixColor = "#00ffb7";
						break;
					case ItemExtensions.Quality.MASTERWORK:
						prefix = "Masterwork";
						prefixColor = "#97ff00";
						break;
					case ItemExtensions.Quality.NORMAL:
					default:
						prefix = null;
						prefixColor = null;
						break;
				}

				if (prefix != null) __instance.ItemName.text = $"<line-height=32><size=15><color={prefixColor}>[{prefix}]\n</color></size></line-height><size=20>{item.ItemName}</size>";
			}

			
		}
	}
}
