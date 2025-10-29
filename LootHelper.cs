using Arcanism.Patches;
using System.Collections.Generic;
using UnityEngine;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism
{
    public class LootHelper : MonoBehaviour
    {
        public static float QUALITY_CHANCE_FACTOR = 1f;

        public LootTable lootTable;
        private List<(Blessing, Quality)> blessingsAndQuality;

        /* Like most data, enemy drops are defined in their ScriptableObjects and loaded at runtime, so changing them requires injecting via code.
        * I'm using a map of drops by enemy and looking that up when the loot table is spawned to insert any relevant drops. */
        public void PopulateExtraItems()
        {
            var npc = GetComponent<NPC>();
            if (npc == null) return;

            var nameToEnum = npc.NPCName.ToUpper().Replace(' ', '_');
            if (System.Enum.TryParse(nameToEnum, out NpcName npcName))
            {
                if (ItemDatabase_Start.dropsByNpc.TryGetValue(npcName, out HashSet<(DropChance, Item)> drops))
                {
                    foreach (var drop in drops)
                    {
                        List<Item> relevantList = null;
                        switch (drop.Item1)
                        {
                            case DropChance.COMMON:
                                relevantList = lootTable.CommonDrop;
                                break;
                            case DropChance.UNCOMMON:
                                relevantList = lootTable.UncommonDrop;
                                break;
                            case DropChance.RARE:
                                relevantList = lootTable.RareDrop;
                                break;
                            case DropChance.LEGENDARY:
                                relevantList = lootTable.LegendaryDrop;
                                break;
                            case DropChance.GUARANTEE_ONE:
                                relevantList = lootTable.GuaranteeOneDrop;
                                break;
                        }

                        if (!relevantList.Contains(drop.Item2)) // I don't think NPCs get recycled from a pool at the moment, but just in case, don't wanna be doubling up
                            relevantList.Add(drop.Item2);
                    }
                }
            }
        }

        // Returns true if any special items generated
        public bool UpdateLootQuality()
        {
            bool specialGenerated = false;
            blessingsAndQuality = new List<(Blessing, Quality)>(lootTable.ActualDrops.Count);
            foreach(var item in lootTable.ActualDrops)
            {
                if (item.RequiredSlot == Item.SlotType.General || item.RequiredSlot == Item.SlotType.Aura)
                    continue;

                var blessLevel = IsRandomlyBlessed() ? Blessing.BLESSED : Blessing.NONE;
                var qualityLevel = GenerateQualityLevel();
                blessingsAndQuality.Add((blessLevel, qualityLevel));

                if (blessLevel != Blessing.NONE || qualityLevel == Quality.MASTERWORK)
                    specialGenerated = true;
            }

            return specialGenerated;
        }

        public void UpdateLootWindowQualities(LootWindow window)
        {
            for (var i = 0; i < window.LootSlots.Count; i++)
            {
                var slot = window.LootSlots[i];

                if (slot.MyItem.RequiredSlot == Item.SlotType.General || slot.MyItem.RequiredSlot == Item.SlotType.Aura)
                    continue;

                var bonus = blessingsAndQuality[i];
                slot.Quantity = (int) bonus.Item1 + (int) bonus.Item2; 
                lootTable.qualUps[i] = slot.Quantity;
                lootTable.ActualDropsQual[i] = slot.Quantity;
            }
        }

        protected bool IsRandomlyBlessed()
        {
            return Random.Range(0, 100) == 0;
        }

        protected Quality GenerateQualityLevel()
        {
            var chance = Random.Range(0, 100);
            if (chance < 1.5f * QUALITY_CHANCE_FACTOR)
                return Quality.MASTERWORK;
            else if (chance < 8f * QUALITY_CHANCE_FACTOR)
                return Quality.SUPERIOR;
            else if (chance < 28)
                return Quality.JUNK;
            
            return Quality.NORMAL;
        }
    }
}
