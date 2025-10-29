using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace Arcanism.Patches
{
    
    [HarmonyPatch(typeof(SimInspect), nameof(SimInspect.OfferSivaks))]
    class SimInspect_OfferSivaks
    {
        struct UpgradeMeta
        {
            public SimInvSlot slot;
            public ItemExtensions.Quality quality;
            public int originalQuantity;
        }

        // In prefix, revert item quantity to original (pre-quality changes) to let original game logic run
        static void Prefix(SimInspect __instance, ref UpgradeMeta __state)
        {
            var slot = GetSlotForUpgrade(__instance);
            if (slot == null)
            {
                __state = default;
                return;
            }

            __state = new UpgradeMeta() { slot = slot, quality = ItemExtensions.GetQualityLevel(slot.Quant), originalQuantity = slot.Quant };
            slot.Quant = (int)ItemExtensions.GetBlessLevel(slot.Quant) + 1;
        }

        // Add preserved quality level back into quantity
        static void Postfix(SimInspect __instance, UpgradeMeta __state)
        {
            if (__state.slot == default) return;

            
        }

        static SimInvSlot GetSlotForUpgrade(SimInspect __instance)
        {
            foreach (SimInvSlot simInvSlot in GameData.InspectSim.Who.MyEquipment)
            {
                if (simInvSlot.MyItem.Id == __instance.AdjustSlot.MyItem.Id && simInvSlot.Quant == __instance.AdjustSlot.ItemLvl)
                {
                    return simInvSlot;
                }
            }

            return null;
        }
    }
}
