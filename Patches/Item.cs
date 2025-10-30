using UnityEngine;
using HarmonyLib;
using System.Globalization;

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

		public static bool IsUpgradeableEquipment(this Item i)
        {
			return (i.RequiredSlot != Item.SlotType.General && i.RequiredSlot != Item.SlotType.Aura && i.RequiredSlot != Item.SlotType.Charm);
		}

		public static string GetNameWithQuality(this Item i, int quantity)
        {
			var qual = GetQualityLevel(quantity);
			if (qual == Quality.NORMAL) return i.ItemName;


			string qualString = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(qual.ToString());

			return $"{qualString} {i.ItemName}";
        }

		public static int GetRealValue(this Item i, int quantity)
        {
			if (!IsUpgradeableEquipment(i))
				return i.ItemValue;

		
			float auctionlessPriceMod = 5f; // because blessed/qual items can't be sold on AH, in addition to being worth more in general, something close to the price you'd get on the AH is baked into their vendor sale value
			float baseValueMod;
			switch(GetBlessLevel(quantity))
            {
				case Blessing.BLESSED:
					baseValueMod = 2f;
					break;
				case Blessing.GODLY:
					baseValueMod = 4f;
					break;
				case Blessing.NONE:
				default:
					baseValueMod = 1f;
					break;
            }
			switch(GetQualityLevel(quantity))
            {
				case Quality.JUNK:
					baseValueMod *= .5f;
					break;
				case Quality.SUPERIOR:
					baseValueMod *= 1.5f;
					break;
				case Quality.MASTERWORK:
					baseValueMod *= 3f;
					break;
				case Quality.NORMAL:
				default:
					break;
			}

			if (baseValueMod > 1) // if value has been pumped, it's because it's not listable on AH, so also apply the AHless price mod
				baseValueMod *= auctionlessPriceMod;

			return Mathf.FloorToInt(i.ItemValue * baseValueMod);
        }

		public readonly struct OriginalItemMeta<T>
		{
			public readonly T itemRef;
			public readonly Quality quality;

			public OriginalItemMeta (T itemRef, Quality quality) {
				this.itemRef = itemRef;
				this.quality = quality;
			}
		}

		public static OriginalItemMeta<T> RevertQuantity<T>(T itemRef, ref int itemQuantity)
        {
			int originalQuantity = itemQuantity;
			itemQuantity = ToOriginalBlessLevel(itemQuantity);

			return new OriginalItemMeta<T>(itemRef, GetQualityLevel(originalQuantity));
        }

		public static void RestoreQuantity<T>(OriginalItemMeta<T> originalData, ref int itemQuantity) {
			itemQuantity = ToNewQuantity(itemQuantity, originalData.quality);
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

		public static int ToOriginalBlessLevel(int quantity)
        {
			return (int)GetBlessLevel(quantity) / 10;
        }

		public static int ToNewQuantity(int ogBlessLevel, Quality quality)
        {
			return (int)GetBlessLevel(ogBlessLevel) + (int) quality;
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
			/*if (__instance.RequiredSlot == Item.SlotType.Charm)
			{
				var bless = ItemExtensions.GetBlessLevel(_qual);
				if (bless == ItemExtensions.Blessing.BLESSED)
					_stat = 1;
				else if (bless == ItemExtensions.Blessing.GODLY)
					_stat = 2;

				__result = _stat;
				return false;
			}*/

			int res = _stat + (ItemExtensions.ToOriginalBlessLevel(_qual) - 1);
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

		static bool Prefix(Item __instance, ref int __result, int _stat, int _qual)
		{
			/*if (__instance.RequiredSlot == Item.SlotType.Charm)
            {
				var bless = ItemExtensions.GetBlessLevel(_qual);
				if (bless == ItemExtensions.Blessing.BLESSED)
					_stat = 8;
				else if (bless == ItemExtensions.Blessing.GODLY)
					_stat = 16;

				__result = _stat;
				return false;
			}*/
			float mod = ItemExtensions.GetMod(_qual);
			__result = Mathf.RoundToInt(_stat * mod);
			return false;
		}
	}

	[HarmonyPatch(typeof(Item), nameof(Item.CalcACHPMC))]
	public class Item_CalcACHPMC
	{

		static bool Prefix(Item __instance, ref int __result, int _stat, int _qual)
		{
			float mod = ItemExtensions.GetMod(_qual, .5f);
			__result = Mathf.RoundToInt(_stat * mod);
			return false;
		}
	}
}