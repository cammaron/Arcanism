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

        // Waist
        OGRESKIN_CORD = 1727100,
        BRAXONIAN_SASH = 75333375,
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
        GARDENERS_WRIST_WRAP = 173442,
        BRACELET_OF_VESSEL = 46442784,
        RESONATING_BRACE = 12451740,

        // Arm/shoulder
        DESERT_SILK_SLEEVES = 5557329,
        INTRICATE_SLEEVES = 45089611,
        GIFTED_SLEEVES = 9671598,
        BRAXONIAN_ROYAL_ARMBAND = 20398309,
        DREAMY_SLEEVES = 30698425,
        ARMBANDS_OF_ORDER = 66574848,
        BONEBANDED_ARMGUARD = 62353340,

        // Neck
        CREST_OF_BRAXONIA = 12271853,

        // Weapons/Offhands
        ARGOS_GRIMOIRE = 39840084,
        MAGUS_SHIELD = 43055028,
        OGRESPICE_BUNDLE = 79500585,
        SPECTRAL_SCEPTRE = 3794175,
        PEARLESCENT_KELP_TOTEM = 4853466,
        DIAMONDINE_SHIELD = 29324202,
        ROYAL_CARAPACE = 84620118,
        BLACKFLAME_TORCH = 20176773,
        SCORCHED_WALKING_STICK = 7912436,
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

        TWIN_SPELL_3 = 90000008,
        VANISHING_TWIN = 90000009,
        PARASITIC_TWIN = 90000010,
        SIBLING_SYNERGY = 90000011,
        PERFECT_RELEASE = 90000012,
        PERFECT_RELEASE_2 = 90000013,

        MANA_CHAIN = 90000014,
        VIZIERS_LAMENT = 90000015

    }

    public enum NpcName {
        BRAXON_MANFRED,
        EDWIN_ANSEGG,
        MOLORAI_MILITIA_ARCANIST,
        RISEN_DRUID,
        PRIEL_DECEIVER,
        VESSEL_SIRAETHE,
        SEED_OF_BLIGHT,
        ELWIO_THE_TRAITOR,
        FENTON_THE_BLIGHTED,
        BLIGHT_WYRM,
        DIAMOND_HOUND,
        ARCANE_PUPIL
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

        static void Postfix(ItemDatabase __instance)
        {
            Main.Log.LogInfo("Adding and updating items for Arcanism.");
            var itemDictionary = Traverse.Create(__instance).Field<Dictionary<string, Item>>("itemDict").Value;
            var itemsToAdd = new List<Item>();

            var funeralPyreScroll = __instance.GetItemByID(ItemId.FUNERAL_PYRE);
            string origName = funeralPyreScroll.ItemName;
            funeralPyreScroll.ItemIcon = __instance.GetItemByID("335358").ItemIcon; // Make it purple!
            funeralPyreScroll.ItemName = $"Spell Scroll: {SpellDBStartPatch.FUNERAL_PYRE_NAME_CHANGE}";
            Main.Log.LogInfo($"Item updated: {origName}");

            var controlChant = __instance.GetItemByID(ItemId.CONTROL_CHANT);
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.EXPERT_CONTROL, "Expert Control I", 9, SkillDBStartPatch.EXPERT_CONTROL_SKILL_ID, 35000))); // Sold by Braxonian Manfred
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.PERFECT_RELEASE, "Perfect Release I", 14, SkillDBStartPatch.PERFECT_RELEASE_SKILL_ID, 75000))); // Sold by Braxonian Manfred
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.EXPERT_CONTROL_2, "Expert Control II", 17, SkillDBStartPatch.EXPERT_CONTROL_2_SKILL_ID, 125000))); // Sold by Braxonian Manfred
            itemsToAdd.Add(DroppedBy(NpcName.ELWIO_THE_TRAITOR, DropChance.RARE, CreateSkillBook(controlChant, ItemId.PERFECT_RELEASE_2, "Perfect Release II", 32, SkillDBStartPatch.PERFECT_RELEASE_2_SKILL_ID, 115000))); // Dropped by Elwio the Traitor in Vitheo's Rest


            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.TWIN_SPELL, "Twin Spell", 10, SkillDBStartPatch.TWIN_SPELL_SKILL_ID, 40000)));
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.VANISHING_TWIN, "Vanishing Twin", 15, SkillDBStartPatch.VANISHING_TWIN_SKILL_ID, 85000)));
            itemsToAdd.Add(DroppedBy(NpcName.SEED_OF_BLIGHT, DropChance.UNCOMMON, CreateSkillBook(controlChant, ItemId.SIBLING_SYNERGY, "Sibling Synergy", 18, SkillDBStartPatch.SIBLING_SYNGERY_SKILL_ID, 85000)));
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.TWIN_SPELL_2, "Twin Spell Mastery I", 20, SkillDBStartPatch.TWIN_SPELL_MASTERY_SKILL_ID, 250000)));
            itemsToAdd.Add(SoldBy(NpcName.BRAXON_MANFRED, CreateSkillBook(controlChant, ItemId.PARASITIC_TWIN, "Parasitic Twin", 23, SkillDBStartPatch.PARASITIC_TWIN_SKILL_ID, 340000)));
            itemsToAdd.Add(DroppedBy(NpcName.FENTON_THE_BLIGHTED, DropChance.RARE, CreateSkillBook(controlChant, ItemId.TWIN_SPELL_3, "Twin Spell Mastery II", 27, SkillDBStartPatch.TWIN_SPELL_MASTERY_2_SKILL_ID, 95000)));



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
            public Item.WeaponType? WeaponType;
            public bool? IsWand;
            public int? WandRange;
            public Color? WandBoltColour;
            public float? WandBoltSpeed;
            public AudioClip WandAttackSound;

            public int? Damage;
            public float? AttackDelay;

            public string AppearanceType;

            public (string, string)? ShoulderTrim;
            public (string, string)? ElbowTrim; 
            public (string, string)? KneeTrim;
            
            public (Color, Color)? ColorsMain;
            public (Color, Color)? ColorsLeather;
            public (Color, Color)? ColorsMetal;

            public List<Class> Classes;

            public Spell WornEffect;
            public Spell ClickEffect;
            public Spell WandEffect;

            public float? SpellCastTime;

            public Item Generate(ItemDatabase db)
            {
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
                if (WeaponType.HasValue) item.ThisWeaponType = WeaponType.Value;
                
                if (IsWand.HasValue) item.IsWand = IsWand.Value;
                if (WandBoltColour.HasValue) item.WandBoltColor = WandBoltColour.Value;
                if (WandBoltSpeed.HasValue) item.WandBoltSpeed = WandBoltSpeed.Value;
                if (WandRange.HasValue) item.WandRange = WandRange.Value;
                if (WandAttackSound != null) item.WandAttackSound = WandAttackSound;

                if (Damage.HasValue) item.WeaponDmg = Damage.Value;
                if (AttackDelay.HasValue) item.WeaponDly = AttackDelay.Value;

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

                if (WornEffect != null) item.WornEffect = WornEffect;
                if (ClickEffect != null) item.ItemEffectOnClick = ClickEffect;
                if (WandEffect != null) item.WandEffect = WandEffect;

                if (SpellCastTime.HasValue) item.SpellCastTime = SpellCastTime.Value;

                if (CreateFromBaseId.HasValue)
                    Main.Log.LogInfo($"Item created: {Name}");
                else
                    Main.Log.LogInfo($"Item updated: {origName}");

                return item;
            }
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

        static void UpdateLegPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            itemsToAdd.Add(DroppedBy(NpcName.MOLORAI_MILITIA_ARCANIST, DropChance.RARE, new EquipmentGenerator { 
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
            }.Generate(__instance)));

            itemsToAdd.Add(DroppedBy(NpcName.PRIEL_DECEIVER, DropChance.RARE, new EquipmentGenerator
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
                
            }.Generate(__instance)));

            itemsToAdd.Add(DroppedBy(NpcName.VESSEL_SIRAETHE, DropChance.RARE, new EquipmentGenerator
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
            itemsToAdd.Add(SoldBy(NpcName.EDWIN_ANSEGG, new EquipmentGenerator { 
                CreateFromBaseId = ItemId.FUNERAL_GARB,
                Id = ItemId.NOVICE_ROBE, 
                Name = "Novice's Robe", 
                Lore = "The hallmark of a fledgling magician.",
                Level = 6,	HP = 20,	Mana = 25,	AC = 14,	End = 4,	Int = 5,    Wis = 4,	Cha = 3,	Res = 1,	Value = 650, 
                Classes = new List<Class>() { GameData.ClassDB.Arcanist },
                AppearanceType = "ReinforcedLeatherPants",
                ColorsMain = (new Color32(100, 78, 65, 255), new Color32(100, 78, 65, 255)),
                ColorsLeather = (new Color32(50, 40, 36, 255), new Color32(120, 107, 78, 255)),
            }.Generate(__instance)));

            DroppedBy(NpcName.RISEN_DRUID, DropChance.RARE, new EquipmentGenerator { Id = ItemId.SPIDERSILK_SHIRT, HP = 60, Mana = 55, AC = 27, End = 2, Int = 10, Wis = 7, Cha = 7, Res = 1, Value = 950, }.Generate(__instance));
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

        static void UpdateWaistPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.OGRESKIN_CORD, HP = 28, Mana = 35, AC = 14, End = 5, Int = 6, Wis = 4, Cha = 0, Res = 0, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_SASH, HP = 200, Mana = 0, AC = 25, End = 10, Int = 7, Wis = 9, Cha = 3, Res = 1, }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SASH_OF_THE_LOST_GUARD, HP = 200, Mana = 200, AC = 35, End = 10, Int = 5, Wis = 7, Cha = 4,  }.Generate(__instance);
            itemsToAdd.Add(DroppedBy(NpcName.DIAMOND_HOUND, DropChance.RARE, new EquipmentGenerator{ 
                CreateFromBaseId = ItemId.PRISMATIC_WARCORD, 
                Id = ItemId.MANA_CHAIN, 
                Name = "Mana Chain",
                Lore = "A colourful belt infused with sparkling rainbow magic.",
                HP = 200, Mana = 250, AC = 29, End = 0, Int = 12, Wis = 9, Cha = 10, Res = 1, Value = 6000, 
                Classes = new List<Class>() { GameData.ClassDB.Arcanist } 
            }.Generate(__instance)));
            new EquipmentGenerator { Id = ItemId.FLOWING_BLIGHT_SILK_SASH, HP = 10, Mana = 350, AC = 8, Str = 0, Dex = 0, Agi = 0, End = 0, Int = 15, Wis = 25, Cha = 3, Value = 11200, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.CHARMED_BELT, HP = 200, Mana = 255, AC = 25, End = 10, Int = 14, Wis = 11, Cha = 25, Res = 3, Value = 11200 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ROYAL_WAISTBAND, HP = 450, Mana = 180, AC = 50, End = 15, Int = 19, Wis = 2, Cha = 18, Value = 9500, Res = 1 }.Generate(__instance);
            itemsToAdd.Add(DroppedBy(NpcName.BLIGHT_WYRM, DropChance.GUARANTEE_ONE, new EquipmentGenerator { 
                CreateFromBaseId = ItemId.BRAXONIAN_SASH, 
                Id = ItemId.VIZIERS_LAMENT,
                Name = "Vizier's Lament",
                Lore = "After the Blight Wyrm simply refused to listen to diplomacy, the vizier was said to have... well, died. And been eaten. This thing still smells a bit.",
                Level = 36, HP = 380, Mana = 350, AC = 43, End =  10, Int = 21, Wis = 25, Cha = 20, Res = 5, Value = 18400,
                Classes = new List<Class>() { GameData.ClassDB.Arcanist } 
            }.Generate(__instance)));
        }

        static void UpdateBackPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.WARDWARPED_CAPE, HP = 400, Mana = 180, AC = 50, End = 20, Int = 22, Wis = 25, Cha = 16, Res = 3 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.AZYNTHIAN_CAPE, HP = 180, Mana = 400, AC = 20, Dex = 0, Agi = 0, Str = 0, End = 0, Int = 22, Wis = 15, Cha = 26, Res = 4 }.Generate(__instance);
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
            new EquipmentGenerator { Id = ItemId.ERODED_BANGLE }.Generate(__instance).WeaponProcChance = 6;
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_BRACELET, HP = 100, Mana = 100, AC = 22, End = 12, Int = 19, Wis = 20, Cha = 8, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BLESSED_BRAXONIAN_BRACELET, HP = 200, Mana = 200, AC = 30, End = 14, Int = 24, Wis = 24, Cha = 11, Res = 3, Value = 11000 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.PERFECT_LINKS, HP = 80, Mana = 80, AC = 15, Int = 6, Wis = 12, Cha = 20, Res = 5 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.WARDED_BRACE, HP = 120, Mana = 180, AC = 5, Int = 15, Wis = 15, Cha = 5, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.GARDENERS_WRIST_WRAP, HP = 220 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRACELET_OF_VESSEL, HP = 30, Mana = 200, AC = 12, Int = 26, Wis = 0, Cha = 20, Res = 4 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.RESONATING_BRACE, HP = 0, Mana = 0, AC = 40, Int = 5, Wis = 8, Cha = 5, Res = 8 }.Generate(__instance);
        }

        static void UpdateShoulderPieces(ItemDatabase __instance, List<Item> itemsToAdd)
        {
            new EquipmentGenerator { Id = ItemId.DESERT_SILK_SLEEVES, HP = 0, Mana = 80, AC = 0, Str = 0, Dex = 0, End = 0, Int = 10, Wis = 2, Cha = 7, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.INTRICATE_SLEEVES, HP = 130, Mana = 40, AC = 15, End = 9, Int = 8, Wis = 6, Cha = 9, Res = 0 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.GIFTED_SLEEVES, HP = 30, Mana = 120, AC = 3, Str = 0, End = 0, Int = 23, Wis = 19, Cha = 6, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BRAXONIAN_ROYAL_ARMBAND, HP = 250, Mana = 250, AC = 30, End = 12, Int = 20, Wis = 11, Cha = 20, Res = 2 }.Generate(__instance);
            
            new EquipmentGenerator { Id = ItemId.DREAMY_SLEEVES, HP = 0, Mana = 350, AC = 0, End = 0,                Int = 24, Wis = 12, Cha = 26, Res = 4, Classes = new List<Class>() { GameData.ClassDB.Arcanist } }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ARMBANDS_OF_ORDER, HP = 280, Mana = 120, AC = 60, Str=20, End = 25, Int = 10, Wis = 10, Cha = 10, Res = 2 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BONEBANDED_ARMGUARD, HP = 180, Mana = 260, AC = 30, End = 15,       Int = 16, Wis = 25, Cha = 9, Res = 3 }.Generate(__instance);
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
            new EquipmentGenerator { Id = ItemId.ARGOS_GRIMOIRE, HP = 0, Mana = 60, AC = 0, Int = 8, Wis = 7, Cha = 2, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.MAGUS_SHIELD, HP = 40, Mana = 18, AC = 25, Int = 4, Wis = 4, Cha = 6, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.OGRESPICE_BUNDLE, HP = 45, Mana = 0, AC = 0, Int = 7, Wis = 0, Cha = 6, Res = 0 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SPECTRAL_SCEPTRE, HP = 0, Mana = 45, AC = 0, End = 2, Dex = 0, Int = 10, Wis = 5, Cha = 7, Res = 1 }.Generate(__instance);

            // Here's an interesting one -- by default this does no damage and is a 1 hander. Let's change it to be a 2 hander with no damage, completely disabling regular attacks, but with good stats to buff casting.
            // NB It competes with Diamondine/Royal Carapace + weapons from Spectral Sceptre to Scorched W Stick
            new EquipmentGenerator { Id = ItemId.PEARLESCENT_KELP_TOTEM, Damage = 0, HP = 20, Mana = 110, AC = 5, Int = 30, Wis = 20, Cha = 0, Res = 7, SlotType = Item.SlotType.Primary, WeaponType = Item.WeaponType.TwoHandStaff }.Generate(__instance);

            new EquipmentGenerator { Id = ItemId.DIAMONDINE_SHIELD, HP = 250, Mana = 200, AC = 50, End = 13, Int = 2, Wis = 10, Cha = 14, Res = 1 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.ROYAL_CARAPACE, HP = 120, Mana = 120, AC = 30, End = 0, Int = 10, Wis = 10, Cha = 5, Res = 2 }.Generate(__instance);

            // Cinder of Birth: despite higher item level this actually drops 25% from Beady, lvl 16 in swamp who isn't hard, so could be gotten around lvl 14...
            // it's a close range weapon with chance to cast Brax's Rage which is a relatively poweful medium spell now, so it's a fantastic item for this level
            // Let's make it interesting by giving it a high damage, low frequency attack -- encouraging a meta of darting in to hit and retreating
            // Icnreasing Brax's Rage proc chance a lot to account for the lower attack speed. This will make it feel really heavy whenever it procs!
            new EquipmentGenerator { Id = ItemId.CINDER_OF_BIRTH, Damage = 12 * 3, AttackDelay = 3, HP = 30, Mana = 30, Int = 4, Wis = 12, Cha = 4, Res = 1 }.Generate(__instance).WeaponProcChance = 35;


            // the next 3 are all about teh same level drops
            // Blackflame is rare and has nothing special, so buff it to be the best cast augmentor of the three
            // Make Scorched Walking Stick into an interesting 2 hander with early access to Brax's Fury but with a longer cast time, which does similar damage to current level's heavy spell (Winter's Bite) but now faster
            // Boneweaver's gets nerfs relative to peers but higher raw wand DPS, and the benefit of its amazing DoT
            // So now, there's actually interesting choices at this level!
            new EquipmentGenerator { Id = ItemId.BLACKFLAME_TORCH,       Damage = 6, AttackDelay = 1,   HP = 0, Mana = 90, Int = 17, Wis = 5, Cha = 13, Res = 3 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SCORCHED_WALKING_STICK, ClickEffect = GameData.SpellDatabase.GetSpellByID(SpellDBStartPatch.BRAXS_FURY_SPELL_ID),    SpellCastTime = 3.2f,
                    HP = 60, Mana = 100, AC = 14, End = 7, Int = 20, Wis = 30, Cha = 6, Res = 4, SlotType = Item.SlotType.Primary, WeaponType = Item.WeaponType.TwoHandStaff }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.BONEWEAVERS_LEG,        Damage = 18, AttackDelay = 1,   HP = 25,  Mana = 60, AC = 0, End = 0, Int = 12, Wis = 8, Cha = 3, Res = 2 }.Generate(__instance);

            // Shivering Step drop, requires going out of your way, needs to be worth it. Casts Mithril Shards (400 magic) at 5% chance in vanilla, but 2s attack delay makes it uninteresting.
            // Let's make it similar in stats to Blackflame -- noting this item is 4 lvls higher -- and very low raw DPS, but higher chance to proc and faster attack
            new EquipmentGenerator { Id = ItemId.DREAMY_WAND, Damage = 1, AttackDelay = 1, HP = 0, Mana = 120, AC = 0, Int = 16, Wis = 5, Cha = 12, Res = 3 }.Generate(__instance).WeaponProcChance = 8;

            // Here's an interesting idea. Cryst. Tactics + Celestial Spike are similar lvl and as a combo they're *aesthetic* and feel very magical. Sounds like it's time to design a set bonus system!
            var crystallisedTactics = new EquipmentGenerator { Id = ItemId.CRYSTALLISED_TACTICS, HP = 65, Mana = 120, AC = 15, Int = 15, Wis = 17, Cha = 4, Res = 2,
                Lore = "Carried by the warriors of Vitheo, its energies are said to give an edge in battle... But its glow is dull without the Celestial Spike by its side."
            }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_SCEPTRE, Damage = 37, AttackDelay = 1.5f, HP = 50, Mana = 50, AC = 0, Int = 23, Wis = 0, Cha = 8, Res = 3 }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.CELESTIAL_SPIKE, HP = 150, Mana = 150, AC = 22, End = 10, Int = 16, Wis = 5, Cha = 10, Res = 1,
                Lore = "This unique item's true power is only unlocked when equipped alongside other crystals or enchanted stones, when its celestial facets send powerful fractal ripples, intensifying their glow two-fold."
            }.Generate(__instance);

            // Mem. of Snow = ice version of Cinder of Birth! But higher level, and with more stat sacrifices,  but a much more powerful spell attached.
            // Again, keep in mind this is a close range weapon... needs to have good damage to be worth getting up close to use.
            new EquipmentGenerator { Id = ItemId.MEMORIES_OF_SNOW, Damage = 12 * 4, AttackDelay = 4f, HP = 0, Mana = 0, AC = 0, Int = 0, Wis = 20, Cha = 0, Res = 2 }.Generate(__instance).WeaponProcChance = 42;
            
            // for volc sceptre, don't change regular damage, but upgrade the spell to Brax's Fury so it's much stronger. Balanced against Mem of Snow, it's got a lot lower chance to proc, 
            // but has better stats in general, and is also a RANGED weapon, making its chance to proc at all much more valuable
            new EquipmentGenerator { Id = ItemId.VOLCANIC_SCEPTRE, HP = 180, Mana = 30, AC = 15, Int = 0, Wis = 6, Cha = 20, Res = 2, 
                WandEffect = GameData.SpellDatabase.GetSpellByID(SpellDBStartPatch.BRAXS_RAGE_SPELL_ID) 
            }.Generate(__instance).WandProcChance = 5;

            // changing res crystal to be a wand type instead of melee, got enough of those, and doesn't... *look* like a melee weapon.
            // Stats once paired in set are "all 'round good" but not stand-out amazing -- what makes it special is its atk speed halves, dmg doubles, so it ends up shooting pretty powerful projectiles
            var resonatingCrystal = new EquipmentGenerator { Id = ItemId.RESONATING_CRYSTAL, Damage = 14, AttackDelay = 2f, HP = 150, Mana = 150, AC = 12, Int = 13, Wis = 13, Cha = 13, Res = 3, 
                IsWand = true, WandRange = 15, WandBoltSpeed = 18, WandBoltColour = (Color) new Color32(252, 136, 255, 255),
                WandAttackSound = GameData.SpellDatabase.GetSpellByID(SpellDBStartPatch.HARDENED_SKIN_SPELL_ID).ChargeSound,
                Lore = "Sounds reverberate oddly through this crystal, coming out the other side louder and more vibrant. Its color is dull, however, as though it's missing something..."
            }.Generate(__instance);

            // despite being a lowish item level, Siva Wand actually drops from 27-28 enemies, so tune accordingly. I'm increasing its item level to match to prevent showing up in AH early.
            new EquipmentGenerator { Id = ItemId.SIVAKAYAN_WAND, Level = 26, HP = 100, Mana = 100, AC = 0, Int = 30, Wis = 0, Cha = 12, Res = 3 }.Generate(__instance).WandProcChance = 8;
            // Runed Shield = lots of extra defense, sacrificing int
            new EquipmentGenerator { Id = ItemId.RUNED_SHIELD, HP = 300, Mana = 60, AC = 100, End = 15, Int = 0, Wis = 12, Cha = 16, Res = 2 }.Generate(__instance);

            // Vanilla Brax's Candle just seems like... a worse Volcanic Sceptre that you unlock later. I'm removing its chance to prox Brax's Rage on hit, and instead giving it an activatable cast of something.
            // Also going to buff it and make it part of a set with the Brax Testament... after all, these two surely belong together, and Brax-themed gear is also extremely wizardly!

            var braxsCandle = new EquipmentGenerator { Id = ItemId.BRAXS_CANDLE, HP = 25, Mana = 25, End = 6, Int = 0, Wis = 16, Cha = 0, Res = 0, 
                ClickEffect = GameData.SpellDatabase.GetSpellByID(SpellDBStartPatch.DESERT_COFFIN_SPELL_ID),    SpellCastTime = 6f,
                Lore = "A large, ancient wand once wielded by a Braxonian High Priest. Only the incantation in the Braxonian Testament may awaken its power.",
            }.Generate(__instance);
            braxsCandle.WandEffect = null;
            braxsCandle.WandProcChance = 0;

            new EquipmentGenerator { Id = ItemId.BRAXONIAN_TESTAMENT, HP = 25, Mana = 25, AC = 6, Int = 0, Wis = 16, Cha = 0, Res = 0,
                Lore = "A dusty old tome. The cryptic runes can only be decoded using the cipher on Brax's Candle..."
            }.Generate(__instance);
            // The glowing blue stone, once paired with the Celestial Spike, is marginally more defensive than the SivaBrax set, and with 1 point higher res, but lower int/wis/cha, and obv lacking the Desert Tempest spell.
            // However, SivaBrax teachings are only obtained via a quest, therefore can't be dropped as rares etc. from the Loot Rarity mod -- making the glowing blue stone a solid endgame option if you manage a good drop!
            // How's that for a reason to go back to Fernella's and kill those spectres that whooped our arses earlier? ;)
            var glowingBlueStone = new EquipmentGenerator { Id = ItemId.GLOWING_BLUE_STONE, HP = 450, Mana = 210, AC = 35, Int = 21, Wis = 10, Cha = 13, Res = 5,
                Lore = "You've never seen the color blue look so vivid... But is there something that could make it brighter still?",
            }.Generate(__instance);
            new EquipmentGenerator { Id = ItemId.SIVA_BRAXONIAN_TEACHINGS, HP = 155, Mana = 155, AC = 6, Int = 0, Wis = 25, Cha = 5, Res = 0,
                Lore = "Wait... This contradicts the Braxonian Testament! It changes EVERYTHING about the Candle -- and reveals its TRUE power!"
            }.Generate(__instance);

            // Pleeeeenty of wands going around... Let's make this another 2 hander! And an endgame viable one at that.
            // Balancing with the following in mind:
            // - It's an easy drop to get from Statue of Brax, very farmable, 50% drop rate
            // - That means it's easier to farm for rare/fabled etc.
            // - Also if you get this as a rare drop, and it's as good as a 1h+offhand in stats, then that's half the time required compared with getting rare on *both* those items.
            // It's also only 1 item to bless+siva!
            // So, this will be balanced to be an "early endgame item" that will be outclassed by the other items if you manage to get good drops on both the 1h+offhand.
            new EquipmentGenerator { Id = ItemId.PETRIFIED_WOOD_CANE, HP = 650, Mana = 800, AC = 40, End = 13, Int = 55, Wis = 30, Cha = 45, Res = 10,
                WeaponType = Item.WeaponType.TwoHandStaff, SlotType = Item.SlotType.Primary,
            }.Generate(__instance);


            // Garg wand's role: previously it was BiS just based on it having 5 res, without much else going for it. Now I'm making it a mixed bag with a couple of great stats and the rest avg/0,
            // still 5 res which isn't as big as it used to be now that lots of item combos can give you 8-11ish,
            // and swapping out Magic Missile on hit for... INFERNIS! So a big advantage of using Garg is you can keep Infernis up on enemies without wasting mana/time to cast.
            // but if you don't mind doing that yourself anyway, maybe this isn't the weapon for you any more...
            new EquipmentGenerator { Id = ItemId.GARG_WAND, HP = 50, Mana = 200, Int = 35, Wis = 18, Cha = 0, Res = 5,
                WandEffect = GameData.SpellDatabase.GetSpellByID(SpellDBStartPatch.INFERNIS_SPELL_ID)
            }.Generate(__instance).WandProcChance = 30;

            // Endgame viable defensive offhander with int, wis and res
            new EquipmentGenerator { Id = ItemId.ULORS_ENCYCLOPEDIA, HP = 500, Mana = 200, AC = 40, End = 20, Int = 30, Wis = 22, Cha = 0, Res = 5 }.Generate(__instance);

            // Ehhh... Le'ts make this a very wis and cha focused mildly defensive off-hander, for some choice around which stats to prioritise
            new EquipmentGenerator { Id = ItemId.ASCENDED_REMAINS, HP = 250, Mana = 500, AC = 25, End = 10, Int = 6, Wis = 36, Cha = 21, Res = 5 }.Generate(__instance);

            // At last we come to you! When did I start writing comments for every item? *I DON'T KNOW!* But it's a thing now! I'm probably losing my mind!
            // I've tweaked way, way too many items! I don't know what I'm doing any more! LET'S GOOO!
            // This guy is already a motherfucking powerhouse in vanilla, and I still want to keep it like that -- a fantastic reward for beating Astra,
            // The effort involved in summoning+beating Astra -- and then only 20% drop chance -- means this is probably one of the harder items to get a rare etc. drop of.
            // So, having great stats out of the gate doesn't necessarily make it OP -- its base is competing with other items that have dropped twice as strong.
            // BUT, dropping Aetherstorm proc chance a little -- Aetherstorm does almost 50% more damage in Arcanism, after all.
            new EquipmentGenerator { Id = ItemId.SINGULARITY_VESSEL_OF_CREATION, HP = 700, Mana = 400, AC = 100, Int = 40, Wis = 14, Cha = 34, Res = 6 }.Generate(__instance).WandProcChance = 10;


            // because it's actually a weapon for attacking, we don't actually want to DOUBLE its delay
            // instead, we'll make the main boon of it the way everythign doubles -- twice as much damage, and twice as fast, so a 4x damage boost -- making it similar in strength to the melee weapons, but at range!
            // therefore, make the set bonus *subtract* half the existing weapon delay
            resonatingCrystal = GameObject.Instantiate(resonatingCrystal);
            resonatingCrystal.WeaponDly = -resonatingCrystal.WeaponDly * .5f; ; 
            setBonusesByItemId.Add(ItemId.CELESTIAL_SPIKE, new Dictionary<ItemId, Item>() {
                { ItemId.CRYSTALLISED_TACTICS, crystallisedTactics },
                { ItemId.RESONATING_CRYSTAL, resonatingCrystal },
                { ItemId.GLOWING_BLUE_STONE, glowingBlueStone },
            });

            var braxBraxBonus = ScriptableObject.CreateInstance<Item>();
            braxBraxBonus.SpellCastTime = -(braxsCandle.SpellCastTime * 0.25f);
            braxBraxBonus.HP = 350;
            braxBraxBonus.Mana = 250;
            braxBraxBonus.AC = 15;
            braxBraxBonus.End = 12;
            braxBraxBonus.Int = 44;
            braxBraxBonus.Wis = 18;
            braxBraxBonus.Cha = 22;
            braxBraxBonus.Res = 7;

            var braxSivaBonus = ScriptableObject.CreateInstance<Item>();
            braxBraxBonus.SpellCastTime = -(braxsCandle.SpellCastTime * 0.5f);
            braxBraxBonus.HP = 700;
            braxBraxBonus.Mana = 400;
            braxBraxBonus.AC = 50;
            braxBraxBonus.End = 20;
            braxBraxBonus.Int = 55;
            braxBraxBonus.Wis = 26;
            braxBraxBonus.Cha = 35;
            braxBraxBonus.Res = 9;

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
    }
}