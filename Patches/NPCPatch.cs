using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;


namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(NPC), "Start")]
    public class NPCStartPatch
    {

        static void Prefix(NPC __instance)
        {
            var nameToEnum = __instance.NPCName.ToUpper().Replace(' ', '_');
            if (System.Enum.TryParse(nameToEnum, out NpcName npcName))
            {
                if (ItemDatabase_Start.itemsSoldByVendor.TryGetValue(npcName, out HashSet<Item> itemsForSale)) {
                    __instance.GetComponent<Character>().isVendor = true;
                    var shop = __instance.GetComponent<VendorInventory>();
                    if (shop == null)
                    {
                        shop = __instance.gameObject.AddComponent<VendorInventory>();
                        shop.ItemsForSale = new List<Item>();
                        shop.VendorDesc = "Exotic Book"; // TODO: At the moment, only adding items to one *new* vendor, so no real need to have a generic way to define vendor desc -- may need to adjust later
                        Traverse.Create(__instance.GetComponent<NPCDialogManager>()).Field("isVendor").SetValue(true); // ^ as above, might need a null check on this, although unlikely
                    }
                    foreach (var item in itemsForSale)
                    {
                        if (!shop.ItemsForSale.Contains(item)) // don't think NPCs are recycled atm but just in case they are in future, don't want to double up items
                            shop.ItemsForSale.Add(item);
                    }
                    
                }

                if (npcName == NpcName.EDWIN_ANSEGG)
                {
                    var shop = __instance.GetComponent<VendorInventory>();
                    shop.ItemsForSale.Remove(GameData.ItemDB.GetItemByID(ItemId.SPIDERSILK_SHIRT)); // this is replaced by Novice Robe
                }
            }
            
            
        }
    }
}