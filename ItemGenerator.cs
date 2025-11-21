using Arcanism.Patches;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanism
{
    public class ItemGenerator
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
        public float? WandProcChance;

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

        public float? SpellCastSeconds;

        public Skill TeachSkill;

        public ItemGenerator TuneWand(int damage, float attackDelay, int range, string spellId = null, float procChance = 0) // juuuust an extra lil helper to make doing another pass over all the wands less annoying
        {
            this.Damage = damage;
            this.IsWand = true;
            this.WandRange = range;
            this.AttackDelay = attackDelay;
            if (spellId != null) WandEffect = GameData.SpellDatabase.GetSpellByID(spellId);
            if (procChance > 0) WandProcChance = procChance;
            return this;
        }

        public ItemGenerator NewSkillBook(ItemId id, string skillId, int value)
        {
            this.Id = id;
            this.CreateFromBaseId = ItemId.ARCANE_RECOVERY;
            this.TeachSkill = GameData.SkillDatabase.GetSkillByID(skillId);
            this.Name = $"Skill Book: {this.TeachSkill.SkillName}";
            this.Level = this.TeachSkill.ArcanistRequiredLevel;
            this.Value = value;
            return this;
        }

        public ItemGenerator NewEffectScroll(ItemId id, string spellId, int value)
        {
            this.Id = id;
            this.CreateFromBaseId = ItemId.BREAD;
            this.ClickEffect = GameData.SpellDatabase.GetSpellByID(spellId);
            this.Name = $"Scroll of {this.ClickEffect.SpellName}";
            this.Value = value;
            this.Lore = this.ClickEffect.SpellDesc;
            return this;
        }

        public ItemGenerator SoldBy(NpcName npcName)
        {
            if (!ItemDatabase_Start.itemsSoldByVendor.TryGetValue(npcName, out HashSet<ItemId> set))
            {
                set = new HashSet<ItemId>();
                ItemDatabase_Start.itemsSoldByVendor.Add(npcName, set);
            }

            set.Add(this.Id);

            return this;
        }

        public ItemGenerator DroppedBy(NpcName npcName, DropChance dropChance)
        {
            if (!ItemDatabase_Start.dropsByNpc.TryGetValue(npcName, out Dictionary<ItemId, DropChance> chanceByItemId))
            {
                chanceByItemId = new Dictionary<ItemId, DropChance>();
                ItemDatabase_Start.dropsByNpc.Add(npcName, chanceByItemId);
            }

            chanceByItemId[this.Id] = dropChance;

            return this;
        }

        public Item Generate(ItemDatabase db)
        {
            Item item;
            if (CreateFromBaseId.HasValue)
            {
                item = GameObject.Instantiate(db.GetItemByID(CreateFromBaseId.Value));
                item.Id = Id.Id();
            }
            else
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
            if (WandProcChance.HasValue) item.WandProcChance = WandProcChance.Value;

            if (WornEffect != null) item.WornEffect = WornEffect;
            if (ClickEffect != null)
            {
                item.ItemEffectOnClick = ClickEffect;
                if (!SpellCastSeconds.HasValue) item.SpellCastTime = ClickEffect.SpellChargeTime;
            }
            if (WandEffect != null) item.WandEffect = WandEffect;

            if (item.IsWand && item.WeaponProcOnHit != null) // Some fixes for the fact that I'm converting some weapons to wands and want their effects to translate (initially didn't realise these were separate settings. because... they should not be)
            {
                if (item.WandEffect == null)
                {
                    item.WandEffect = item.WeaponProcOnHit;
                    item.WeaponProcOnHit = null;
                }
                if (item.WandProcChance == 0 && item.WeaponProcChance > 0)
                {
                    item.WandProcChance = item.WeaponProcChance;
                    item.WeaponProcChance = 0;
                }

            }

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

            if (SpellCastSeconds.HasValue) item.SpellCastTime = SpellCastSeconds.Value * 60f;

            if (this.TeachSkill != null) item.TeachSkill = this.TeachSkill;

            if (CreateFromBaseId.HasValue)
                Main.Log.LogInfo($"Item created: {Name}");
            else
                Main.Log.LogInfo($"Item updated: {origName}");

            return item;
        }


    }
}
