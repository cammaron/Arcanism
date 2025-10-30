using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;
using HarmonyLib;

namespace Arcanism.Patches
{
    /* Prevent masterwork items triggering the 'i can't take this item it's blessed' code on left click drop on NPC */
    [HarmonyPatch(typeof(PlayerControl), "LeftClick")]
    public class PlayerControl_LeftClick
    {
        
        static void Prefix(ref OriginalItemMeta<Item> __state)
        {
            var slot = GameData.MouseSlot;
            var item = slot.MyItem;
            if (item != null && item.IsUpgradeableEquipment())
            {
                __state = RevertQuantity(item, ref slot.Quantity);
            }
        }

        static void Postfix(OriginalItemMeta<Item> __state)
        {
            // Two possibilities: item is still in MouseSlot (i.e. on cursor) meaning a drop somewhere else failed,
            // or--and this is fragile as heck to assume going forward indefinitely, but true at time of writing--it's been dropped in a tradewindow slot.
            if (__state.itemRef != default)
            {
                if (GameData.MouseSlot.MyItem == __state.itemRef)
                    RestoreQuantity(__state, ref GameData.MouseSlot.Quantity);
                else
                {
                    var tradeSlot = GameData.TradeWindow.LootSlots.Find(s => s.MyItem == __state.itemRef);
                    if (tradeSlot != null)
                        RestoreQuantity(__state, ref tradeSlot.Quantity);
                }
            }
        }
    }
}
