using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Arcanism.Patches
{
	/* Custom loot rarity mod: Having too many issues w/ bugs in og Loot Rarity mod and race conditions trying to patch it,
	 * so implementing simplified version. 
	 * 
	 * Using item quality (...internally, quantity) as both bless level AND rarity level.
	 * 11-14: normal item, digit represents rarity
	 * 21-24: blue bless, 2nd digit is rarity
	 * 31-34: as above but for purple
	 * */

	public static class ItemExtensions
    {
		public enum Blessing
		{
			NONE = 10,
			BLESSED = 20,
			GODLY = 30
		}

		public enum Quality
		{
			JUNK = 0, 
			NORMAL = 1,
			SUPERIOR = 2,
			MASTERWORK = 3
		}

		public static bool IsItemQualityUpdated(int quantity)
        {
			return quantity >= (int)Blessing.NONE;
        }

		public static Blessing GetBlessLevel(int quantity)
		{
			if (!IsItemQualityUpdated(quantity))
				return (Blessing) (quantity * 10);

			if (quantity >= (int)Blessing.GODLY)
				return Blessing.GODLY;
			if (quantity >= (int)Blessing.BLESSED)
				return Blessing.BLESSED;

			return Blessing.NONE;
		}

		public static Quality GetQualityLevel(int quantity)
		{
			if (!IsItemQualityUpdated(quantity)) return Quality.NORMAL; // An item that hasn't been processed -- dropped prior to Arcanism installation

			return (Quality) (quantity - (int)GetBlessLevel(quantity));
		}

		public static float GetMod(int quantity, float modFactor = 1)
		{
			float mod;

			switch (GetBlessLevel(quantity))
			{
				case Blessing.BLESSED:
					mod = 1 + 0.5f * modFactor;
					break;
				case Blessing.GODLY:
					mod = 1 + 1 * modFactor;
					break;
				default:
					mod = 1f;
					break;
			}

			switch (GetQualityLevel(quantity))
			{
				case Quality.JUNK:
					mod *= 1 - (.3f * modFactor); 
					break;
				case Quality.SUPERIOR:
					mod *= 1 + (0.25f * modFactor);
					break;
				case Quality.MASTERWORK:
					mod *= 1 + (0.5f * modFactor);
					break;

				case Quality.NORMAL:
				default:
					break;
			}

			return mod;
		}
    }

	/* Blessings need to be updated to give more than just +1 damage to a weapon, because that's completely useless */
	[HarmonyPatch(typeof(Item), nameof(Item.CalcDmg))]
	public class Item_CalcDmg
	{

		static bool Prefix(ref int __result, int _stat, int _qual)
		{
			float mod = ItemExtensions.GetMod(_qual, .5f); // Weapon damage is less affected by quality/blessing so as not to be OP
			__result = Mathf.RoundToInt(_stat * mod); 
			return false;
		}
	}

	[HarmonyPatch(typeof(Item), nameof(Item.CalcRes))]
	public class Item_CalcRes
	{

		static bool Prefix(Item __instance, ref int __result, int _stat, int _qual)
		{
			int res = _stat + (int)ItemExtensions.GetBlessLevel(_qual);
			switch (ItemExtensions.GetQualityLevel(_qual))
			{
				case ItemExtensions.Quality.JUNK:
					if (res > 0) res -= 1;
					break;
				case ItemExtensions.Quality.SUPERIOR:
					if (__instance.ItemLevel > 8) res += 1; // level restrictions on res are just so really early masterworks don't feel too overpowered since the res appearance is disproportionately good
					break;
				case ItemExtensions.Quality.MASTERWORK:
					if (__instance.ItemLevel > 16) res += 2;
					else res += 1;
					break;
			}
			__result = res;
			return false;
		}
	}

	[HarmonyPatch(typeof(Item), nameof(Item.CalcStat))]
	public class Item_CalcStat
	{

		static bool Prefix(ref int __result, int _stat, int _qual)
		{
			float mod = ItemExtensions.GetMod(_qual);
			__result = Mathf.RoundToInt(_stat * mod);
			return false;
		}
	}

	[HarmonyPatch(typeof(Item), nameof(Item.CalcACHPMC))]
	public class Item_CalcACHPMC
	{

		static bool Prefix(ref int __result, int _stat, int _qual)
		{
			float mod = ItemExtensions.GetMod(_qual, .5f);
			__result = Mathf.RoundToInt(_stat * mod);
			return false;
		}
	}
}