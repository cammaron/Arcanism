using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism.Patches
{

	[HarmonyPatch(typeof(ItemIcon), "Start")]
	public class ItemIcon_Start { 
		static void Postfix(ItemIcon __instance)
        {
			var visuals = __instance.gameObject.AddComponent<ItemIconVisuals>();
			visuals.originalSparkler = __instance.MySparkler;
		}
	}


	[HarmonyPatch(typeof(ItemIcon), "Update")]
	public class ItemIcon_Update
	{
		public static void Postfix(ItemIcon __instance)
		{
			var visuals = __instance.gameObject.GetOrAddComponent<ItemIconVisuals>();
			visuals.originalSparkler = __instance.MySparkler; // not actually necesary, just doing for hotreload help
			visuals.SetItemAndQuantity(__instance.MyItem, __instance.Quantity);
		}
	}

	[HarmonyPatch(typeof(ItemIcon), "DoInitialChecks", new Type[] { })]
	public class ItemIcon_DoInitialChecks
	{
		public static void Prefix(ItemIcon __instance, ref OriginalItemMeta<ItemIcon> __state)
		{
			if (__instance.MyItem.IsUpgradeableEquipment())
				__state = RevertQuantity(__instance, ref __instance.Quantity);
		}
		public static void Postfix(ItemIcon __instance, ref OriginalItemMeta<ItemIcon> __state)
		{
			if (__state.itemRef != default)
				RestoreQuantity(__state, ref __instance.Quantity);
		}
	}

	[HarmonyPatch(typeof(ItemIcon), "DoInitialChecks", typeof(Item), typeof(ItemIcon))]
	public class ItemIcon_DoInitialChecksParams
	{
		public static void Prefix(ItemIcon __instance, ref OriginalItemMeta<ItemIcon> __state)
		{
			__state = RevertQuantity(__instance, ref __instance.Quantity);
		}
		public static void Postfix(ItemIcon __instance, ref OriginalItemMeta<ItemIcon> __state)
		{
			RestoreQuantity(__state, ref __instance.Quantity);
		}
	}
}
