using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace Arcanism.Patches
{
    /**/
    [HarmonyPatch(typeof(PlayerControl), "LeftClick")]
    public class PlayerControl_LeftClick
    {
        
        static bool Prefix(PlayerControl __instance, ref OriginalItemMeta<Item> __state)
        {
            Ray ray = __instance.camera.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                // Prevent masterwork items triggering the 'i can't take this item it's blessed' code on left click drop on NPC (ONLY for NPCs receiving quest items -- we still want to be able to give quality/blessed items to SimPlayers!)
                if (raycastHit.transform.GetComponent<NPC>() != null && raycastHit.transform.GetComponent<SimPlayer>() == null)
                {
                    var slot = GameData.MouseSlot;
                    var item = slot.MyItem;
                    if (item != null && item.IsUpgradeableEquipment())
                    {
                        __state = RevertQuantity(item, ref slot.Quantity);
                    }
                }
                
                // Overriding treasure chest spawn code to spawn predefined chest based on level of treasure map used
                if (raycastHit.transform.tag == "Treasure" && !EventSystem.current.IsPointerOverGameObject() && Vector3.Distance(__instance.transform.position, raycastHit.transform.position) < 5f)
                {
                    SpawnTreasureChest(raycastHit);
                    return false;
                }
            }

            return true;
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

        public static void SpawnTreasureChest(RaycastHit raycastHit)
        {
            GameObject.Instantiate(SpellVessel_DoMiscSpells.TreasureHuntChest, raycastHit.transform.position, raycastHit.transform.rotation);
            GameObject.Instantiate(GameData.Misc.DigFX, raycastHit.transform.position, raycastHit.transform.rotation);
            GameData.PlayerAud.PlayOneShot(GameData.Misc.DigSFX, GameData.PlayerAud.volume * GameData.SFXVol);
            GameData.GM.GetComponent<TreasureHunting>().ResetTreasureHunt();
            raycastHit.transform.gameObject.SetActive(false);
        }
    }
}
