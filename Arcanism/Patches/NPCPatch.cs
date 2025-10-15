using UnityEngine;
using HarmonyLib;


namespace Arcanism
{
    [HarmonyPatch(typeof(NPC), "Start")]
    public class NPCStartPatch
    {

        static void Prefix(NPC __instance)
        {
            if (__instance.NPCName == "Braxon Manfred")
            {
                __instance.GetComponent<Character>().isVendor = true;
                VendorInventory shop = __instance.GetComponent<VendorInventory>(); // a shop COULD get added to him in a future update, or depending on how death/respawn/zone change alters or resets the entity, may need re-adding too
                if (shop == null)
                {
                    shop = __instance.gameObject.AddComponent<VendorInventory>();
                    shop.ItemsForSale = new System.Collections.Generic.List<Item>();
                    shop.VendorDesc = "Exotic Book";
                    Traverse.Create(__instance.GetComponent<NPCDialogManager>()).Field("isVendor").SetValue(true);
                }
                var expertControlBook = GameData.ItemDB.GetItemByID(ItemDatabaseStartPatch.EXPERT_CONTROL_BOOK_ITEM_ID);
                if (!shop.ItemsForSale.Contains(expertControlBook)) // assuming if either one is already there, the other must be
                {
                    Main.Log.LogInfo("Adding items to Braxon Manfred's's shop.");
                    shop.ItemsForSale.Add(expertControlBook);
                    shop.ItemsForSale.Add(GameData.ItemDB.GetItemByID(ItemDatabaseStartPatch.TWIN_SPELL_BOOK_ITEM_ID));
                }
            }
        }
    }
}