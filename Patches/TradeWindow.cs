using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;
using HarmonyLib;

namespace Arcanism.Patches
{
    /* Patch to ensure sivakrux offerings work w/ quality system -- requires reverting to original blessing (quantity) then back while preserving quality in state */
    [HarmonyPatch(typeof(TradeWindow), nameof(TradeWindow.CompleteTrade))]
    public class TradeWindow_CompleteTrade
    {
        /* Trade is for lots of stuff aside from the Braxonian Flame Well. To make it simple, all equipment will be reverted before trading... */
        static void Prefix(List<ItemIcon> ___LootSlots, ref List<OriginalItemMeta<Item>> __state)
        {
            __state = new List<OriginalItemMeta<Item>>();
            foreach(var s in ___LootSlots)
            {
                if (s.MyItem != null && s.MyItem.IsUpgradeableEquipment())
                {
                    __state.Add(RevertQuantity(s.MyItem, ref s.Quantity));
                }
            }
        }

        /* In the case of the Flame Well, we get the item back (now blessed w/ Quantity=2). A failed trade with anyone incl Flame Well will also return items to inventory.
         * That means we can't just restore Quantity to the slot we used in Prefix -- we have to find matching items in the player's inventory now. */
        static void Postfix(List<OriginalItemMeta<Item>> __state)
        {
            foreach (var meta in __state)
            {
                ItemIcon inventorySlot = GameData.PlayerInv.StoredSlots.Find(s => s.MyItem == meta.itemRef && !IsItemQualityUpdated(s.Quantity));
                if (inventorySlot != null)
                    RestoreQuantity(meta, ref inventorySlot.Quantity);
            }

            GameData.PlayerInv.UpdatePlayerInventory();
        }
    }

    /* For unknown reasons CancelTrade iterates over the quantity of each item in its slots, calling add item to player once *per quantity.* Works for items with stacks, but makes no sense
     * for items that are blessed or, as the case may be, have a quality. */
    [HarmonyPatch(typeof(TradeWindow), nameof(TradeWindow.CancelTrade))]
    public class TradeWindow_CancelTrade
    {
        
        static void Prefix(List<ItemIcon> ___LootSlots)
        {
            // I'm just going to get around this issue by moving upgradeable items first and letting the vanilla code handle only the basic things.
            foreach(var slot in ___LootSlots)
            {
                if (slot.MyItem != null && slot.MyItem.IsUpgradeableEquipment())
                {
                    if (!GameData.PlayerInv.AddItemToInv(slot.MyItem, slot.Quantity))
                    {
                        GameData.PlayerInv.ForceItemToInv(slot.MyItem, slot.Quantity);
                    }
                    slot.MyItem = GameData.PlayerInv.Empty;
                    slot.Quantity = 1;
                }
            }
        }
    }
}
