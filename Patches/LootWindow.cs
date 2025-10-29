using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(LootWindow), nameof(LootWindow.LoadWindow))]
	public class LootWindow_LoadWindow
	{
		static void Postfix(LootWindow __instance, LootTable _incoming)
		{
			_incoming.GetComponent<LootHelper>()?.UpdateLootWindowQualities(__instance);
		}
	}
}
