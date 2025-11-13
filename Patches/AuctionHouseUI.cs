using HarmonyLib;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism.Patches
{
	/* Just to prevent players trying to list blessed/quality gear on the AH -- AH doesn't support qty */
	[HarmonyPatch(typeof(AuctionHouseUI), nameof(AuctionHouseUI.OpenListItem))]
	public class AuctionHouseUI_OpenListItem
	{
		public static bool Prefix()
		{
			var slot = GameData.SlotToBeListed;
			var item = slot.MyItem;
			var blessLevel = ItemExtensions.GetBlessLevel(slot.Quantity);
			var quality = ItemExtensions.GetQualityLevel(slot.Quantity);
			if (item.IsUpgradeableEquipment() && (blessLevel > ItemExtensions.Blessing.NONE || quality > ItemExtensions.Quality.NORMAL))
			{
				UpdateSocialLog.LogAdd("Due to a limitation in auction workings, blessed/quality items in Arcanism cannot reflect their true value when sold on the AH and thus have been disabled -- vendor sale price has been adjusted to make up for this!", "red");
				return false;
			}

			return true;
		}
	}

	/* Normal items still have a quantity of 10 in Arcanism, so it's necessary to revert them to their original 1 before adding to the AH.
	 * In the postfix, restore the quantity in case the item listing actually failed. */
	[HarmonyPatch(typeof(AuctionHouseUI), nameof(AuctionHouseUI.CommitItem))]
    public class AuctionHouseUI_CommitItem
    {
        public static void Prefix(OriginalItemMeta<ItemIcon> __state)
        {
			var slot = GameData.SlotToBeListed;
			if (slot.MyItem != null && slot.MyItem.IsUpgradeableEquipment())
				__state = RevertQuantity(slot, ref slot.Quantity);
        }

		public static void Postfix(OriginalItemMeta<ItemIcon> __state)
        {
			var slot = GameData.SlotToBeListed;
			if (slot.MyItem != null && slot.MyItem.IsUpgradeableEquipment())
				RestoreQuantity(__state, ref slot.Quantity);
		}
    }
}
