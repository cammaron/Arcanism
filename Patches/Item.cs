using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Arcanism.Patches
{
	/* Blessings need to be updated to give more than just +1 damage to a weapon, because that's completely useless */
	[HarmonyPatch(typeof(Item), nameof(Item.CalcDmg))]
	public class Item_CalcDmg
	{

		static bool Prefix(Item __instance, ref int __result, int _stat, int _qual)
		{
			if (_qual == 2) __result = Mathf.RoundToInt(_stat * 1.25f);
			else if (_qual == 3) __result = Mathf.RoundToInt(_stat * 1.5f);
			__result = _stat;
			return false;
		}
	}
}