using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism.Patches
{

	/* Dealing w/ some duplicated item slot functionality */
	[HarmonyPatch(typeof(SimSelectorSlot), nameof(SimSelectorSlot.LoadSelectorSlot))]
	public class SimSelectorSlot_LoadSelectorSlot
	{
		static void Postfix(SimSelectorSlot __instance, int _qual)
		{
			ItemIconVisuals visuals = __instance.gameObject.GetOrAddComponent<ItemIconVisuals>();
			visuals.originalSparkler = __instance.MySparkler;
			visuals.SetItemAndQuantity(__instance.DisplayItem, _qual);
		}
	}
}
