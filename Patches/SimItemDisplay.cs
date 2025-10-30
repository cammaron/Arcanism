using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism.Patches
{

	[HarmonyPatch(typeof(SimItemDisplay), "AssignItem")]
	public class SimItemDisplay_AssignItem
	{
		static void Postfix(SimItemDisplay __instance, int _quant)
		{
			ItemIconVisuals visuals = __instance.gameObject.GetOrAddComponent<ItemIconVisuals>();
			visuals.backgroundScale = .8f;
			visuals.originalSparkler = __instance.MySparkler;
			visuals.SetItemAndQuantity(__instance.MyItem, _quant);

			__instance.ItemLvl = ToOriginalBlessLevel(_quant); // for confusing reasons, ItemLvl on SimItemDisplay is actually the *BLESSING* level, and to make some of the logic work around whether they're ready to be upgraded, it needs to be the OG bless level
		}
	}

	[HarmonyPatch(typeof(SimItemDisplay), nameof(SimItemDisplay.ShowInfoWindow))]
	public class SimItemDisplay_ShowInfoWindow
    {
		public static void Prefix(SimItemDisplay __instance, ref int __state)
		{
			// ShowInfoWindow passes ItemLvl into the window as the quant field just to be difficult, so we temporarily set ItemLvl to the real (arcanism-style) quantity,
			// before flipping it back in postfix so as not to interfere with other logic
			__state = __instance.ItemLvl;
			__instance.ItemLvl = __instance.gameObject.GetOrAddComponent<ItemIconVisuals>().GetQuantity();
		}

		public static void Postfix(SimItemDisplay __instance, int __state)
		{
			__instance.ItemLvl = __state;
		}
	}
}
