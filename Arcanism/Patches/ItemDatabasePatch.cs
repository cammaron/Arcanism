using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections;

namespace Arcanism
{
	[HarmonyPatch(typeof(ItemDatabase), "Start")]
	public class ItemDatabaseStartPatch
    {
        public const string EXPERT_CONTROL_BOOK_ITEM_ID = "90000000";
        public const string EXPERT_CONTROL_2_BOOK_ITEM_ID = "90000001";
        public const string TWIN_SPELL_BOOK_ITEM_ID = "90000002";
        public const string TWIN_SPELL_2_BOOK_ITEM_ID = "90000003";

        static void Postfix(ItemDatabase __instance)
        {
            Main.Log.LogInfo("ItemDatabase initialising. Waiting for SkillDB to initialise before continuing.");
            __instance.StartCoroutine(UpdateItemDatabaseAfterSkillsLoaded(__instance));
        }

        static Item CreateSkillBook(Item baseSkillBook, string id, string name, int itemLevel, string skillId, int value)
        {
            var book = GameObject.Instantiate(baseSkillBook);
            book.Id = id;
            book.name = book.ItemName = $"Skill Book: {name}";
            book.ItemLevel = itemLevel;
            book.TeachSkill = GameData.SkillDatabase.GetSkillByID(skillId);
            book.ItemValue = value;
            return book;
        }

        static IEnumerator UpdateItemDatabaseAfterSkillsLoaded(ItemDatabase __instance)
        {
            while (GameData.SkillDatabase == null || !SkillDBStartPatch.IsInitialised()) yield return null;

            Main.Log.LogInfo("Adding Arcanism items to ItemDatabase.");
            var itemDictionary = Traverse.Create(__instance).Field<Dictionary<string, Item>>("itemDict").Value;
            var itemsToAdd = new List<Item>();

            var funeralPyreScroll = __instance.GetItemByID("17433350"); 
            string origName = funeralPyreScroll.ItemName;
            funeralPyreScroll.ItemIcon = __instance.GetItemByID("335358").ItemIcon; // Make it purple!
            funeralPyreScroll.ItemName = $"Spell Scroll: {SpellDBStartPatch.FUNERAL_PYRE_NAME_CHANGE}";
            Main.Log.LogInfo($"Item updated: {origName}");

            var controlChant = __instance.GetItemByID("27169650");
            itemsToAdd.Add(CreateSkillBook(controlChant, EXPERT_CONTROL_BOOK_ITEM_ID, "Expert Control I", 15, SkillDBStartPatch.EXPERT_CONTROL_SKILL_ID, 33000)); // Sold by Edwin in Port Azure
            itemsToAdd.Add(CreateSkillBook(controlChant, EXPERT_CONTROL_2_BOOK_ITEM_ID, "Expert Control II", 32, SkillDBStartPatch.EXPERT_CONTROL_2_SKILL_ID, 49000)); // Dropped by Elwio the Traitor in Vitheo's Rest
            itemsToAdd.Add(CreateSkillBook(controlChant, TWIN_SPELL_BOOK_ITEM_ID, "Twin Spell", 13, SkillDBStartPatch.TWIN_SPELL_SKILL_ID, 28000)); // Sold by Edwin in Port Azure
            itemsToAdd.Add(CreateSkillBook(controlChant, TWIN_SPELL_2_BOOK_ITEM_ID, "Twin Spell Mastery", 28, SkillDBStartPatch.TWIN_SPELL_MASTERY_SKILL_ID, 41000)); // Dropped by Fenton  the Blighted in The Blight

            int oldItemDbLength = __instance.ItemDB.Length;
            System.Array.Resize(ref __instance.ItemDB, __instance.ItemDB.Length + itemsToAdd.Count);
            for (var i = 0; i < itemsToAdd.Count; i++)
            {
                var item = itemsToAdd[i];

                if (itemDictionary.ContainsKey(item.Id))
                    throw new System.Exception($"Unable to add item {item.ItemName} as item with ID {item.Id} already exists: {itemDictionary[item.Id].ItemName}");

                __instance.ItemDB[oldItemDbLength + i] = item;
                itemDictionary.Add(item.Id, item);
                Main.Log.LogInfo($"Item added: {item.ItemName}");
            }

        }
    }
}