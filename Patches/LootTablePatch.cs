using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;


namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(LootTable), "InitLootTable")]
    public class LootTableInitLootTablePatch
    {

        static void Prefix(LootTable __instance)
        {
            var npc = __instance.transform.gameObject.GetComponent<NPC>();
            if (npc == null) return;

            var nameToEnum = npc.NPCName.ToUpper().Replace(' ', '_');
            if (System.Enum.TryParse(nameToEnum, out NpcName npcName))
            {
                if (ItemDatabase_Start.dropsByNpc.TryGetValue(npcName, out HashSet<(DropChance, Item)> drops))
                {
                    foreach(var drop in drops)
                    {
                        List<Item> relevantList = null;
                        switch(drop.Item1)
                        {
                            case DropChance.COMMON:
                                relevantList = __instance.CommonDrop;
                                break;
                            case DropChance.UNCOMMON:
                                relevantList = __instance.UncommonDrop;
                                break;
                            case DropChance.RARE:
                                relevantList = __instance.RareDrop;
                                break;
                            case DropChance.LEGENDARY:
                                relevantList = __instance.LegendaryDrop;
                                break;
                            case DropChance.GUARANTEE_ONE:
                                relevantList = __instance.GuaranteeOneDrop;
                                break;
                        }

                        if (!relevantList.Contains(drop.Item2)) // I don't think NPCs get recycled from a pool at the moment, but just in case, don't wanna be doubling up
                            relevantList.Add(drop.Item2);
                    }
                }
            }
        }
    }
}