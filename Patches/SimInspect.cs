using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism.Patches
{
    static class SimGearUpgradeFixes
    {
        
        public static void PrepForUpgrade(SimInspect inspector, ref OriginalItemMeta<SimInvSlot> __state)
        {
            var slot = GetSlotForUpgrade(inspector);
            if (slot == null)
            {
                __state = default;
                return;
            }

            __state = RevertQuantity(slot, ref slot.Quant);
        }

        public static void RestoreAfterUpgrade(SimInspect inspector, OriginalItemMeta<SimInvSlot> __state)
        {
            if (__state.itemRef == default) return;

            RestoreQuantity(__state, ref __state.itemRef.Quant);
            inspector.InspectSim(GameData.InspectSim.Who); // upgrade methods call this to refresh data, but it's called before I've re-fixed quantity, so needs calling again.
        }

        static SimInvSlot GetSlotForUpgrade(SimInspect __instance)
        {
            foreach (SimInvSlot simInvSlot in GameData.InspectSim.Who.MyEquipment)
            {
                if (simInvSlot.MyItem.Id == __instance.AdjustSlot.MyItem.Id && ToOriginalBlessLevel(simInvSlot.Quant) == __instance.AdjustSlot.ItemLvl)
                    return simInvSlot;
            }

            return null;
        }
    }
    /* In both cases of upgrading to blessed or godly, we want to simplify by regressing item quantity to original value in prefix, then re-converting to appropriate bless quantity and re-adding in quality in postfix */
    [HarmonyPatch(typeof(SimInspect), nameof(SimInspect.OfferSivaks))]
    class SimInspect_OfferSivaks
    { 
        static void Prefix(SimInspect __instance, ref OriginalItemMeta<SimInvSlot> __state)
        {
            SimGearUpgradeFixes.PrepForUpgrade(__instance, ref __state);
        }

        static void Postfix(SimInspect __instance, OriginalItemMeta<SimInvSlot> __state)
        {
            SimGearUpgradeFixes.RestoreAfterUpgrade(__instance, __state);
        }
    }

    [HarmonyPatch(typeof(SimInspect), nameof(SimInspect.OfferPlanar))]
    class SimInspect_OfferPlanar
    {
        static void Prefix(SimInspect __instance, ref OriginalItemMeta<SimInvSlot> __state)
        {
            SimGearUpgradeFixes.PrepForUpgrade(__instance, ref __state);
        }

        static void Postfix(SimInspect __instance, OriginalItemMeta<SimInvSlot> __state)
        {
            SimGearUpgradeFixes.RestoreAfterUpgrade(__instance, __state);
        }
    }
}
