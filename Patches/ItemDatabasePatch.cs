using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections;

namespace Arcanism.Patches
{
    public enum ItemId
    {
        //Feets!
        DEVOUT_MOCCASINS = 39403944,
        FLAMESOLES = 24495417,
        ELDERS_SLIPPERS = 12898230,
        BRAXONIAN_ROYAL_SHOES = 12898230,
        PIOUS_SANDALS = 2562654,

        //Pants
        COMPRESSION_WRAPS = 50065464,
        REED_TROUSERS = 15254492,
        LINEN_BEDPANTS = 4222708,
        CAVESILK_TROUSERS = 18975850,
        STARCHED_WOOL_LEGGINGS = 2067177,
        SIVAKAYAN_BREECHES = 32144661,
        BRAXONIAN_LINENS = 1980512,
        ILLUSIONIST_TROUSERS = 2830721,
        
        //Tops
        FUNERAL_GARB = 15595785,
        SPIDERSILK_SHIRT = 12534767,
        BRAXONIAN_WRAP = 19249410,
        PRESERVED_CLOTH_COAT = 74125116,
        SIVAKAYAN_DRESSCOAT = 20247051,
        BRAXONIAN_ROYAL_TUNIC = 53553930,
        ELDOTHS_FINERY = 43380006,

        // Scrolls
        FUNERAL_PYRE = 17433350,
        CONTROL_CHANT = 27169650,
        
        // Custom items
        EXPERT_CONTROL = 90000000,
        EXPERT_CONTROL_2 = 90000001,
        TWIN_SPELL = 90000002,
        TWIN_SPELL_2 = 90000003,
        
        TRICKSTERS_PANTS = 90000004,
        TATTERED_WRAP = 90000005,
        LUNAR_WEAVE = 90000006,

        NOVICE_ROBE = 90000007,
        
    }

    static class ItemIdExtensions
    {
        public static string Id(this ItemId itemId) => ((int)itemId).ToString();
        public static Item GetItemByID(this ItemDatabase db, ItemId itemId) => db.GetItemByID(itemId.Id());
    }


    [HarmonyPatch(typeof(ItemDatabase), "Start")]
	public class ItemDatabase_Start
    {
        public static Item[] originalDb;

        static void Postfix(ItemDatabase __instance)
        {
            __instance.StartCoroutine(UpdateItemDatabaseAfterSkillsLoaded(__instance));
        }

        static Item CreateSkillBook(Item baseSkillBook, ItemId id, string name, int itemLevel, string skillId, int value)
        {
            var book = GameObject.Instantiate(baseSkillBook);
            book.Id = id.Id();
            book.name = book.ItemName = $"Skill Book: {name}";
            book.ItemLevel = itemLevel;
            book.TeachSkill = GameData.SkillDatabase.GetSkillByID(skillId);
            book.ItemValue = value;
            return book;
        }

        private class EquipmentGenerator
        {
            public ItemId? CreateFromBaseId;
            public ItemId Id;

            public int? Level;
            public int? HP;
            public int? Mana;
            public int? AC;

            public int? Str;
            public int? Dex;
            public int? Agi;
            public int? End;
            public int? Int;
            public int? Wis;
            public int? Cha;

            public int? Res;
            public int? Value;

            public string Name;
            public string Lore;

            public Item.SlotType? SlotType;
            public string AppearanceType;

            public (string, string)? ShoulderTrim;
            public (string, string)? ElbowTrim; 
            public (string, string)? KneeTrim;
            
            public (Color, Color)? ColorsMain;
            public (Color, Color)? ColorsLeather;
            public (Color, Color)? ColorsMetal;

            public List<Class> Classes;
            
            public Item Generate(ItemDatabase db)
            {
                Main.Log.LogInfo($"**** Generating item {Id.Id()} - {Name}");
                Item item;
                if (CreateFromBaseId.HasValue)
                {
                    item = GameObject.Instantiate(db.GetItemByID(CreateFromBaseId.Value));
                    item.Id = Id.Id();
                } else 
                    item = db.GetItemByID(Id);

                if (item == GameData.PlayerInv.Empty) throw new System.Exception("Accidentally retrieved 'Empty' item for ID " + Id.Id() + " or CreateFromBaseId " + (CreateFromBaseId.HasValue ? CreateFromBaseId.Value.Id() : "(none)"));

                string origName = item.ItemName;

                // Nullable<T> Map extension w/ pretty syntax "Level.Map(_ => item.ItemLevel = _);" worked at compile time but caused weird generic type crash in Mono :'(
                
                if (Level.HasValue) item.ItemLevel = Level.Value;
                if (HP.HasValue) item.HP = HP.Value;
                if (Mana.HasValue) item.Mana = Mana.Value;
                if (AC.HasValue) item.AC = AC.Value;

                if (Str.HasValue) item.Str = Str.Value;
                if (Dex.HasValue) item.Dex = Dex.Value;
                if (Agi.HasValue) item.Agi = Agi.Value;
                if (End.HasValue) item.End = End.Value;
                if (Int.HasValue) item.Int = Int.Value;
                if (Wis.HasValue) item.Wis = Wis.Value;
                if (Cha.HasValue) item.Cha = Cha.Value;
                if (Res.HasValue) item.Res = Res.Value;
                if (Value.HasValue) item.ItemValue = Value.Value;
                
                if (Name != null) item.name = item.ItemName = Name;
                if (Lore != null) item.Lore = Lore;

                if (SlotType.HasValue) item.RequiredSlot = SlotType.Value;
                if (AppearanceType != null) item.EquipmentToActivate = AppearanceType;

                if (ShoulderTrim.HasValue)
                {
                    item.ShoulderTrimL = ShoulderTrim.Value.Item1;
                    item.ShoulderTrimR = ShoulderTrim.Value.Item2;
                }
                if (ElbowTrim.HasValue)
                {
                    item.ElbowTrimL = ElbowTrim.Value.Item1;
                    item.ElbowTrimR = ElbowTrim.Value.Item2;
                }
                if (KneeTrim.HasValue)
                {
                    item.KneeTrimL = KneeTrim.Value.Item1;
                    item.KneeTrimR = KneeTrim.Value.Item2;
                }

                if (ColorsMain.HasValue)
                {
                    item.ItemPrimaryColor = ColorsMain.Value.Item1;
                    item.ItemSecondaryColor = ColorsMain.Value.Item2;
                }
                if (ColorsLeather.HasValue)
                {
                    item.ItemLeatherPrimary = ColorsLeather.Value.Item1;
                    item.ItemLeatherSecondary = ColorsLeather.Value.Item2;
                }
                if (ColorsMetal.HasValue)
                {
                    item.ItemMetalPrimary = ColorsMetal.Value.Item1;
                    item.ItemMetalSecondary = ColorsMetal.Value.Item2;
                }

                if (Classes != null) item.Classes = Classes;
                else
                {
                    if (item.Classes == null) item.Classes = new List<Class>();
                    if (!item.Classes.Contains(GameData.ClassDB.Arcanist)) item.Classes.Add(GameData.ClassDB.Arcanist);
                }

                if (CreateFromBaseId.HasValue)
                    Main.Log.LogInfo($"Item created: {Name}");
                else
                    Main.Log.LogInfo($"Item updated: {origName}");

                return item;
            }
        }

        static IEnumerator UpdateItemDatabaseAfterSkillsLoaded(ItemDatabase __instance)
        {
            while (GameData.SkillDatabase == null || !SkillDBStartPatch.IsInitialised()) yield return null;

            Main.Log.LogInfo("Adding Arcanism items to ItemDatabase.");
            var itemDictionary = Traverse.Create(__instance).Field<Dictionary<string, Item>>("itemDict").Value;
            var itemsToAdd = new List<Item>();

            var funeralPyreScroll = __instance.GetItemByID(ItemId.FUNERAL_PYRE);
            string origName = funeralPyreScroll.ItemName;
            funeralPyreScroll.ItemIcon = __instance.GetItemByID("335358").ItemIcon; // Make it purple!
            funeralPyreScroll.ItemName = $"Spell Scroll: {SpellDBStartPatch.FUNERAL_PYRE_NAME_CHANGE}";
            Main.Log.LogInfo($"Item updated: {origName}");

            var controlChant = __instance.GetItemByID(ItemId.CONTROL_CHANT);
            itemsToAdd.Add(CreateSkillBook(controlChant, ItemId.EXPERT_CONTROL, "Expert Control I", 15, SkillDBStartPatch.EXPERT_CONTROL_SKILL_ID, 33000)); // Sold by Edwin in Port Azure
            itemsToAdd.Add(CreateSkillBook(controlChant, ItemId.EXPERT_CONTROL_2, "Expert Control II", 32, SkillDBStartPatch.EXPERT_CONTROL_2_SKILL_ID, 49000)); // Dropped by Elwio the Traitor in Vitheo's Rest
            itemsToAdd.Add(CreateSkillBook(controlChant, ItemId.TWIN_SPELL, "Twin Spell", 13, SkillDBStartPatch.TWIN_SPELL_SKILL_ID, 28000)); // Sold by Edwin in Port Azure
            itemsToAdd.Add(CreateSkillBook(controlChant, ItemId.TWIN_SPELL_2, "Twin Spell Mastery", 28, SkillDBStartPatch.TWIN_SPELL_MASTERY_SKILL_ID, 41000)); // Dropped by Fenton the Blighted in The Blight

            UpdateLegPieces(__instance, itemsToAdd);
            UpdateChestPieces(__instance, itemsToAdd);
            UpdateTheFeets(__instance, itemsToAdd);

            int oldItemDbLength = __instance.ItemDB.Length;
            System.Array.Resize(ref __instance.ItemDB, __instance.ItemDB.Length + itemsToAdd.Count);
            for (var i = 0; i < itemsToAdd.Count; i++)
            {
                var item = itemsToAdd[i];

                if (itemDictionary.ContainsKey(item.Id))
                    throw new System.Exception($"Unable to add item {item.ItemName} as item with ID {item.Id} already exists: {itemDictionary[item.Id].ItemName}");

                __instance.ItemDB[oldItemDbLength + i] = item;
                itemDictionary.Add(item.Id, item);
                Main.Log.LogInfo($"New item added to database: {item.ItemName}");
            }

            __instance.ItemDBList = new List<Item>(__instance.ItemDB);

            RefreshSprites(__instance);
        }
        public static int weaveIndex;

        public static void RefreshSprites(ItemDatabase itemDb = null)
        {
            if (itemDb == null) itemDb = GameData.ItemDB;
            foreach (var entry in Main.itemSpriteById)
            {
                itemDb.GetItemByID(entry.Key).ItemIcon = entry.Value;
            }
            Main.Log.LogInfo($"Updated graphics for {Main.itemSpriteById.Count} items");
        }

        static void UpdateLegPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            itemsToAdd.Add(new EquipmentGenerator { 
                CreateFromBaseId = ItemId.REED_TROUSERS, 
                Id = ItemId.TRICKSTERS_PANTS, 
                Name = "Trickster's Pants", 
                Lore = "Belonged to a carnival man whose particular taste in magic tricks resulted in his being executed.",
                Level = 8,	HP = 15,	Mana = 30,	AC = 13,	End = 2,	Int = 4,    Wis = 2,	Cha = 4,	Res = 0,	Value = 500, 
                Str = 0,    Dex = 2,    Agi = 2,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist, GameData.ClassDB.Stormcaller },
                AppearanceType = "ReinforcedLeatherPants",
                ColorsMain = (new Color32(90, 45, 45, 255), new Color32(120, 120, 120, 255)),
                ColorsLeather = (new Color32(160, 160, 160, 255), new Color32(120, 120, 120, 255)),
            }.Generate(__instance));

            itemsToAdd.Add(new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.COMPRESSION_WRAPS,
                Id = ItemId.TATTERED_WRAP,
                Name = "Tattered Wrap",
                Lore = "These may have once belonged to a powerful court wizard, but any remaining traces of magic are faint indeed.",
                Level = 14,	HP = 0,	    Mana = 60,	AC = 5,	    End = 0,	Int = 10,   Wis = 8,	Cha = 1,	Res = 0,	Value = 1500,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist, GameData.ClassDB.Druid },
                AppearanceType = "ClothPants",
                ColorsMain = (new Color32(60, 57, 52, 255), new Color32(80, 74, 70, 255)),
                ColorsLeather = (new Color32(50, 56, 45, 255), new Color32(50, 56, 45, 255)),
                
            }.Generate(__instance));

            itemsToAdd.Add(new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.BRAXONIAN_LINENS,
                Id = ItemId.LUNAR_WEAVE,
                Name = "Lunar Weave",  
                Lore = "This unusually silky and light lower enrobement literally pulses with latent magical energy.",
                Level = 39,	HP = 130,	Mana = 250,	AC = 30,	End = 10,	Int = 35,   Wis = 25,	Cha = 25,	Res = 4,	Value = 26500,
                AppearanceType = "ClothPants",
                ColorsMain = (new Color32(80, 55, 80, 255), new Color32(190, 150, 0, 255)),
                ColorsLeather = (new Color32(80, 55, 80, 255), new Color32(190, 150, 0, 255)),
                Classes = new List<Class>() { GameData.ClassDB.Arcanist }
            }.Generate(__instance));

            new EquipmentGenerator { Id = ItemId.CAVESILK_TROUSERS, Level = 20, HP = 40, Mana = 75, AC = 10, End = 3, Int = 10, Wis = 4, Cha = 15, Res = 1, Value = 2500, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.LINEN_BEDPANTS, Level = 24, HP = 75, Mana = 75, AC = 20, End = 0, Int = 16, Wis = 4, Cha = 2, Res = 0, Value = 5100, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.STARCHED_WOOL_LEGGINGS, Level = 28, HP = 140, Mana = 60, AC = 55, End = 14, Int = 9, Wis = 17, Cha = 6, Res = 2, Value = 9000, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_BREECHES, Level = 29, HP = 0, Mana = 140, AC = 20, End = 0, Int = 25, Wis = 16, Cha = 0, Res = 3, Value = 11200, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_LINENS, Level = 31, HP = 50, Mana = 150, AC = 30, End = 10, Int = 20, Wis = 26, Cha = 10, Res = 2, Value = 12500, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ILLUSIONIST_TROUSERS, Level = 33, HP = 65, Mana = 200, AC = 10, End = 0, Int = 25, Wis = 12, Cha = 20, Res = 2, Value = 18545, }.Generate(__instance);
        }

        static void UpdateChestPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            itemsToAdd.Add(new EquipmentGenerator { 
                CreateFromBaseId = ItemId.FUNERAL_GARB,
                Id = ItemId.NOVICE_ROBE, 
                Name = "Novice's Robe", 
                Lore = "The hallmark of a fledgling magician.",
                Level = 6,	HP = 20,	Mana = 25,	AC = 14,	End = 4,	Int = 5,    Wis = 4,	Cha = 3,	Res = 1,	Value = 650, 
                Classes = new List<Class>() { GameData.ClassDB.Arcanist },
                AppearanceType = "ReinforcedLeatherPants",
                ColorsMain = (new Color32(100, 78, 65, 255), new Color32(100, 78, 65, 255)),
                ColorsLeather = (new Color32(50, 40, 36, 255), new Color32(120, 107, 78, 255)),
            }.Generate(__instance));

            new EquipmentGenerator { Id = ItemId.SPIDERSILK_SHIRT, HP = 60, Mana = 55, AC = 27, End = 2, Int = 10, Wis = 7, Cha = 7, Res = 1, Value = 950, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_WRAP, HP = 70, Mana = 120, AC = 28, End = 4, Int = 15, Wis = 14, Cha = 9, Res = 2, Value = 2950, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.PRESERVED_CLOTH_COAT, HP = 170, Mana = 110, AC = 36, End = 12, Int = 18, Wis = 18, Cha = 9, Res = 1, Value = 5500, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_DRESSCOAT, HP = 200, Mana = 200, AC = 38, End = 6, Int = 23, Wis = 23, Cha = 15, Res = 2, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_ROYAL_TUNIC, HP = 280, Mana = 370, AC = 40, End = 0, Int = 30, Wis = 29, Cha = 21, Res = 3, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ELDOTHS_FINERY, HP = 650, Mana = 600, AC = 45, End = 10, Int = 45, Wis = 44, Cha = 27, Res = 5, }.Generate(__instance);
        }

        static void UpdateTheFeets(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.DEVOUT_MOCCASINS, HP = 15, Mana = 60, AC = 15, End = 5, Int = 8, Wis = 13, Cha = 2, Res = 1, Value = 1900, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.FLAMESOLES, HP = 90, Mana = 120, AC = 23, End = 12, Int = 13, Wis = 15, Cha = 11, Res = 2, Value = 3000, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ELDERS_SLIPPERS, HP = 125, Mana = 250, AC = 20, End = 5, Int = 18, Wis = 24, Cha = 8, Res = 2, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_ROYAL_SHOES, HP = 135, Mana = 250, AC = 21, End = 5, Int = 23, Wis = 24, Cha = 15, Res = 2, Value = 15000, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.PIOUS_SANDALS, HP = 150, Mana = 290, AC = 25, End = 8, Int = 25, Wis = 30, Cha = 22, Res = 3, Value = 20500, }.Generate(__instance);
        }
    }
}