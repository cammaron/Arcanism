using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(VendorWindow), nameof(VendorWindow.Transaction))]
	public class VendorWindow_Transaction
    {
		public static void Prefix(ref (Item, string, int) __state)
        {
			var vendorSlot = GameData.SlotActiveForVendor;
			if (vendorSlot == null || vendorSlot.MyItem == null)
				return;
			var item = vendorSlot.MyItem;
			__state = (item, item.ItemName, item.ItemValue);
			item.ItemName = item.GetNameWithQuality(vendorSlot.Quantity); 
			item.ItemValue = item.GetRealValue(vendorSlot.Quantity);
			UpdateSocialLog.LogAdd("For selling purposes item value has been set to " + item.ItemValue);
        }

		public static void Postfix((Item, string, int) __state) {
			if (__state == default) return;
			__state.Item1.ItemName = __state.Item2;
			__state.Item1.ItemValue = __state.Item3;
		}
    }

	[HarmonyPatch(typeof(VendorWindow), nameof(VendorWindow.DoSellStack))]
	public class VendorWindow_DoSellStack
	{
		public static void Prefix(ref (Item, string, int) __state)
		{
			var vendorSlot = GameData.SlotActiveForVendor;
			if (vendorSlot == null || vendorSlot.MyItem == null)
				return;
			var item = vendorSlot.MyItem;
			__state = (item, item.ItemName, item.ItemValue);
			item.ItemName = item.GetNameWithQuality(vendorSlot.Quantity);
			item.ItemValue = item.GetRealValue(vendorSlot.Quantity);
		}

		public static void Postfix((Item, string, int) __state)
		{
			if (__state == default) return;
			__state.Item1.ItemName = __state.Item2;
			__state.Item1.ItemValue = __state.Item3;
		}
	}

	[HarmonyPatch(typeof(GameData), nameof(GameData.ActivateSlotForVendor))] // Lil cheeky, not a patch for VendorWindow, but better to keep it in the same spot.
	public class GameData_ActivateSlotForVendor
	{
		public static void Prefix(ItemIcon _newActive, ref (Item, string, int) __state)
		{
			var item = _newActive.MyItem;
			__state = (item, item.ItemName, item.ItemValue);
			item.ItemName = item.GetNameWithQuality(_newActive.Quantity);
			item.ItemValue = item.GetRealValue(_newActive.Quantity);
		}

		public static void Postfix((Item, string, int) __state)
		{
			if (__state == default) return;
			__state.Item1.ItemName = __state.Item2;
			__state.Item1.ItemValue = __state.Item3;
		}
	}
}
