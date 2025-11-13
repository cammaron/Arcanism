using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace Arcanism.Patches
{
    public enum QuestId
    {
        TREASUREMAP,
        PIRATE_MAP,
        DIG_SITE_MAP,
        VITHEAN_CACHE_MAP
    }

    public static class QuestDBExtensions
    {
        public static Quest GetQuestById(this QuestDB db, QuestId questId) => db.GetQuestByName(questId.ToString());
    }

    [HarmonyPatch(typeof(QuestDB), "Start")]
    public class QuestDB_Start
    {
        public static void Postfix(QuestDB __instance)
        {
            Main.Log.LogInfo("QuestDB: Waiting for Arcanism item database updates before updating quests.");
            __instance.StartCoroutine(WaitForItemDatabaseBeforeUpdatingQuests(__instance));
        }

        static IEnumerator WaitForItemDatabaseBeforeUpdatingQuests(QuestDB __instance)
        {
            while (!ItemDatabase_Start.IsFinished()) yield return null;
            Main.Log.LogInfo("QuestDB: Resuming quest updates.");

            List<Quest> questsToAdd = new List<Quest>();

            var mapQuestOne = __instance.GetQuestByName("TreasureMap");
            UpdateMapQuestStrings(mapQuestOne, "Stash");
            AddMapQuestItems(mapQuestOne, ItemId.MAP_PIECE_1, ItemId.FULL_MAP_1);

            AddNewMapQuest(mapQuestOne, QuestId.PIRATE_MAP, "Pirate", 150, ItemId.MAP_PIECE_2, ItemId.FULL_MAP_2, questsToAdd);
            AddNewMapQuest(mapQuestOne, QuestId.DIG_SITE_MAP, "Dig Site", 250, ItemId.MAP_PIECE_3, ItemId.FULL_MAP_3, questsToAdd);
            AddNewMapQuest(mapQuestOne, QuestId.VITHEAN_CACHE_MAP, "Vithean Cache", 450, ItemId.MAP_PIECE_4, ItemId.FULL_MAP_4, questsToAdd);

            int oldDbLength = __instance.QuestDatabase.Length;
            Array.Resize(ref __instance.QuestDatabase, __instance.QuestDatabase.Length + questsToAdd.Count);
            for (var i = 0; i < questsToAdd.Count; i++)
            {
                var quest = questsToAdd[i];
                __instance.QuestDatabase[oldDbLength + i] = quest;
                Main.Log.LogInfo($"New quest added to database: {quest.QuestName}");
            }
        }

        private static void AddNewMapQuest(Quest baseQuest, QuestId questId, string itemPlural, int expGain, ItemId mapPieceId, ItemId mapRewardId, List<Quest> questsToAdd)
        {
            var quest = GameObject.Instantiate(baseQuest);
            quest.DBName = questId.ToString();
            quest.XPonComplete = expGain;
            UpdateMapQuestStrings(quest, itemPlural);
            AddMapQuestItems(quest, mapPieceId, mapRewardId);


            questsToAdd.Add(quest);
        }

        private static void UpdateMapQuestStrings(Quest quest, string mapType)
        {
            quest.QuestName = $"Repairing {mapType} Maps";
            quest.QuestDesc = $"Cecil Threbb will repair {mapType} maps if you bring him 4 pieces in Faerie's Brake.";
        }

        private static void AddMapQuestItems(Quest quest, ItemId mapPieceId, ItemId mapRewardId)
        {
            var mapPiece = GameData.ItemDB.GetItemByID(mapPieceId);
            var mapReward = GameData.ItemDB.GetItemByID(mapRewardId);

            quest.RequiredItems.Clear();

            for (var i = 0; i < 4; i++)
                quest.RequiredItems.Add(mapPiece);

            quest.ItemOnComplete = mapReward;
        }
    }
}
