using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(ItemInfoWindow), nameof(ItemInfoWindow.DisplayItem))]
	public class ItemInfoWindow_DisplayItem
	{

		static void Postfix(ItemInfoWindow __instance, Item item, int _quantity)
		{
			if (item.ItemValue > 0 && !item.NoTradeNoDestroy)
				__instance.itemPrice.text = item.GetRealValue(_quantity).ToString();

			if (item.IsUpgradeableEquipment())
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

				var quality = ItemExtensions.GetQualityLevel(_quantity);
				
				if (quality != ItemExtensions.Quality.NORMAL) // I actually feel a little mean writing "if this item is... not normal"
                {
					string prefix = ItemExtensions.ToString(quality);
					string prefixColor = $"#{ColorUtility.ToHtmlStringRGB(ItemExtensions.GetQualityColor(quality))}";
					__instance.ItemName.text = $"<line-height=23><size=13><color={prefixColor}>{prefix}\n</color></size></line-height><size=20>{item.ItemName}</size>";
				}	
			}

			if (item.IsWand)
				__instance.OtherTextParent.GetComponent<TextMeshProUGUI>().text = "Base DPS: " + Mathf.RoundToInt(item.WeaponDmg / item.WeaponDly).ToString();

			__instance.MR.text = "+" + Inventory_UpdateInvStats.CalcResistance(item.MR, _quantity).ToString() + "%";
			__instance.PR.text = "+" + Inventory_UpdateInvStats.CalcResistance(item.PR, _quantity).ToString() + "%";
			__instance.VR.text = "+" + Inventory_UpdateInvStats.CalcResistance(item.VR, _quantity).ToString() + "%";
			__instance.ER.text = "+" + Inventory_UpdateInvStats.CalcResistance(item.ER, _quantity).ToString() + "%";
		}
	}
}
