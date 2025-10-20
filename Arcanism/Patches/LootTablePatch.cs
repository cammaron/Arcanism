using UnityEngine;
using HarmonyLib;


namespace Arcanism.Patches
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
                var twinSpell2 = GameData.ItemDB.GetItemByID(ItemId.TWIN_SPELL_2);
                if (!__instance.RareDrop.Contains(twinSpell2))
                    __instance.RareDrop.Add(twinSpell2);
            } else if (npc.NPCName == "Elwio the Traitor")
            {
                Main.Log.LogInfo("Adding Expert Control Book II skill book to Elwio the Traitor's loot table.");
                var expertControl2 = GameData.ItemDB.GetItemByID(ItemId.EXPERT_CONTROL_2);
                if (!__instance.RareDrop.Contains(expertControl2))
                    __instance.RareDrop.Add(expertControl2);
            } else  if (npc.NPCName == "Risen Druid")
            {
                Main.Log.LogInfo("Adding Spidersilk Shirt to Risen Druid's loot table.");
                var shirt = GameData.ItemDB.GetItemByID(ItemId.SPIDERSILK_SHIRT);
                if (!__instance.RareDrop.Contains(shirt))
                    __instance.RareDrop.Add(shirt);
            }
            else if (npc.NPCName == "Molorai Militia Arcanist")
            {
                Main.Log.LogInfo("Adding Trickster's Pants to Molorai Militia Arcanist's loot table.");
                var pants = GameData.ItemDB.GetItemByID(ItemId.TRICKSTERS_PANTS);
                if (!__instance.RareDrop.Contains(pants))
                    __instance.RareDrop.Add(pants);
            }
            else if (npc.NPCName == "Priel Deceiver")
            {
                Main.Log.LogInfo("Adding Tattered Wrap to Priel Deceiver's loot table.");
                var pants = GameData.ItemDB.GetItemByID(ItemId.TATTERED_WRAP);
                if (!__instance.RareDrop.Contains(pants))
                    __instance.RareDrop.Add(pants);
            }
            else if (npc.NPCName == "Vessel Siraethe")
            {
                Main.Log.LogInfo("Adding Lunar Weaveto Vessel Siraethe's loot table.");
                var pants = GameData.ItemDB.GetItemByID(ItemId.LUNAR_WEAVE);
                if (!__instance.RareDrop.Contains(pants))
                    __instance.RareDrop.Add(pants);
            }
        }
    }
}