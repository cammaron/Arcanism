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
        SHADOWSTEP_SHOES = 27008672,
        BRAXONIAN_ROYAL_SHOES = 59276768,
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

        // Waist
        OGRESKIN_CORD = 1727100,
        BRAXONIAN_SASH = 75333375,
        SWAMPY_LOIN_CLOTH = 41401668,
        SASH_OF_THE_LOST_GUARD = 2265408,
        PRISMATIC_WARCORD = 49183856,
        FLOWING_BLIGHT_SILK_SASH = 26587955,
        CHARMED_BELT = 39940675,
        ROYAL_WAISTBAND = 63410245,

        // Back
        WARDWARPED_CAPE = 43316376,
        AZYNTHIAN_CAPE = 13444192,

        // Head
        LOST_WIZARDING_HAT = 8183875,
        SIRAETHES_PRAYERCREST = 1884800,
        SIVAKAYAN_TRICORNER = 35666952,

        // Bracer/wrist
        RUNEWOVEN_BRACER = 11364474,
        TWISTED_SPIDER_LEG = 15167780,
        IGNITING_BRACE = 21309534,
        CHITIN_BRACER = 4538569,
        MOONSTONE_BANGLE = 3338832,
        ERODED_BANGLE = 33256192,
        BRAXONIAN_BRACELET = 15458050,
        BLESSED_BRAXONIAN_BRACELET = 14217632,
        PERFECT_LINKS = 1824224,
        WARDED_BRACE = 45884862,
        BLIGHTED_SILK_WRAPS = 17677440,
        GARDENERS_WRIST_WRAP = 173442,
        BRACELET_OF_VESSEL = 46442784,
        RESONATING_BRACE = 12451740,
        FUNGAL_WRAPPED_BRACER = 16581714,

        // Arm/shoulder
        DESERT_SILK_SLEEVES = 5557329,
        FUNGUS_COVERED_ARMBAND = 706180,
        INTRICATE_SLEEVES = 45089611,
        GIFTED_SLEEVES = 9671598,
        BRAXONIAN_ROYAL_ARMBAND = 20398309,
        DREAMY_SLEEVES = 30698425,
        ARMBANDS_OF_ORDER = 66574848,
        BONEBANDED_ARMGUARD = 62353340,

        // Neck
        CREST_OF_BRAXONIA = 12271853,

        // Weapons/Offhands
        COPPER_SCEPTRE = 7308447,
        WEAK_WAND = 58835400,
        ADEPT_WAND = 32620720,
        WAND_OF_AIR = 1624168,
        EYESTALK_WAND = 55277254,
        HARDENED_SCEPTRE = 41050704,
        ARGOS_GRIMOIRE = 39840084,
        MAGUS_SHIELD = 43055028,
        OGRESPICE_BUNDLE = 79500585,
        SPECTRAL_SCEPTRE = 3794175,
        PEARLESCENT_KELP_TOTEM = 4853466,
        BRAXONIAN_SHIELD = 16758324,
        DIAMONDINE_SHIELD = 29324202,
        ROYAL_CARAPACE = 84620118,
        BLACKFLAME_TORCH = 20176773,
        SCORCHED_WALKING_STICK = 7912436,
        FUNGAL_BOUQUET = 4214124,
        BONEWEAVERS_LEG = 31427040,
        CINDER_OF_BIRTH = 29424696,
        DREAMY_WAND = 655914,
        CRYSTALLISED_TACTICS = 24566942,
        SIVAKAYAN_SCEPTRE = 3239916,
        CELESTIAL_SPIKE = 61040760,
        MEMORIES_OF_SNOW = 1345764,
        SIVAKAYAN_WAND = 23926573,
        RUNED_SHIELD = 21833055,
        VOLCANIC_SCEPTRE = 46328832,
        RESONATING_CRYSTAL = 2270100,
        BRAXS_CANDLE = 47409984,
        BRAXONIAN_TESTAMENT = 11581290,
        GLOWING_BLUE_STONE = 25539100,
        SIVA_BRAXONIAN_TEACHINGS = 18034240,
        PETRIFIED_WOOD_CANE = 49375795,
        GARG_WAND = 52381840,
        ULORS_ENCYCLOPEDIA = 28820532,
        ASCENDED_REMAINS = 53997971,
        SINGULARITY_VESSEL_OF_CREATION = 11537478,

        // Scrolls
        FUNERAL_PYRE = 17433350,
        MAGICAL_SKIN = 30972000,

        // BOOKS
        CONTROL_CHANT = 27169650,
        ARCANE_RECOVERY = 49058786,

        // Treasure Map pieces
        MAP_PIECE_1 = 6188236,
        MAP_PIECE_2 = 28043030,
        MAP_PIECE_3 = 28362792,
        MAP_PIECE_4 = 270986,
        FULL_MAP_1 = 36302700,
        // ^ remaining map pieces below in Custom Items

        // CONSUMBLES
        BREAD = 11823624,

        // Custom items
        EXPERT_CONTROL = 90000000,
        EXPERT_CONTROL_2 = 90000001,
        TWIN_SPELL = 90000002,
        TWIN_SPELL_2 = 90000003,

        TRICKSTERS_PANTS = 90000004,
        TATTERED_WRAP = 90000005,
        LUNAR_WEAVE = 90000006,

        NOVICE_ROBE = 90000007,

        TWIN_SPELL_3 = 90000008,
        VANISHING_TWIN = 90000009,
        PARASITIC_TWIN = 90000010,
        SIBLING_SYNERGY = 90000011,
        PERFECT_RELEASE = 90000012,
        PERFECT_RELEASE_2 = 90000013,

        MANA_CHAIN = 90000014,
        VIZIERS_LAMENT = 90000015,

        FULL_MAP_2 = 90000016,
        FULL_MAP_3 = 90000017,
        FULL_MAP_4 = 90000018,

        LUCK_SCROLL_1 = 90000019,
        LUCK_SCROLL_2 = 90000020,
        LUCK_SCROLL_3 = 90000021,
        LUCK_SCROLL_4 = 90000022,
        LUCK_SCROLL_5 = 90000023,

        TIME_IS_POWER = 90000024,
        TIME_IS_POWER_2 = 90000025,
        TIME_IS_POWER_3 = 90000026,
    }

    public enum DropChance
    {
        COMMON,
        UNCOMMON,
        RARE,
        LEGENDARY,
        GUARANTEE_ONE
    }

    static class ItemIdExtensions
    {
        public static string Id(this ItemId itemId) => ((int)itemId).ToString();
        public static Item GetItemByID(this ItemDatabase db, ItemId itemId) => db.GetItemByID(itemId.Id());
    }


    [HarmonyPatch(typeof(ItemDatabase), "Start")]
    public class ItemDatabase_Start
    {
        public static Dictionary<NpcName, HashSet<Item>> itemsSoldByVendor = new Dictionary<NpcName, HashSet<Item>>();
        public static Dictionary<NpcName, HashSet<(DropChance, Item)>> dropsByNpc = new Dictionary<NpcName, HashSet<(DropChance, Item)>>();
        // A map to look up the available set bonuses for an item, by ID. The set bonuses themselves are another map with the ID for the other item that must be equipped,
        // and the value -- an Item itself -- is merely a shell for carrying stats, not needing to be a real Item record
        public static Dictionary<ItemId, Dictionary<ItemId, Item>> setBonusesByItemId = new Dictionary<ItemId, Dictionary<ItemId, Item>>();

        private static bool isFinished;
        
        public static bool IsFinished()
        {
            return isFinished;
        }

        public static void Postfix(ItemDatabase __instance)
        {
            UpdateItemDatabase(__instance);
            RegisterSets(__instance);
            isFinished = true;
        }

        public static void UpdateItemDatabase(ItemDatabase __instance)
        {
            Main.Log.LogInfo("Adding and updating items for Arcanism.");

            var itemDictionary = Traverse.Create(__instance).Field<Dictionary<string, Item>>("itemDict").Value;
            var itemsToAdd = new List<Item>();

            var funeralPyreScroll = __instance.GetItemByID(ItemId.FUNERAL_PYRE);
            string origName = funeralPyreScroll.ItemName;
            funeralPyreScroll.ItemIcon = __instance.GetItemByID("335358").ItemIcon; // Make it purple!
            funeralPyreScroll.ItemName = $"Spell Scroll: {SpellDB_Start.FUNERAL_PYRE_NAME_CHANGE}";
            Main.Log.LogInfo($"Item updated: {origName}");

            var skillBookBase = __instance.GetItemByID(ItemId.ARCANE_RECOVERY);

            itemsToAdd.Add(
                DroppedBy(NpcName.ZASHLYN_BLOODBANE, DropChance.UNCOMMON,
                DroppedBy(NpcName.MOURNING, DropChance.UNCOMMON, 
                SoldBy(NpcName.NYLITH_VALORRI, 
                CreateSkillBook(skillBookBase, ItemId.TIME_IS_POWER, SkillDB_Start.TIME_IS_POWER_SKILL_ID, 700))
                )));
            itemsToAdd.Add(SoldBy(NpcName.RORELI_GILMARE, CreateSkillBook(skillBookBase, ItemId.EXPERT_CONTROL, SkillDB_Start.EXPERT_CONTROL_SKILL_ID, 9500)));
            itemsToAdd.Add(SoldBy(NpcName.EDWIN_ANSEGG, CreateSkillBook(skillBookBase, ItemId.TWIN_SPELL, SkillDB_Start.TWIN_SPELL_SKILL_ID, 11500)));
            itemsToAdd.Add(SoldBy(NpcName.RORELI_GILMARE, CreateSkillBook(skillBookBase, ItemId.PERFECT_RELEASE, SkillDB_Start.PERFECT_RELEASE_SKILL_ID, 24500)));
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(skillBookBase, ItemId.TWIN_SPELL_2, SkillDB_Start.TWIN_SPELL_MASTERY_SKILL_ID, 32000)));
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(skillBookBase, ItemId.VANISHING_TWIN, SkillDB_Start.VANISHING_TWIN_SKILL_ID, 40000)));
            itemsToAdd.Add(DroppedBy(NpcName.A_DEEPLING_ORATOR, DropChance.UNCOMMON, CreateSkillBook(skillBookBase, ItemId.TIME_IS_POWER_2, SkillDB_Start.TIME_IS_POWER_2_SKILL_ID, 38000)));
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(skillBookBase, ItemId.EXPERT_CONTROL_2, SkillDB_Start.EXPERT_CONTROL_2_SKILL_ID, 80000)));
            itemsToAdd.Add(DroppedBy(NpcName.SEED_OF_BLIGHT, DropChance.GUARANTEE_ONE, CreateSkillBook(skillBookBase, ItemId.SIBLING_SYNERGY, SkillDB_Start.SIBLING_SYNGERY_SKILL_ID, 65000))); 
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(skillBookBase, ItemId.PARASITIC_TWIN, SkillDB_Start.PARASITIC_TWIN_SKILL_ID, 150000)));
            itemsToAdd.Add(DroppedBy(NpcName.EVADNE_THE_CORRUPTED, DropChance.UNCOMMON, CreateSkillBook(skillBookBase, ItemId.TIME_IS_POWER_3, SkillDB_Start.TIME_IS_POWER_3_SKILL_ID, 98000)));
            itemsToAdd.Add(DroppedBy(NpcName.FENTON_THE_BLIGHTED, DropChance.UNCOMMON, CreateSkillBook(skillBookBase, ItemId.TWIN_SPELL_3, SkillDB_Start.TWIN_SPELL_MASTERY_2_SKILL_ID, 120000))); 
            itemsToAdd.Add(DroppedBy(NpcName.ELWIO_THE_TRAITOR, DropChance.UNCOMMON, CreateSkillBook(skillBookBase, ItemId.PERFECT_RELEASE_2, SkillDB_Start.PERFECT_RELEASE_2_SKILL_ID, 150000)));

            SoldBy(NpcName.CERBANTIAS_FLAMEWARD, __instance.GetItemByID(ItemId.MAGICAL_SKIN));

            UpdateItemValues(__instance);

            UpdateLegPieces(__instance, itemsToAdd);
            UpdateChestPieces(__instance, itemsToAdd);
            UpdateTheFeets(__instance, itemsToAdd);
            UpdateWaistPieces(__instance, itemsToAdd);
            UpdateBackPieces(__instance, itemsToAdd);
            UpdateHeadPieces(__instance, itemsToAdd);
            UpdateWristPieces(__instance, itemsToAdd);
            UpdateShoulderPieces(__instance, itemsToAdd);
            UpdateRings(__instance, itemsToAdd);
            UpdateNeckPieces(__instance, itemsToAdd);
            UpdateWeaponSlots(__instance, itemsToAdd);
            UpdateAuras(__instance, itemsToAdd);
            UpdateTreasureMaps(__instance, itemsToAdd);
            AddLuckScrolls(__instance, itemsToAdd);

            itemsToAdd.RemoveAll(item =>
            {
                bool remove = itemDictionary.ContainsKey(item.Id);
                if (remove) Main.Log.LogInfo($"Skipping adding item {item.ItemName} because an item with ID {item.Id} already exists: {itemDictionary[item.Id].ItemName}");
                return remove;
            });

            int oldItemDbLength = __instance.ItemDB.Length;
            System.Array.Resize(ref __instance.ItemDB, __instance.ItemDB.Length + itemsToAdd.Count);
            for (var i = 0; i < itemsToAdd.Count; i++)
            {
                var item = itemsToAdd[i];
                __instance.ItemDB[oldItemDbLength + i] = item;
                itemDictionary.Add(item.Id, item);
                Main.Log.LogInfo($"New item added to database: {item.ItemName}");
            }

            __instance.ItemDBList = new List<Item>(__instance.ItemDB);

            RefreshSprites(__instance);
        }

        
        /* The vanilla base item valuesare mostly good but there are a few instances of things being valued a lot less than they should be relative to their power, which feels pretty inconsistent.
         * This formula is just to set a floor for equipment prices so that, based on their level (which ROUGHLY corresponds to usefulness), they will always be worth more than items from earlier/weaker enemies.
         * The vanilla price is kept if it was higher. With that in mind I've tried to keep the values for any given level a chunk lower than what they would be for standard decently selling gear of that level
         * so that this only catches stuff that has a pretty crappy price -- inspired by the Moonstone Bangle price disappointment after selling easily farmed leaf capes for 9.5k each moments earlier
         */
        public static void UpdateItemValues(ItemDatabase __instance) {
            var swampyCloth = __instance.GetItemByID(ItemId.SWAMPY_LOIN_CLOTH);
            swampyCloth.ItemLevel = 14; // no fucking clue why this thing had item level 35 before, but i ain't having it sell for 15k
            var allowPriceChange = new HashSet<Item>() { 
                __instance.GetItemByID(ItemId.SIRAETHES_PRAYERCREST),
                __instance.GetItemByID(ItemId.SIVAKAYAN_TRICORNER) 
            };

            foreach(var item in __instance.ItemDB)
            {
                if (item.ItemValue > 0 && (item.IsUpgradeableEquipment() || item.RequiredSlot == Item.SlotType.Aura))
                {
                    int scaledValue = ScaleItemValue(item.ItemLevel, 1f, 15f, 1.105f);
                    if (!allowPriceChange.Contains(item) && scaledValue >= item.ItemValue * 3.5f) // lots of items have a bit less than 1/3 the value they should have, so this is a good cut-off point
                    {
                        Main.Log.LogInfo($"Skipping price update for {item.ItemName} with level {item.ItemLevel} and value {item.ItemValue} because the updated value calculation ({scaledValue}) exceeds anticipated parameters.");
                        continue; // avoid absurd bumps due to things like incorrect item levels or things that were deliberately super low value like rottenfoot sash
                    }
                    if (scaledValue > item.ItemValue)
                    {
                        Main.Log.LogInfo($"Updating price for {item.ItemName} from {item.ItemValue} to {scaledValue}");
                        item.ItemValue = scaledValue;
                    }

                    /* Here's how this formula looks for equippable items:
                    [Leve]: Minimum Value

                    [1]: 1
                    [2]: 18
                    [5]: 91 (Aim for 90)  -- Funeral Shroud (100), Ceremonial Gloves (350), 
                    [7]: 166
                    [10]: 334 (Aim for 340) -- Solunian Armguard (500), Demon's Crest (255), Rottenfoot Sash (4), Sailor's belt (700), Grassland Sap Necklace (300), Rotting Sivakayan helm (1000)
                    [13]: 600
                    [15]: 854 (Aim for 780) -- Twisted Spider Leg (1470), Twilight Belt (1800), Bog Hoop (1000), Explorer's Cap (1500), Nagalok Claws (1500), Fungal Scab Cape (2000), 
                    [17]: 1191
                    [20]: 1907 (Aim for 1950) -- Lost Cape (3500), Igniting Brace (2000), Ancient Guardian Plate (3550), Battleworn Plate (2300), Lost Girdle (3200), Prismatic Warcord (2700)
                    [23]: 2977
                    [25]: 3965 (Aim for 3600) -- Charred Sleeves (2000), Shackle of Bidding (2200), Chitin Protector (6500), Moonstone Bangle (2145), Sivakayan Garb (4000), Preserved Cloth Coat (3300), Gifted Sleeves (2455), 
                    [27]: 5243 -- Red/Blue leaf capes (9500), Shadowstep Shoes (5000), Jacak Hood (6250), Priel Steeel Armguards (3200)
                    [30]: 7888 (Aim for 7200) -- most gear around 9538, some higher like 17k
                    [33]: 11742
                    [35]: 15231 (Aim for 15000)
                    [38]: 22360
                    [40]: 28776*/
                }
                else if (item.TeachSpell != null && item.ItemName.StartsWith("Spell Scroll"))
                {
                    /* Vanilla scroll prices are honestly whack. Most are so cheap at the time you unlock them that the cost is irrelevant, and this goes triply for their value as loot --
                    * because they can't be sold on the AH they're already worth a lot less than equipment, and then the vendor short-changes you too.
                    * With that in mind, presenting... a formula for scroll value so that they're more of a gold sink and also better loot!
                    */
                    int scaledVal = ScaleScrollValue(item.TeachSpell.RequiredLevel, 50f, 100f, 5f, 1.04f);
                    if (scaledVal > item.ItemValue)
                        item.ItemValue = scaledVal;

                    // Here's how it looks:
                    // Scroll Level: Value
                    // 1: 50
                    // 3: 155
                    // 7: 1,050
                    // 10: 2,749
                    // 15: 9,302
                    // 20: 24,649
                    // 25: 57,121
                    // 30: 121,640
                    // 35: 244,419
                }
            }
        }

        protected static int ScaleItemValue(int level, float baseCost, float costPerLevel, float exponentPerLevel)
        {
            float basePerLevelCost = baseCost + (costPerLevel * (level - 1));
            return Mathf.RoundToInt(basePerLevelCost * Mathf.Pow(exponentPerLevel, level - 1));
        }

        protected static int ScaleScrollValue(int level, float baseCost, float costPerLevel, float levelsToIncreaseCostPerLevel, float exponentPerLevel)
        {
            float scaledCostPerLevel = costPerLevel * ((level - 1) / levelsToIncreaseCostPerLevel); // the cost per level, itself, increases per level! This could probs have been reflected in a more simple formula
            float basePerLevelCost = baseCost + (scaledCostPerLevel * level);
            return Mathf.RoundToInt(basePerLevelCost * Mathf.Pow(exponentPerLevel, level - 1));
        }

        static void UpdateLegPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            itemsToAdd.Add(DroppedBy(NpcName.MOLORAI_MILITIA_ARCANIST, DropChance.RARE, new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.REED_TROUSERS,
                Id = ItemId.TRICKSTERS_PANTS,
                Name = "Trickster's Pants",
                Lore = "Belonged to a carnival man whose particular taste in magic tricks resulted in his being executed.",
                Level = 8,
                HP = 15,
                Mana = 30,
                AC = 13,
                End = 2,
                Int = 4,
                Wis = 2,
                Cha = 4,
                Res = 0,
                Value = 500,
                Str = 0,
                Dex = 2,
                Agi = 2,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist, GameData.ClassDB.Stormcaller },
                AppearanceType = "ReinforcedLeatherPants",
                ColorsMain = (new Color32(90, 45, 45, 255), new Color32(120, 120, 120, 255)),
                ColorsLeather = (new Color32(160, 160, 160, 255), new Color32(120, 120, 120, 255)),
            }.Generate(__instance)));

            itemsToAdd.Add(DroppedBy(NpcName.PRIEL_DECEIVER, DropChance.RARE, new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.COMPRESSION_WRAPS,
                Id = ItemId.TATTERED_WRAP,
                Name = "Tattered Wrap",
                Lore = "These may have once belonged to a powerful court wizard, but any remaining traces of magic are faint indeed.",
                Level = 14,
                HP = 0,
                Mana = 60,
                AC = 5,
                End = 0,
                Int = 10,
                Wis = 8,
                Cha = 1,
                Res = 0,
                Value = 1500,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist, GameData.ClassDB.Druid },
                AppearanceType = "ClothPants",
                ColorsMain = (new Color32(60, 57, 52, 255), new Color32(80, 74, 70, 255)),
                ColorsLeather = (new Color32(50, 56, 45, 255), new Color32(50, 56, 45, 255)),

            }.Generate(__instance)));

            itemsToAdd.Add(DroppedBy(NpcName.VESSEL_SIRAETHE, DropChance.RARE, new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.BRAXONIAN_LINENS,
                Id = ItemId.LUNAR_WEAVE,
                Name = "Lunar Weave",
                Lore = "This unusually silky and light lower enrobement literally pulses with latent magical energy.",
                Level = 39,
                HP = 130,
                Mana = 250,
                AC = 30,
                End = 10,
                Int = 30,
                Wis = 25,
                Cha = 30,
                Res = 4,
                Value = 27500,
                AppearanceType = "ClothPants",
                ColorsMain = (new Color32(80, 55, 80, 255), new Color32(190, 150, 0, 255)),
                ColorsLeather = (new Color32(80, 55, 80, 255), new Color32(190, 150, 0, 255)),
                Classes = new List<Class>() { GameData.ClassDB.Arcanist }
            }.Generate(__instance)));

            new EquipmentGenerator { Id = ItemId.CAVESILK_TROUSERS, Level = 20, HP = 40, Mana = 75, AC = 10, End = 3, Int = 10, Wis = 4, Cha = 15, Res = 1, Value = 2500, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.LINEN_BEDPANTS, Level = 24, HP = 75, Mana = 75, AC = 20, End = 0, Int = 16, Wis = 4, Cha = 2, Res = 0, Value = 5100, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.STARCHED_WOOL_LEGGINGS, Level = 28, HP = 140, Mana = 60, AC = 55, End = 14, Int = 9, Wis = 17, Cha = 6, Res = 2, Value = 9000, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_BREECHES, Level = 29, HP = 0, Mana = 140, AC = 20, End = 0, Int = 25, Wis = 16, Cha = 0, Res = 3, Value = 11200, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_LINENS, Level = 31, HP = 50, Mana = 150, AC = 30, End = 10, Int = 20, Wis = 26, Cha = 10, Res = 2, Value = 12500, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ILLUSIONIST_TROUSERS, Level = 33, HP = 65, Mana = 200, AC = 10, End = 0, Int = 25, Wis = 12, Cha = 20, Res = 2, Value = 18545, }.Generate(__instance);
        }

        static void UpdateChestPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            itemsToAdd.Add(SoldBy(NpcName.EDWIN_ANSEGG, new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.FUNERAL_GARB,
                Id = ItemId.NOVICE_ROBE,
                Name = "Novice's Robe",
                Lore = "The hallmark of a fledgling magician.",
                Level = 6,
                HP = 20,
                Mana = 25,
                AC = 14,
                End = 4,
                Int = 5,
                Wis = 4,
                Cha = 3,
                Res = 1,
                Value = 650,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist },
            }.Generate(__instance)));

            DroppedBy(NpcName.RISEN_DRUID, DropChance.UNCOMMON, new EquipmentGenerator { Id = ItemId.SPIDERSILK_SHIRT, HP = 60, Mana = 55, AC = 27, End = 2, Int = 10, Wis = 7, Cha = 7, Res = 1, Value = 950, }.Generate(__instance));
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_WRAP, HP = 70, Mana = 120, AC = 28, End = 4, Int = 15, Wis = 14, Cha = 9, Res = 2, Value = 2950, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.PRESERVED_CLOTH_COAT, HP = 170, Mana = 110, AC = 36, End = 12, Int = 18, Wis = 18, Cha = 9, Res = 1, Value = 5500, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_DRESSCOAT, HP = 200, Mana = 200, AC = 38, End = 6, Int = 23, Wis = 23, Cha = 15, Res = 2, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_ROYAL_TUNIC, HP = 350, Mana = 800, AC = 45, End = 5, Int = 30, Wis = 30, Cha = 30, Res = 5, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ELDOTHS_FINERY, HP = 650, Mana = 600, AC = 60, End = 15, Int = 36, Wis = 25, Cha = 36, Res = 4, }.Generate(__instance);
        }

        static void UpdateTheFeets(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.DEVOUT_MOCCASINS, HP = 15, Mana = 60, AC = 15, End = 5, Int = 8, Wis = 13, Cha = 2, Res = 1, Value = 1900, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.FLAMESOLES, HP = 90, Mana = 120, AC = 23, End = 12, Int = 13, Wis = 15, Cha = 11, Res = 2, Value = 3000, }.Generate(__instance);
            var shadowSteps = new EquipmentGenerator { Id = ItemId.SHADOWSTEP_SHOES, HP = 170, Mana = 170, AC = 35, End = 8, Dex = 12, Agi = 12, Int = 20, Wis = 11, Cha = 16, Res = 2, }.Generate(__instance);
            shadowSteps.WornEffect = GameData.SpellDatabase.GetSpellByID("108204"); // Adding flight of foot 1 becasue it makes sense!
            new EquipmentGenerator { Id = ItemId.ELDERS_SLIPPERS, HP = 125, Mana = 400, AC = 12, End = 4, Int = 20, Wis = 27, Cha = 8, Res = 3, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_ROYAL_SHOES, HP = 135, Mana = 250, AC = 21, End = 5, Int = 23, Wis = 15, Cha = 25, Res = 2, Value = 15000, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.PIOUS_SANDALS, HP = 150, Mana = 290, AC = 25, End = 8, Int = 25, Wis = 30, Cha = 22, Res = 3, Value = 20500, }.Generate(__instance);
        }

        static void UpdateWaistPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.OGRESKIN_CORD, HP = 28, Mana = 35, AC = 14, End = 5, Int = 6, Wis = 4, Cha = 0, Res = 0, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SASH_OF_THE_LOST_GUARD, HP = 200, Mana = 100, AC = 35, End = 10, Int = 5, Wis = 8, Cha = 4, }.Generate(__instance); 
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_SASH, HP = 100, Mana = 200, AC = 0, End = 0, Int = 15, Wis = 12, Cha = 8, Res = 1, }.Generate(__instance);
            itemsToAdd.Add(DroppedBy(NpcName.DIAMOND_HOUND, DropChance.RARE, new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.PRISMATIC_WARCORD,
                Id = ItemId.MANA_CHAIN,
                Level = 25,
                Name = "Mana Chain",
                Lore = "A colourful belt infused with sparkling rainbow magic.",
                HP = 150,
                Mana = 300,
                AC = 25,
                End = 0,
                Int = 15,
                Wis = 12,
                Cha = 15,
                Res = 3,
                Value = 8500,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist }
            }.Generate(__instance)));
            new EquipmentGenerator { Id = ItemId.CHARMED_BELT, HP = 180, Mana = 200, AC = 16, End = 5, Agi = 5, Int = 12, Wis = 9, Cha = 23, Res = 2, Value = 11200 }.Generate(__instance); 
            new EquipmentGenerator { Id = ItemId.FLOWING_BLIGHT_SILK_SASH, HP = 10, Mana = 300, AC = 0, Str = 0, Dex = 0, Agi = 10, End = 0, Int = 18, Wis = 18, Cha = 18, Value = 15500, Res = 3 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ROYAL_WAISTBAND, HP = 240, Mana = 200, AC = 30, End = 18, Agi=5, Int = 18, Wis = 9, Cha = 18, Value = 17800, Res = 3 }.Generate(__instance);
            itemsToAdd.Add(DroppedBy(NpcName.BLIGHT_WYRM, DropChance.GUARANTEE_ONE, new EquipmentGenerator
            {
                CreateFromBaseId = ItemId.BRAXONIAN_SASH,
                Id = ItemId.VIZIERS_LAMENT,
                Name = "Vizier's Lament",
                Lore = "After the Blight Wyrm simply refused to listen to diplomacy, the vizier was said to have... well, died. And been eaten. This thing still smells a bit.",
                Level = 38,
                HP = 100,
                Mana = 450,
                AC = 10,
                End = 5,
                Agi = 15,
                Int = 28,
                Wis = 12,
                Cha = 36,
                Res = 5,
                Value = 30000,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist, GameData.ClassDB.Stormcaller }
            }.Generate(__instance)));
        }

        static void UpdateBackPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.WARDWARPED_CAPE, HP = 600, Mana = 0, AC = 70, End = 0, Dex=15, Agi=15, Int = 11, Wis = 10, Cha = 13, Res = 7 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.AZYNTHIAN_CAPE, HP = 200, Mana = 400, AC = 20, Dex = 0, Agi = 0, Str = 0, End = 0, Int = 22, Wis = 15, Cha = 26, Res = 4 }.Generate(__instance);
        }

        static void UpdateHeadPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            DroppedBy(NpcName.ARCANE_PUPIL, DropChance.UNCOMMON, new EquipmentGenerator { Id = ItemId.LOST_WIZARDING_HAT, Res = 4 }.Generate(__instance));
        }

        static void UpdateWristPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.RUNEWOVEN_BRACER, HP = 0, Mana = 20, Int = 6, Wis = 2, Cha = 2, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.TWISTED_SPIDER_LEG, HP = 50, Mana = 28, AC = 12, Int = 8, Wis = 6, Cha = 0, Res = 0 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.IGNITING_BRACE, HP = 40, Mana = 40, AC = 5, Int = 18, Wis = 0, Cha = 0, Res = 0 }.Generate(__instance).WeaponProcChance = 10;
            new EquipmentGenerator { Id = ItemId.CHITIN_BRACER, HP = 65, Mana = 65, AC = 15, Int = 12, Wis = 14, Cha = 0, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.MOONSTONE_BANGLE, HP = 0, Mana = 130, AC = 0, Int = 15, Wis = 15, Cha = 15, Res = 3 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BLIGHTED_SILK_WRAPS, HP = 120, Mana = 120, End = 0, AC = 0, Agi = 15, Dex = 15, Int = 15, Wis = 15, Cha = 15, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ERODED_BANGLE }.Generate(__instance).WeaponProcChance = 6;
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_BRACELET, HP = 100, Mana = 150, AC = 22, End = 12, Int = 19, Wis = 20, Cha = 8, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BLESSED_BRAXONIAN_BRACELET, HP = 150, Mana = 200, AC = 30, End = 14, Int = 24, Wis = 24, Cha = 11, Res = 3, Value = 11000 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.PERFECT_LINKS, HP = 80, Mana = 200, AC = 15, Int = 6, Wis = 12, Cha = 26, Res = 5 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.WARDED_BRACE, HP = 500, Mana = 200, End = 15, AC = 70, Int = 5, Wis = 14, Cha = 5, Res = 3 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.GARDENERS_WRIST_WRAP, HP = 220, Mana = 400, Int = 15, Wis = 30, Cha = 7, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRACELET_OF_VESSEL, HP = 50, Mana = 300, AC = 12, Int = 26, Wis = 0, Cha = 20, Res = 4 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.RESONATING_BRACE, HP = 0, Mana = 300, AC = 40, Int = 5, Wis = 8, Cha = 5, Res = 8 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.FUNGAL_WRAPPED_BRACER, HP = 150, Mana = 150, AC = 40, Int = 8, Wis = 8, Cha = 2, Res = 8 }.Generate(__instance);
        }

        static void UpdateShoulderPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.DESERT_SILK_SLEEVES, HP = 0, Mana = 80, AC = 0, Str = 0, Dex = 0, End = 0, Int = 10, Wis = 2, Cha = 7, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.FUNGUS_COVERED_ARMBAND, HP = 80, Mana = 40, AC = 15, Str = 0, Dex = 0, End = 8, Int = 7, Wis = 15, Cha = 4, Res = 0 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.INTRICATE_SLEEVES, HP = 90, Mana = 90, AC = 15, End = 8, Int = 11, Wis = 6, Cha = 16, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.GIFTED_SLEEVES, HP = 30, Mana = 120, AC = 3, Str = 0, End = 0, Int = 23, Wis = 19, Cha = 6, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_ROYAL_ARMBAND, HP = 250, Mana = 250, AC = 30, End = 12, Int = 20, Wis = 11, Cha = 20, Res = 2 }.Generate(__instance);

            new EquipmentGenerator { Id = ItemId.DREAMY_SLEEVES, HP = 0, Mana = 350, AC = 0, End = 0, Int = 24, Wis = 12, Cha = 26, Res = 4, Classes = new List<Class>() { GameData.ClassDB.Arcanist } }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ARMBANDS_OF_ORDER, HP = 280, Mana = 120, AC = 60, Str = 20, End = 25, Int = 10, Wis = 10, Cha = 10, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BONEBANDED_ARMGUARD, HP = 180, Mana = 300, AC = 40, End = 15, Int = 18, Wis = 25, Cha = 12, Res = 3 }.Generate(__instance);
        }

        static void UpdateRings(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            // No changes needed, rings look pretty good already
        }

        static void UpdateNeckPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            // decent variety of available amulets with interesting stat differences, not much to change
            new EquipmentGenerator { Id = ItemId.CREST_OF_BRAXONIA, HP = 180, Mana = 180, AC = 20, End = 15 }.Generate(__instance); // needs bigger core stats to compete wtih Soul Echo's Mana Charge II
        }

        static void UpdateWeaponSlots(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.WEAK_WAND }.TuneWand(7, 1, 10).Generate(__instance); // Competing with: Ice Bolt: 19 damage
            new EquipmentGenerator { Id = ItemId.COPPER_SCEPTRE }.TuneWand(24, 2, 25).Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ADEPT_WAND }.TuneWand(22, 1, 18).Generate(__instance);
            new EquipmentGenerator { Id = ItemId.WAND_OF_AIR }.TuneWand(29, 1, 18, SpellDB_Start.JOLT_SPELL_ID, 10).Generate(__instance); // Competing with: Jolt: 200
            new EquipmentGenerator { Id = ItemId.HARDENED_SCEPTRE }.TuneWand(80, 2, 25).Generate(__instance);
            new EquipmentGenerator { Id = ItemId.EYESTALK_WAND }.TuneWand(65, 2, 7, null, 25).Generate(__instance);

            new EquipmentGenerator { Id = ItemId.ARGOS_GRIMOIRE, HP = 0, Mana = 60, AC = 0, Int = 8, Wis = 7, Cha = 2, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.MAGUS_SHIELD, HP = 40, Mana = 18, AC = 25, Int = 4, Wis = 4, Cha = 6, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.OGRESPICE_BUNDLE, HP = 45, Mana = 0, AC = 0, Int = 7, Wis = 0, Cha = 6, Res = 0 }.TuneWand(80, 3, 18, null, 50).Generate(__instance); // fitty percent chance to cast sleep, decent range, but slower than vanilla, and not a good DPS option

            // Here's an interesting one -- by default this does no damage and is a 1 hander. Let's change it to be a 2 hander with no damage, completely disabling regular attacks, but with good stats to buff casting.
            // NB It competes with Diamondine/Royal Carapace + weapons from Spectral Sceptre to Scorched W Stick
            new EquipmentGenerator { Id = ItemId.PEARLESCENT_KELP_TOTEM, Damage = 0, HP = 20, Mana = 110, AC = 5, Int = 30, Wis = 20, Cha = 0, Res = 7, SlotType = Item.SlotType.Primary, WeaponType = Item.WeaponType.TwoHandStaff }.Generate(__instance);

            // Braxonian: Defense, Int,Wis
            // Diamondine: Magic, Int,Cha, Res
            // Royal Carapace: Balanced
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_SHIELD, HP = 250, Mana = 65, AC = 50, End = 13, Int = 10, Wis = 10, Cha = 5, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.DIAMONDINE_SHIELD, HP = 65, Mana = 250, AC = 20, End = 0, Int = 12, Wis = 4, Cha = 12, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ROYAL_CARAPACE, HP = 160, Mana = 160, AC = 35, End = 6, Int = 9, Wis = 9, Cha = 9, Res = 1 }.Generate(__instance);

            // Cinder of Birth: despite higher item level this actually drops 25% from Beady, lvl 16 in swamp who isn't hard, so could be gotten around lvl 14...
            // it's a close range weapon with chance to cast Brax's Rage which is a relatively poweful medium spell now, so it's a fantastic item for this level
            // Let's make it interesting by giving it a high damage, low frequency attack -- encouraging a meta of darting in to hit and retreating
            // Icnreasing Brax's Rage proc chance a lot to account for the lower attack speed. This will make it feel really heavy whenever it procs!
            new EquipmentGenerator
            {
                Id = ItemId.CINDER_OF_BIRTH,
                HP = 30,
                Mana = 30,
                Int = 4,
                Wis = 12,
                Cha = 4,
                Res = 1,  // Competing with: Ice Shock 360, Ice Spear 1920
                WandBoltSpeed = 5,
                WandBoltColour = new Color32(255, 108, 0, 255)
            }.TuneWand(360, 3, 5, null, 45).Generate(__instance);


            // the next 4 are all about teh same level drops
            // Blackflame is rare and has nothing special, so buff it to be the best cast augmentor of the three
            // Make Scorched Walking Stick into an interesting 2 hander with early access to Brax's Fury but with a longer cast time, which does similar damage to current level's heavy spell (Winter's Bite) but now faster (and free)
            // Boneweaver's gets nerfs relative to peers but higher raw wand DPS, and the benefit of its amazing DoT
            // Fungal Bouquet... vanilla spell proc is utterly useless at this level. Let's make it have poor dmg, fantastic int/defense, *NEGATIVE* cha mod
            // So now, there's actually interesting choices at this level!
            new EquipmentGenerator { Id = ItemId.BLACKFLAME_TORCH, Damage = 6, AttackDelay = 1, HP = 0, Mana = 90, Int = 17, Wis = 5, Cha = 13, Res = 3 }.TuneWand(180, 2, 25).Generate(__instance);
            new EquipmentGenerator
            {
                Id = ItemId.SCORCHED_WALKING_STICK,
                ClickEffect = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.SCORCHED_FURY_SPELL_ID),
                HP = 60,
                Mana = 100,
                AC = 14,
                End = 7,
                Int = 20,
                Wis = 30,
                Cha = 6,
                Res = 4,
                SlotType = Item.SlotType.Primary,
                WeaponType = Item.WeaponType.TwoHandStaff
            }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BONEWEAVERS_LEG, HP = 25, Mana = 60, AC = 0, End = 0, Int = 12, Wis = 8, Cha = 3, Res = 2 }.TuneWand(140, 1, 18).Generate(__instance);
            new EquipmentGenerator { Id = ItemId.FUNGAL_BOUQUET, HP = 120, Mana = 50, AC = 40, End = 15, Int = 25, Wis = 5, Cha = -10, Res = 0 }.TuneWand(150, 1.5f, 12).Generate(__instance);

            // Shivering Step drop, requires going out of your way, needs to be worth it. In Vanilla, casts Mithril Shards (400 magic) at 5% chance per 2 seconds
            // I'm making it similar in stats to Blackflame -- noting this item is 4 lvls higher -- but with extremely low raw DPS,
            // and making it interesting by buffing Mithril Shards a lot and giving this a higher proc chance, so it's an all-or-nothing gamble weapon
            // NB with mithril shards @ 1400 dmg and 18% chance to proc per second, this does about 252 dps *on average*
            new EquipmentGenerator { Id = ItemId.DREAMY_WAND, Damage = 1, AttackDelay = 1, HP = 0, Mana = 120, AC = 0, Int = 16, Wis = 5, Cha = 12, Res = 3 }.TuneWand(1, 1, 25, null, 18).Generate(__instance);

            // Here's an interesting idea. Cryst. Tactics + Celestial Spike are similar lvl and as a combo they're *aesthetic* and feel very magical. Sounds like it's time to design a set bonus system!
            // Note Cryst. Tactics is *not a weapon* so the set involves zero weapon dps and needs good stats to balance that out.
            new EquipmentGenerator
            {
                Id = ItemId.CRYSTALLISED_TACTICS,
                HP = 70,
                Mana = 70,
                AC = 10,
                Int = 13,
                Wis = 13,
                Cha = 7,
                Res = 2,
                Lore = "Carried by the warriors of Vitheo, its energies are said to give an edge in battle... But its glow is dull without the Celestial Spike by its side."
            }.Generate(__instance);

            // wands now compete with Brax's Fury (1300 dps)

            // Farming Stardust for Celestial Spike just doesn't make sense with the vanilla RARE drop chance; you have to use the flower thingies to make them vulnerable, 
            // and you want to be saving them to farm Astra later because they're a pain, and (as a new player) you probably won't even know how to kill them 'til later.
            // So, I'm making it a rare drop from Celestial Matter instead, and if you're game enough to spend said flower thingies (forgotten hope? lost faith? strangled echidna? I don't remember) 
            // then you now at least have UNCOMMON chance from Stardust themselves. Probably in addition to the existing RARE chance so a very remote chance to get 2. Yay you!
            DroppedBy(NpcName.CELESTIAL_MATTER, DropChance.RARE, 
                DroppedBy(NpcName.STARDUST, DropChance.UNCOMMON, new EquipmentGenerator { Id = ItemId.CELESTIAL_SPIKE, HP = 150, Mana = 150, AC = 22, End = 10, Int = 14, Wis = 5, Cha = 10, Res = 1,
                Lore = "This unique item's true power is only unlocked when equipped alongside other crystals or enchanted stones, when its celestial facets send powerful fractal ripples, intensifying their glow two-fold."
            }.Generate(__instance)));

            // Mem. of Snow = ice version of Cinder of Birth! But higher level, and with more stat sacrifices,  but a much more powerful spell attached.
            // Again, keep in mind this is a close range weapon... needs to have good damage to be worth getting up close to use.
            // base dmg = 840/3 = 280 + Ice Spear 1920/3 * .42 = 268 DPS avg,c total = 280+268=549~
            // Because the damage is so high, I'm adding negatives to defense as well as a significant mana penalty, to make it like "you can use this for good damage, but it means you're focusing more on weapon than spells"
            new EquipmentGenerator
            {
                Id = ItemId.MEMORIES_OF_SNOW,
                HP = -100,
                Mana = -300,
                AC = -15,
                End = -10,
                Int = 0,
                Wis = 20,
                Cha = 0,
                Res = 2,
                WandBoltSpeed = 5,
                WandBoltColour = new Color32(134, 244, 255, 255),
            }.TuneWand(840, 3, 5, null, 42).Generate(__instance);

            // Upgrading Volc Sceptre's spell to Brax's Fury so it's much stronger. Balanced against Mem of Snow, it's got a lot lower chance to proc, 
            // but has better stats in general, and is also a RANGED weapon, making its chance to proc at all much more valuable
            // DPS goal: 320 (so mem of snow is 50% stronger, but requires close range and slow/frustrating)
            // so Brax Fury 1300 dmg 20%/s = 260 dps, make weapon 60 dps
            new EquipmentGenerator { Id = ItemId.VOLCANIC_SCEPTRE, HP = 180, Mana = 30, AC = 15, Int = 5, Wis = 6, Cha = 20, Res = 2, }.TuneWand(60, 1, 25, SpellDB_Start.BRAXS_FURY_SPELL_ID, 20).Generate(__instance);

            // changing res crystal to be a wand type instead of melee, got enough of those, and doesn't... *look* like a melee weapon.
            // Stats once paired in set are "all 'round good" but not stand-out amazing -- what makes it special is its atk speed halves, dmg doubles, so it ends up *powerful* as a wand in set
            // make it feel like the plasma gun from Doom! 460 DPS in set seems like a good point to get to
            new EquipmentGenerator
            {
                Id = ItemId.RESONATING_CRYSTAL,
                HP = 120,
                Mana = 120,
                AC = 0,
                Int = 13,
                Wis = 13,
                Cha = 7,
                Res = 3,
                WandBoltSpeed = 3,
                WandBoltColour = new Color32(252, 136, 255, 255),
                WandAttackSound = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.HARDENED_SKIN_SPELL_ID).ChargeSound,
                Lore = "Sounds reverberate oddly through this crystal. Its color is dull, however, as though it's missing something..."
            }.TuneWand(115, 1, 12).Generate(__instance);

            // despite being a lowish item level, Siva Sceptre actually drops from 27-28 enemies, making it actually a very similar drop to Siva Wand. 
            // So to differentiate them, make Siva Sceptre a slow hard hitting non-proc weapon w/ good base stats (250 dps)
            // And Siva Wand somewhere between that and Volc Sceptre -- better stats and raw dps, less proc chance/dmg (290 dps)
            // With a spell proc and similar level  this would be too similar to Volc Sceptre, so make it stronger raw dps, slower, better stats, but fewer procs
            // DPS goal: 290 (focus pain = 1500 dmg @ 10% = 150 so 140 base dps)
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_SCEPTRE, Level = 26, HP = 100, Mana = 100, AC = 8, End = 8, Int = 28, Wis = 12, Cha = 7, Res = 3 }.TuneWand(500, 2, 25).Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_WAND, HP = 20, Mana = 80, AC = 0, Int = 23, Wis = 2, Cha = 14, Res = 2 }.TuneWand(140, 1, 18, null, 10).Generate(__instance);
            // Runed Shield = lots of extra defense, completely sacrificing int
            new EquipmentGenerator { Id = ItemId.RUNED_SHIELD, HP = 300, Mana = 60, AC = 75, End = 15, Int = 0, Wis = 12, Cha = 16, Res = 2 }.Generate(__instance);

            // Vanilla Brax's Candle just seems like... a worse Volcanic Sceptre that you unlock later. I'm removing its chance to prox Brax's Rage on hit, and instead giving it an activatable cast of something.
            // Also going to buff it and make it part of a set with the Brax Testament... after all, these two surely belong together, and Brax-themed gear is also extremely wizardly!

            //  Spectral Sceptre is an odd one -- low level and unremarkable at that, but dropped from a high end enemy. To play into its spectral nature I'm making it to high damage but with nil stats except res
            new EquipmentGenerator { Id = ItemId.SPECTRAL_SCEPTRE, Level = 29, HP = 0, Mana = 0, AC = 0, End = 0, Dex = 0, Int = 0, Wis = 0, Cha = 0, Res = 4 }.TuneWand(400, 1, 25).Generate(__instance);

            var braxsCandle = new EquipmentGenerator
            {
                Id = ItemId.BRAXS_CANDLE,
                HP = 25,
                Mana = 25,
                End = 6,
                Int = 0,
                Wis = 16,
                Cha = 0,
                Res = 0,
                ClickEffect = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.DESERT_COFFIN_SPELL_ID),
                Lore = "A large, ancient wand once wielded by a Braxonian High Priest. Only the incantation in the Braxonian Testament may awaken its power.",
            }.TuneWand(150, 1, 20).Generate(__instance);
            braxsCandle.WandEffect = null;
            braxsCandle.WandProcChance = 0;

            new EquipmentGenerator
            {
                Id = ItemId.BRAXONIAN_TESTAMENT,
                HP = 25,
                Mana = 25,
                AC = 6,
                Int = 0,
                Wis = 16,
                Cha = 0,
                Res = 0,
                Lore = "A dusty old tome. The cryptic runes can only be decoded using the cipher on Brax's Candle..."
            }.Generate(__instance);
            // The glowing blue stone, once paired with the Celestial Spike, is marginally more defensive than the SivaBrax set, and with 1 point higher res, but lower int/wis/cha, and obv lacking the Desert Tempest spell.
            // However, SivaBrax teachings are only obtained via a quest, therefore can't be dropped as quality items etc. -- making the glowing blue stone a solid endgame option if you manage a good drop!
            // How's that for a reason to go back to Fernella's and kill those spectres that whooped our arses earlier? ;)
            // DPS Goal: similar to resonating crystal, slightly stronger dps, but mainly an upgrade because it's got good stats
            new EquipmentGenerator
            {
                Id = ItemId.GLOWING_BLUE_STONE,
                HP = 300,
                Mana = 210,
                AC = 80,
                Int = 17,
                Wis = 10,
                Cha = 13,
                Res = 5,
                WandBoltSpeed = 5,
                WandBoltColour = new Color32(0, 108, 255, 255),
                Level = 36,
                Lore = "You've never seen the color blue look so vivid... But is there something that could make it brighter still?",
            }.TuneWand(130, 1, 15).Generate(__instance);
            new EquipmentGenerator
            {
                Id = ItemId.SIVA_BRAXONIAN_TEACHINGS,
                HP = 155,
                Mana = 155,
                AC = 6,
                Int = 0,
                Wis = 25,
                Cha = 5,
                Res = 0,
                Lore = "Wait... This contradicts the Braxonian Testament! It changes EVERYTHING about the Candle -- and reveals its TRUE power!"
            }.Generate(__instance);

            // Pleeeeenty of wands going around... Let's make this another 2 hander! And an endgame viable one at that.
            // Balancing with the following in mind:
            // - It's an easy drop to get from Statue of Brax, very farmable, 50% drop rate
            // - That means it's easier to farm for superior/masterwork
            // - As materwork it's as good as a 1h+offhand in stats, then that's half the time required compared with getting rare on *both* those items.
            // It's also only 1 item to blue+purple!
            // So, this will be balanced to be an "early endgame item" that will be outclassed by the other items if you manage to get good drops on both the 1h+offhand.
            new EquipmentGenerator
            {
                Id = ItemId.PETRIFIED_WOOD_CANE,
                HP = 280,
                Mana = 600,
                AC = 22,
                End = 4,
                Int = 30,
                Wis = 30,
                Cha = 30,
                Res = 10,
                WeaponType = Item.WeaponType.TwoHandStaff,
                SlotType = Item.SlotType.Primary,
                WandBoltSpeed = 15,
                WandBoltColour = new Color32(133, 86, 67, 255)
            }.TuneWand(900, 3, 12).Generate(__instance);


            // Garg wand's role: previously it was BiS just based on it having 5 res, without much else going for it. Now I'm making it a mixed bag with a couple of great stats and the rest avg/0,
            // still 5 res which isn't as big as it used to be now that lots of item combos can give you 8-11ish,
            // and swapping out Magic Missile on hit for... INFERNIS! So a big advantage of using Garg is you can keep Infernis up on enemies without wasting mana/time to cast.
            // but if you don't mind doing that yourself anyway, maybe this isn't the weapon for you any more...
            // 380 DPS -- solid for endgame, but not extraordinary. 
            new EquipmentGenerator { Id = ItemId.GARG_WAND, HP = 50, Mana = 200, Int = 35, Wis = 18, Cha = 0, Res = 5 }.TuneWand(760, 2, 25, SpellDB_Start.INFERNIS_SPELL_ID, 30).Generate(__instance);

            // Endgame viable defensive offhander with int, wis and res
            new EquipmentGenerator { Id = ItemId.ULORS_ENCYCLOPEDIA, HP = 500, Mana = 200, AC = 40, End = 20, Int = 30, Wis = 22, Cha = 0, Res = 5 }.Generate(__instance);

            // A semi defensive off-hander with a wis focus (less useful) and less stats overall, but with 1 higher res. So, a difficult compromise.
            new EquipmentGenerator { Id = ItemId.ASCENDED_REMAINS, HP = 250, Mana = 400, AC = 20, End = 10, Int = 4, Wis = 30, Cha = 15, Res = 6 }.Generate(__instance);

            // At last we come to you! When did I start writing comments for every item? *I DON'T KNOW!* But it's a thing now! I'm probably losing my mind!
            // I've tweaked way, way too many items! I don't know what I'm doing any more! LET'S GOOO!
            // This guy is already a motherfucking powerhouse in vanilla, and I still want to keep it like that -- a fantastic reward for beating Astra,
            // The effort involved in summoning+beating Astra -- and then only 20% drop chance -- means this is probably one of the harder items to get a rare etc. drop of.
            // So, having great stats out of the gate doesn't necessarily make it OP -- its base is competing with other items that have dropped twice as strong.
            // However, rebalancing it a bit. Same HP/Mana, dropping AC from 100, and buffing other stats
            // DPS Goal: 480~ average. Aetherstorm = 2880 dmg, 10% chance/sec = 288, so add 190~ raw dps
            // On paper this might not look as strong as garg, which I think makes it interesting now. Great stats, hard to pass up, less consistent damage, but ACTUALLY higher DPS on average overall.
            // That's before taking into account that Aetherstorm proc can *resonate*, increasing that damage by a decent margin in endgame (maybe 620ish avg DPS total) 
            // [21/11/2025: Brought raw dps from 190 to 250, bringing total avg dps to 538 on average. That's because battles don't tend to go as long at this point in the game,
            // so a "powerful infrequent proc" which happens to balance damage on avg is less useful. I would even have increased it further, but for the same exact reason, the fact
             // it attacks once per second (2x garg speed) makes up for any further damage loss, as it's handier for clearing weaker enemies]
            new EquipmentGenerator { Id = ItemId.SINGULARITY_VESSEL_OF_CREATION, HP = 500, Mana = 355, AC = 40, Int = 40, Wis = 14, Cha = 34, Res = 6 }.TuneWand(250, 1, 25, null, 10).Generate(__instance);
        }

        public static void RegisterSets(ItemDatabase itemDb)
        {
            setBonusesByItemId.Clear();

            // because it's actually a weapon for attacking, we don't actually want to DOUBLE its delay
            // instead, we'll make the main boon of it the way everythign doubles -- twice as much damage, and twice as fast, so a 4x damage boost -- making it similar in strength to the melee weapons, but at range!
            // therefore, make the set bonus *subtract* half the existing weapon delay
            var resCrystalBonus = GameObject.Instantiate(itemDb.GetItemByID(ItemId.RESONATING_CRYSTAL));
            resCrystalBonus.WeaponDly = -resCrystalBonus.WeaponDly * .5f;
            var blueStoneBonus = GameObject.Instantiate(itemDb.GetItemByID(ItemId.GLOWING_BLUE_STONE));
            blueStoneBonus.WeaponDly = -blueStoneBonus.WeaponDly * .5f;
            setBonusesByItemId.Add(ItemId.CELESTIAL_SPIKE, new Dictionary<ItemId, Item>() {
                { ItemId.CRYSTALLISED_TACTICS, itemDb.GetItemByID(ItemId.CRYSTALLISED_TACTICS) },
                { ItemId.RESONATING_CRYSTAL, resCrystalBonus },
                { ItemId.GLOWING_BLUE_STONE, blueStoneBonus },
            });
            itemDb.GetItemByID(ItemId.GLOWING_BLUE_STONE).WandBoltColor = new Color32(0, 108, 255, 255);

            var braxCandle = itemDb.GetItemByID(ItemId.BRAXS_CANDLE);
            var braxBraxBonus = ScriptableObject.CreateInstance<Item>();
            braxBraxBonus.SpellCastTime = (-(braxCandle.SpellCastTime * 0.25f)) * 60f;
            braxBraxBonus.HP = 350;
            braxBraxBonus.Mana = 250;
            braxBraxBonus.AC = 15;
            braxBraxBonus.End = 12;
            braxBraxBonus.Int = 44;
            braxBraxBonus.Wis = 18;
            braxBraxBonus.Cha = 22;
            braxBraxBonus.Res = 7;
            braxBraxBonus.WeaponDmg = 360 - braxCandle.WeaponDmg;  // just subtracting so i can easily set the SET base dmg without worrying about changes to og weapon base dmg. in 1st set, nearly as strong as spectral sceptre, but with stats!

            var braxSivaBonus = ScriptableObject.CreateInstance<Item>();
            braxSivaBonus.SpellCastTime = (-(braxCandle.SpellCastTime * 0.5f)) * 60f;
            braxSivaBonus.HP = 700;
            braxSivaBonus.Mana = 400;
            braxSivaBonus.AC = 50;
            braxSivaBonus.End = 20;
            braxSivaBonus.Int = 55;
            braxSivaBonus.Wis = 26;
            braxSivaBonus.Cha = 35;
            braxSivaBonus.Res = 9;
            braxSivaBonus.WeaponDmg = 460 - braxCandle.WeaponDmg;

            // Sets are very powerful, but these stats DON'T benefit from blessing or rarity!
            setBonusesByItemId.Add(ItemId.BRAXS_CANDLE, new Dictionary<ItemId, Item>() {
                { ItemId.BRAXONIAN_TESTAMENT, braxBraxBonus },
                { ItemId.SIVA_BRAXONIAN_TEACHINGS, braxSivaBonus }
            });
        }

        static void UpdateAuras(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            // Add a new aura item that has some other useful Arcanist-first alternative benefit (with secondary benefits for other classes)
            // eg, spells are 50% less likely to resonate, but XXX
            // spell cooldowns are lower 
            // TODO: Eh, another time.
        }

        static void UpdateTreasureMaps(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            var firstPart = __instance.GetItemByID(ItemId.MAP_PIECE_1);
            var secondPart = __instance.GetItemByID(ItemId.MAP_PIECE_2);
            var thirdPart = __instance.GetItemByID(ItemId.MAP_PIECE_3);
            var fourthPart = __instance.GetItemByID(ItemId.MAP_PIECE_4);

            firstPart.ItemName = "Stash Map (Piece)";
            firstPart.Lore = "A ripped piece of an amateurish map that looks like it was made by a local who squirreled goods away.";
            secondPart.ItemName = "Pirate's Treasure Map (Piece)";
            secondPart.Lore = "This torn segment bears the distinct style of a seafaring bandit, and once whole, will surely point to booty!";
            thirdPart.ItemName = "Map to Cursed Dig Site (Piece)";
            thirdPart.Lore = "Details leading to a remote area of Erenshor, supposedly containing some artefact of value... But it's incomplete.";
            fourthPart.ItemName = "Vithean Cache Location (Piece)";
            fourthPart.Lore = "Markings depicting an undiscovered cache of ancient Vithean treasures! Only ripped. As always.";

            string spiel = "\n\nRight Click to begin treasure hunt. Dying will end the hunt.\n\nOnly one hunt can be active at a time. Beware: the loot may be guarded!";
            var firstWhole = __instance.GetItemByID(ItemId.FULL_MAP_1);
            firstWhole.ItemName = "Stash Map";
            firstWhole.Lore = "An amateurish map that looks like it was made by a local who squirreled goods away." + spiel;

            var secondWhole = GameObject.Instantiate(firstWhole);
            secondWhole.Id = ItemId.FULL_MAP_2.Id();
            secondWhole.ItemName = "Pirate's Treasure Map";
            secondWhole.Lore = "It bears the distinct style of a seafaring bandit, and surely points to booty!" + spiel;
            // Suuuuuper dodgy, but doesn't really seem to matter if these spells exist in the database. And the SpellVessel code only looks up what they do by name anyway... so... yyyyyeah
            secondWhole.ItemEffectOnClick = GameObject.Instantiate(firstWhole.ItemEffectOnClick);
            secondWhole.ItemEffectOnClick.SpellName = $"Read {secondWhole.ItemName}";
            itemsToAdd.Add(secondWhole);


            var thirdWhole = GameObject.Instantiate(firstWhole);
            thirdWhole.Id = ItemId.FULL_MAP_3.Id();
            thirdWhole.ItemName = "Map to Cursed Dig Site";
            thirdWhole.Lore = "Details lead to an abandoned dig site in a remote area, supposedly containing some artefact of value." + spiel;
            thirdWhole.ItemEffectOnClick = GameObject.Instantiate(firstWhole.ItemEffectOnClick);
            thirdWhole.ItemEffectOnClick.SpellName = $"Read {thirdWhole.ItemName}";
            itemsToAdd.Add(thirdWhole);

            var fourthWhole = GameObject.Instantiate(firstWhole);
            fourthWhole.Id = ItemId.FULL_MAP_4.Id();
            fourthWhole.ItemName = "Vithean Cache Location";
            fourthWhole.Lore = "Markings depicting an undiscovered cache of ancient Vithean treasures!" + spiel;
            fourthWhole.ItemEffectOnClick = GameObject.Instantiate(firstWhole.ItemEffectOnClick);
            fourthWhole.ItemEffectOnClick.SpellName = $"Read {fourthWhole.ItemName}";
            itemsToAdd.Add(fourthWhole);

            // Not sure why this needs to be done, as I'm not actually changing the map pieces' references or IDs, but perhaps they're assigned based on a name lookup somewhere..?
            // In any case, after this patch the list is empty so I'm manually popping them in
            Main.Log.LogInfo("Existing GM.Maps  before I replace the list: ");
            foreach(var m in GameData.GM.Maps)
            {
                Main.Log.LogInfo(m.Id + " - " + m.ItemName);
            }
            GameData.GM.Maps = new List<Item>() { firstPart, secondPart, thirdPart, fourthPart };
        }



        static Item CreateSkillBook(Item baseSkillBook, ItemId id, string skillId, int value)
        {
            var book = GameObject.Instantiate(baseSkillBook);
            book.Id = id.Id();
            book.TeachSkill = GameData.SkillDatabase.GetSkillByID(skillId);
            book.name = book.ItemName = $"Skill Book: {book.TeachSkill.SkillName}";
            book.ItemLevel = book.TeachSkill.ArcanistRequiredLevel;
            book.ItemValue = value;
            return book;
        }

        private static Item SoldBy(NpcName npcName, Item item)
        {
            if (!itemsSoldByVendor.TryGetValue(npcName, out HashSet<Item> set))
            {
                set = new HashSet<Item>();
                itemsSoldByVendor.Add(npcName, set);
            }

            set.Add(item);

            return item;
        }

        private static Item DroppedBy(NpcName npcName, DropChance dropChance, Item item)
        {
            if (!dropsByNpc.TryGetValue(npcName, out HashSet<(DropChance, Item)> set))
            {
                set = new HashSet<(DropChance, Item)>();
                dropsByNpc.Add(npcName, set);
            }

            set.Add((dropChance, item));

            return item;
        }

        public static void RefreshSprites(ItemDatabase itemDb = null)
        {
            if (itemDb == null) itemDb = GameData.ItemDB;
            foreach (var entry in Main.itemSpriteById)
            {
                itemDb.GetItemByID(entry.Key).ItemIcon = entry.Value;
            }
            Main.Log.LogInfo($"Updated graphics for {Main.itemSpriteById.Count} items");
        }

        static void AddLuckScrolls(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            var bread = __instance.GetItemByID(ItemId.BREAD);

            foreach (var scrollData in new List<(ItemId scrollId, string spellId, int value)>() {
                    (ItemId.LUCK_SCROLL_1, SpellDB_Start.LUCK_OF_SOLUNA_1_SPELL_ID, 5500),
                    (ItemId.LUCK_SCROLL_2, SpellDB_Start.LUCK_OF_SOLUNA_2_SPELL_ID, 20000),
                    (ItemId.LUCK_SCROLL_3, SpellDB_Start.LUCK_OF_SOLUNA_3_SPELL_ID, 45000),
                    (ItemId.LUCK_SCROLL_4, SpellDB_Start.LUCK_OF_SOLUNA_4_SPELL_ID, 95000),
                    (ItemId.LUCK_SCROLL_5, SpellDB_Start.LUCK_OF_SOLUNA_5_SPELL_ID, 180000),
                })
            {
                var scroll = GameObject.Instantiate(bread);
                var spell = GameData.SpellDatabase.GetSpellByID(scrollData.spellId);
                scroll.Id = scrollData.scrollId.Id();
                scroll.name = scroll.ItemName = $"Scroll of {spell.SpellName}";
                scroll.Lore = spell.SpellDesc;
                scroll.ItemEffectOnClick = spell;
                scroll.ItemValue = scrollData.value;

                SoldBy(NpcName.ASAGA_UNDERLOFT, scroll);
                itemsToAdd.Add(scroll);
            }
        }
    }
}