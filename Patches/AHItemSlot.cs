using HarmonyLib;

namespace Arcanism.Patches
{

	/* 
	 * Jackin' up AH prices
	 * 
	 * Another highly opinionated change, but I feel as though it's way easier to sell crappy items nobody would really want for a few thousand on the AH and then buy some stuff you wouldn't even be
	 * able to farm yourself yet for that same amount of money, which for me kind of dampens the experience of hunting for my own gear -- I want purchasing gear I didn't find myself to require a real sacrifice
	 * so that it is something I'll do sparingly when I REALLY want or need something, and not so I can just casually go buy a full new set of gear.
	 * Hence, now multiplying base greed factor as well as putting a true floor on item prices based on their level to address inconsistencies in pricing of higher level items. */
	[HarmonyPatch(typeof(AHItemSlot), nameof(AHItemSlot.LoadNewItem))]
	public class AHItemSlot_LoadNewItem
	{
		public static void Prefix(Item _item, ref float _greed)
		{
			_greed *= Main.auctionHouseGreedFactor.Value;

			int expectedBaseItemValue = _item.GetRealValue(1);
			if (_item.ItemValue < expectedBaseItemValue)
				_greed *= expectedBaseItemValue / _item.ItemValue;
		}
	}

	/* TEMPORARY: Randomly stuck some debug here to work out why there are NPEs happening sometimes when saving sims... may have been to do with old RECKS_RV items in sims that aren't in DB any more. */
	[HarmonyPatch(typeof(SimPlayer), nameof(SimPlayer.SaveSim))]
	public class SimPlayer_SaveSim
	{
		public static void Prefix(SimPlayer __instance)
		{
			Main.Log.LogInfo("Saving sim: " + __instance.name + " - " + __instance.GetThisNPC()?.NPCName);

		}
	}


}
