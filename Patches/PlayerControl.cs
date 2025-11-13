using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace Arcanism.Patches
{
    /* Prevent masterwork items triggering the 'i can't take this item it's blessed' code on left click drop on NPC */
    [HarmonyPatch(typeof(PlayerControl), "LeftClick")]
    public class PlayerControl_LeftClick
    {
        
        static bool Prefix(PlayerControl __instance, ref OriginalItemMeta<Item> __state)
        {
            var slot = GameData.MouseSlot;
            var item = slot.MyItem;
            if (item != null && item.IsUpgradeableEquipment())
            {
                __state = RevertQuantity(item, ref slot.Quantity);
            }

            Ray ray = __instance.camera.ScreenPointToRay(Input.mousePosition);
            
            // Overriding treasure chest spawn code to spawn predefined chest based on level of treasure map used
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.transform.tag == "Treasure" && !EventSystem.current.IsPointerOverGameObject() && Vector3.Distance(__instance.transform.position, raycastHit.transform.position) < 5f)
                {
                    GameObject.Instantiate(SpellVessel_DoMiscSpells.TreasureHuntChest, raycastHit.transform.position, raycastHit.transform.rotation);
                    GameObject.Instantiate(GameData.Misc.DigFX, raycastHit.transform.position, raycastHit.transform.rotation);
                    GameData.PlayerAud.PlayOneShot(GameData.Misc.DigSFX, GameData.PlayerAud.volume * GameData.SFXVol);
                    GameData.GM.GetComponent<TreasureHunting>().ResetTreasureHunt();
                    raycastHit.transform.gameObject.SetActive(false);
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
    }
}
