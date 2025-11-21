using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;


namespace Arcanism.Patches
{
    public enum NpcName
    {
        BRAXON_MANFRED,
        EDWIN_ANSEGG,
        CERBANTIAS_FLAMEWARD,
        RORELI_GILMARE,
        CECIL_THREBB,
        ASAGA_UNDERLOFT,
        NYLITH_VALORRI,

        MOLORAI_MILITIA_ARCANIST,
        RISEN_DRUID,
        PRIEL_DECEIVER,
        VESSEL_SIRAETHE,
        SEED_OF_BLIGHT,
        ELWIO_THE_TRAITOR,
        FENTON_THE_BLIGHTED,
        BLIGHT_WYRM,
        DIAMOND_HOUND,
        ARCANE_PUPIL,
        MOURNING,
        ZASHLYN_BLOODBANE,
        A_DEEPLING_ORATOR,
        EVADNE_THE_CORRUPTED,
        CELESTIAL_MATTER,
        STARDUST,
        ANCIENT_SPECTRE
    }

    public static class NpcExtensions
    {
        public static NpcName? GetEnumName(this NPC npc)
        {
            var nameToEnum = npc.NPCName?.ToUpper().Replace(' ', '_');
            if (System.Enum.TryParse(nameToEnum, out NpcName npcName))
            {
                return npcName;
            }

            return null;
        }

        public static bool IsNpc(this NPC npc, NpcName name)
        {
            NpcName? enumName = npc.GetEnumName();
            return (enumName.HasValue && enumName.Value == name);
        }
    }

    [HarmonyPatch(typeof(NPC), "Start")]
    public class NPC_Start
    {

        static void Prefix(NPC __instance)
        {
            var nameMatch = __instance.GetEnumName();
            if (!nameMatch.HasValue)
                    return;

            var npcName = nameMatch.Value;
            if (npcName == NpcName.EDWIN_ANSEGG)
            {
                var shop = __instance.GetComponent<VendorInventory>();
                shop.ItemsForSale.Remove(GameData.ItemDB.GetItemByID(ItemId.SPIDERSILK_SHIRT)); // this is replaced by Novice Robe
            }

            if (ItemDatabase_Start.itemsSoldByVendor.TryGetValue(npcName, out var itemIdsForSale)) {
                __instance.GetComponent<Character>().isVendor = true;
                var shop = __instance.GetComponent<VendorInventory>();
                if (shop == null)
                {
                    shop = __instance.gameObject.AddComponent<VendorInventory>();
                    shop.ItemsForSale = new List<Item>();
                    shop.VendorDesc = "Exotic Book"; // TODO: At the moment, only adding items to one *new* vendor, so no real need to have a generic way to define vendor desc -- may need to adjust later
                    Traverse.Create(__instance.GetComponent<NPCDialogManager>()).Field("isVendor").SetValue(true); // ^ as above, might need a null check on this, although unlikely
                }

                int emptiesRemoved = shop.ItemsForSale.RemoveAll(i => i == GameData.PlayerInv.Empty);
                if (emptiesRemoved > 0) Main.Log.LogInfo("Removed " + emptiesRemoved + " empty item slots from " + __instance.NPCName + "'s shop.");

                foreach (var itemId in itemIdsForSale)
                {
                    if (!shop.ItemsForSale.Exists(i => i.Id == itemId.Id()))  // don't think NPCs are recycled atm but just in case they are in future, don't want to double up items
                    {
                        var item = GameData.ItemDB.GetItemByID(itemId);
                        shop.ItemsForSale.Add(item);
                        Main.Log.LogInfo($"Added {item.ItemName} to {__instance.NPCName}'s shop.");
                    }
                }
            }

            if (npcName == NpcName.CECIL_THREBB)
            {
                var questManager = __instance.GetComponent<QuestManager>();
                questManager.NPCQuests.Add(GameData.QuestDB.GetQuestById(QuestId.PIRATE_MAP));
                questManager.NPCQuests.Add(GameData.QuestDB.GetQuestById(QuestId.DIG_SITE_MAP));
                questManager.NPCQuests.Add(GameData.QuestDB.GetQuestById(QuestId.VITHEAN_CACHE_MAP));
            }

            if (npcName == NpcName.ANCIENT_SPECTRE) // Significantly buff Ancient Spectre, mainly because I've tuned Glowing BLue Stone to be a powerful item, and its predecessor (Resonating Crystal) is dropped by Granitus who in vanilla is actually stronger than Ancient Spectre
            {
                var stats = __instance.GetComponent<Stats>(); // mystats is not defined yet
                stats.Level = 40;
                stats.BaseHP = 125000;
                stats.BaseMana = 4000;
                stats.BaseAC = 650;
                stats.BaseAgi = stats.BaseStr = stats.BaseDex = 800;
                stats.BaseLifesteal = 20;
            }
        }

        static void Postfix(NPC __instance)
        {
            var character = __instance.GetChar();
            if (character != null && character.GetComponent<CharacterUI.CharacterHoverUI>() == null)
            {
                character.gameObject.AddComponent<CharacterUI.CharacterHoverUI>();
            }

            __instance.gameObject.AddComponent<TheyMightBeGiants>();
        }
    }
}