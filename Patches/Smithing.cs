using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;
using HarmonyLib;

namespace Arcanism.Patches
{
    /* Patch to ensure upgrading to GODLY blessing level at smithy works w/ quality system  */
    [HarmonyPatch(typeof(Smithing), "Combine")]
    public class Smithing_Combine
    {
        public static void Prefix(Smithing __instance, ref OriginalItemMeta<Item> __state)
        {
            var slot = __instance.Components.Find(s => s.MyItem.IsUpgradeableEquipment() && GetBlessLevel(s.Quantity) == Blessing.BLESSED);
            if (slot != null)
            {
                __state = RevertQuantity(slot.MyItem, ref slot.Quantity);
                return;
            }

            __state = default;
        }

        static void Postfix(Smithing __instance, OriginalItemMeta<Item> __state)
        {
            // The smithy actually specifically moves the newly blessed item to slot 0 and clears the others, so we need to find it again
            var slot = __instance.Components.Find(s => s.MyItem == __state.itemRef && !IsItemQualityUpdated(s.Quantity));
            if (slot != null)
            {
                RestoreQuantity(__state, ref slot.Quantity);
                slot.UpdateSlotImage();
            }
        }
    }
}
