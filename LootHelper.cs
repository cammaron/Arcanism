using Arcanism.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism
{
    public class LootHelper : MonoBehaviour
    {
        public const float LUCK_OF_SOLUNA_BUFF_FACTOR = 2f;

        enum CorpseGlow
        {
            NONE = 0,
            MISC = 1,
            SUPERIOR = 2,
            MASTERWORK = 3,
            BLESSED = 4,
            MAP = 5,
            SIVAK = 6
        }

        public LootTable lootTable;
        public List<(Item item, Blessing blessLevel, Quality quality)> itemMeta;
        protected NPC npc;
        protected TreasureChestEvent treasureChest;
        protected bool finished = false;

        protected void Awake()
        {
            npc = GetComponent<NPC>();
            treasureChest = GetComponent<TreasureChestEvent>();
        }

        protected void Update()
        {
            if (!finished && (IsOpenChest() || npc == null || npc.GetChar().IsDead()))
            {
                bool isSim = (npc?.SimPlayer).GetValueOrDefault(false);
                if (!isSim)
                {
                    UpdateLootQuality();
                    CreateCorpseGlow();
                }
                finished = true;
            }
        }

        private bool IsOpenChest() {
            return treasureChest != null && (treasureChest.Lid == null || !treasureChest.Lid.activeSelf);
        }

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
                    Main.Log.LogInfo($"{npc.NPCName} has custom drops to register.");
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

                        if (!relevantList.Contains(drop.Item2)) 
                        {
                            Main.Log.LogInfo($"Adding item {drop.Item2.ItemName} to {drop.Item1} loot list");
                            if (drop.Item1 == DropChance.UNCOMMON || drop.Item1 == DropChance.RARE)
                                lootTable.MaxNonCommonDrops += 1;
                            lootTable.MaxNumberDrops += 1;
                            relevantList.Add(drop.Item2);
                        }
                    }
                }
            }
        }

        public void UpdateLootQuality()
        {
            if (itemMeta != null) return;

            float lootRate = GetLootRateMulti();
            if (lootRate > GameData.ServerLootRate) // for this particular instance, loot rate is higher (maybe giant or supermassive), which means the actual InitLootTable method needs to be re-run with the right rate
            {
                lootTable.ActualDrops.Clear();
                lootTable.ActualDropsQual.Clear();
                lootTable.RegenerateLoot(lootRate);
            }

            itemMeta = new List<(Item, Blessing, Quality)>(lootTable.ActualDrops.Count);
            
            foreach(var item in lootTable.ActualDrops)
            {
                if (!item.IsUpgradeableEquipment())
                {
                    itemMeta.Add((item, Blessing.NONE, Quality.NORMAL));
                    continue;
                }

                var blessLevel = ShouldBless(lootRate) ? Blessing.BLESSED : Blessing.NONE;
                var qualityLevel = GenerateQualityLevel(lootRate);
                itemMeta.Add((item, blessLevel, qualityLevel));
            }

            for(var i = 0; i < lootTable.ActualDropsQual.Count; i ++)
            {
                lootTable.ActualDropsQual[i] = 1; // actual drops qual stores the Random(0,100) that determines whether an item will become blessed when LootWindow opens. 0 = blessed. Setting to 1 so we don't double up with my own code.
            }
        }

        protected void CreateCorpseGlow()
        {
            var glow = CorpseGlow.NONE;
            foreach (var meta in itemMeta)
            {
                if (meta.item == null) continue; // shouldn't happen, but been getting some NPEs from somewhere in here
                if (meta.item.RequiredSlot == Item.SlotType.Charm || GameData.GM.WorldDropMolds.Contains(meta.item))
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.MISC);
                
                if (meta.quality == Quality.SUPERIOR || meta.item == GameData.GM.InertDiamond || meta.item == GameData.GM.XPPot || GameData.Misc.Masks.Contains(meta.item) || meta.item == GameData.Misc.MoloraiMask)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.SUPERIOR);
                
                if (meta.quality == Quality.MASTERWORK)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.MASTERWORK);

                if (meta.blessLevel == Blessing.BLESSED || meta.blessLevel == Blessing.GODLY || meta.item == GameData.GM.PlanarShard)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.BLESSED);

                if (GameData.GM.Maps.Contains(meta.item))
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.MAP);

                if (meta.item == GameData.GM.Sivak || meta.item == GameData.GM.Planar || meta.quality == Quality.EXQUISITE)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.SIVAK);
            }

            if (glow == CorpseGlow.NONE)
                return;

            if (lootTable == null) return; // shouldn't happen, but been getting some NPEs from somewhere in here
            var parent = lootTable.transform;
            ParticleSystem beam = Instantiate(GameData.GM.SpecialLootBeam, parent.position + Vector3.up * 0.3f, parent.rotation).GetComponent<ParticleSystem>();
            beam.transform.SetParent(parent);
            
            Color32? color = null;

            var capsule = lootTable.GetComponent<CapsuleCollider>();
            Vector3 baseScale = new Vector3(1, 1, 1);
            if (capsule != null)
            {
                baseScale = new Vector3(capsule.radius, capsule.height, capsule.radius);
            }

            float scaleX = 1;
            float scaleY = 1;

            switch (glow)
            {
                case CorpseGlow.MISC:
                    scaleX = 1.25f;
                    scaleY = 0.5f;
                    color = new Color32(255, 255, 255, 255);
                    break;
                case CorpseGlow.SUPERIOR:
                    scaleX = 1.25f;
                    scaleY = 0.5f;
                    color = SUPERIOR_COLOR;
                    break;
                case CorpseGlow.MASTERWORK:
                    scaleX = 1.25f;
                    scaleY = 0.5f;
                    color = MASTERWORK_COLOR;
                    break;
                case CorpseGlow.MAP:
                    scaleX = 1.25f;
                    scaleY = 0.5f;
                    color = new Color32(167, 126, 101, 255);
                    break;
                case CorpseGlow.BLESSED:
                    scaleX = .8f; // bigger than vanilla
                    scaleY = 2.3f;
                    color = new Color32(140, 140, 255, 255);
                    break;
                case CorpseGlow.SIVAK:
                    scaleX = 1.75f;
                    scaleY = 5f;
                    color = Color.yellow;
                    break;
            }

            scaleX = Mathf.Min(2.5f, scaleX + baseScale.x);
            scaleY = Mathf.Min(8f, scaleY + baseScale.y * .15f);
            beam.transform.localScale = new Vector3(scaleX, scaleY, scaleX);
            
            var main = beam.main;
            var emission = beam.emission;
            var rotationLifetime = beam.rotationOverLifetime;
            var colorLifetime = beam.colorOverLifetime;

            main.prewarm = true;
            main.startSize = 2;
            main.startLifetime = 4;
            main.simulationSpeed = 1;
            emission.rateOverTime = 10;

            rotationLifetime.enabled = true; 
            rotationLifetime.separateAxes = true;
            rotationLifetime.x = rotationLifetime.z = 0f;
            rotationLifetime.y = 7.5f;

            if (color.HasValue)
            {
                main.startColor = new ParticleSystem.MinMaxGradient(new Color32(color.Value.r, color.Value.g, color.Value.b, 30));
                colorLifetime.enabled = false;
            }
            npc?.GetChar()?.MyAudio?.PlayOneShot(Main.sfxByName["quality-drop"], GameData.SFXVol * .7f);
        }

        CorpseGlow MaybeUpgradeCorpseGlow(CorpseGlow original, CorpseGlow newVal)
        {
            if (newVal > original) return newVal;
            return original;
        }

        public void UpdateLootWindowQualities(LootWindow window)
        {
            for (var i = 0; i < itemMeta.Count; i++)
            {
                var meta = itemMeta[i];
                var slot = window.LootSlots[i];
                var item = meta.Item1;
                var blessLevel = meta.Item2;
                var quality = meta.Item3;

                if (!item.IsUpgradeableEquipment())
                    continue;

                slot.Quantity = (int) blessLevel + (int) quality;

                if (i < lootTable.qualUps.Count) // qualUps appears to be used as a cache for blessing status per item if loot window has been opened before.
                    lootTable.qualUps[i] = slot.Quantity;
            }
        }

        protected bool ShouldBless(float lootRate)
        {
            float percentChance = 1.5f * lootRate; // vanilla is 1%, but too rare imo

            
            return Random.Range(0f, 1f) * 100f <= percentChance;
        }

        protected Quality GenerateQualityLevel(float lootRate)
        {
            var roll = Random.Range(0f, 1f) * 100f;
            roll /= lootRate;


            if (npc != null && npc.GetChar().MyStats != null)
            {
                bool hurtByPlayer = npc.AggroTable != null && npc.AggroTable.Exists(aggro => aggro != null && aggro.Player != null && aggro.Player.transform.name == "Player");
                var luckSpellIds = new List<string>() {
                    SpellDB_Start.LUCK_OF_SOLUNA_1_SPELL_ID,
                    SpellDB_Start.LUCK_OF_SOLUNA_2_SPELL_ID,
                    SpellDB_Start.LUCK_OF_SOLUNA_3_SPELL_ID,
                    SpellDB_Start.LUCK_OF_SOLUNA_4_SPELL_ID,
                    SpellDB_Start.LUCK_OF_SOLUNA_5_SPELL_ID,
                };

                var luckOfSoluna = GameData.PlayerStats.StatusEffects.FirstOrDefault(se => se?.Effect != null && luckSpellIds.Contains(se.Effect.Id));
                if (luckOfSoluna != default)
                {
                    var enemyLevel = npc.GetChar().MyStats.Level;
                    int maxEnemyLevelForSolunaBuff = luckOfSoluna.Effect.RequiredLevel - 1;

                    if (enemyLevel <= maxEnemyLevelForSolunaBuff)
                        roll /= LUCK_OF_SOLUNA_BUFF_FACTOR;
                }
            }

            if (roll <= .5f)
                return Quality.EXQUISITE;
            else if (roll <= 3f)
                return Quality.MASTERWORK;
            else if (roll <= 15f)
                return Quality.SUPERIOR;
            else if (roll >= 80)
                return Quality.JUNK;
            
            return Quality.NORMAL;
        }

        protected float GetLootRateMulti()
        {
            float rate = GameData.ServerLootRate;

            var maybeGiant = GetComponent<TheyMightBeGiants>();
            if (maybeGiant != null)
            {
                if (maybeGiant.GetState() == TheyMightBeGiants.State.GIANT)
                    rate *= 4f;
                else if (maybeGiant.GetState() == TheyMightBeGiants.State.SUPERMASSIVE)
                    rate *= 8f;
            }
            
            return rate;
        }
    }
}
