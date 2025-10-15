using UnityEngine;
using HarmonyLib;


namespace Arcanism
{
    [HarmonyPatch(typeof(LootTable), "InitLootTable")]
    public class LootTableInitLootTablePatch
    {

        static void Prefix(LootTable __instance)
        {
            var npc = __instance.transform.gameObject.GetComponent<NPC>();
            if (npc == null) return;

            if (npc.NPCName == "Fenton the Blighted")
            {
                Main.Log.LogInfo("Adding Twin Spell Mastery skill book to Fenton the Blighted's loot table.");
                var twinSpell2 = GameData.ItemDB.GetItemByID(ItemDatabaseStartPatch.TWIN_SPELL_2_BOOK_ITEM_ID);
                if (!__instance.RareDrop.Contains(twinSpell2))
                    __instance.RareDrop.Add(twinSpell2);
            } else if (npc.NPCName == "Elwio the Traitor")
            {
                Main.Log.LogInfo("Adding Expert Control Book II skill book to Elwio the Traitor's loot table.");
                var expertControl2 = GameData.ItemDB.GetItemByID(ItemDatabaseStartPatch.EXPERT_CONTROL_2_BOOK_ITEM_ID);
                if (!__instance.RareDrop.Contains(expertControl2))
                    __instance.RareDrop.Add(expertControl2);
            }
        }
    }
}