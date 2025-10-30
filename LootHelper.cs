﻿using Arcanism.Patches;
using System.Collections.Generic;
using UnityEngine;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism
{
    public class LootHelper : MonoBehaviour
    {
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
        protected bool containsSpecialLoot = false;
        protected bool finished = false;

        protected void Awake()
        {
            npc = GetComponent<NPC>();
        }

        protected void Update()
        {
            if (!finished && (npc == null || npc.GetChar().IsDead()))
            {
                CreateCorpseGlow();
                finished = true;
            }
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
        public void UpdateLootQuality()
        {
            itemMeta = new List<(Item, Blessing, Quality)>(lootTable.ActualDrops.Count);
            
            foreach(var item in lootTable.ActualDrops)
            {
                if (!IsRegularEquipment(item))
                {
                    itemMeta.Add((item, Blessing.NONE, Quality.NORMAL));
                    continue;
                }

                var blessLevel = ShouldBless() ? Blessing.BLESSED : Blessing.NONE;
                var qualityLevel = GenerateQualityLevel();
                itemMeta.Add((item, blessLevel, qualityLevel));
            }

            for(var i = 0; i < lootTable.ActualDropsQual.Count; i ++)
            {
                lootTable.ActualDropsQual[i] = 1; // actual drops qual stores the Random(0,100) that determines whether an item will become blessed when LootWindow opens. 0 = blessed. Setting to 1 so we don't double up with my own code.
            }

            this.containsSpecialLoot = lootTable.special;
            lootTable.special = false; // we'll be handling the glow ourselves
        }

        public void CreateCorpseGlow()
        {
            var glow = CorpseGlow.NONE;
            foreach (var meta in itemMeta)
            {
                if (meta.item.RequiredSlot == Item.SlotType.Charm || GameData.GM.WorldDropMolds.Contains(meta.item))
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.MISC);
                
                if (meta.quality == Quality.SUPERIOR || meta.item == GameData.GM.InertDiamond || meta.item == GameData.GM.XPPot || GameData.Misc.Masks.Contains(meta.item) || meta.item == GameData.Misc.MoloraiMask)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.SUPERIOR);
                
                if (meta.quality == Quality.MASTERWORK)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.MASTERWORK);

                if (meta.blessLevel == Blessing.BLESSED || meta.blessLevel == Blessing.GODLY|| meta.item == GameData.GM.PlanarShard)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.BLESSED);

                if (GameData.GM.Maps.Contains(meta.item))
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.MAP);

                if (meta.item == GameData.GM.Sivak || meta.item == GameData.GM.Planar)
                    glow = MaybeUpgradeCorpseGlow(glow, CorpseGlow.SIVAK);
            }

            if (glow == CorpseGlow.NONE)
            {
                if (!containsSpecialLoot) // this is just in case new stuff is added to the game that doesn't fit the above criteria but is still meant to glow
                    return;
                glow = CorpseGlow.BLESSED;
            }

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
                    color = ItemIconVisuals.SUPERIOR_COLOR;
                    break;
                case CorpseGlow.MASTERWORK:
                    scaleX = 1.25f;
                    scaleY = 0.5f;
                    color = ItemIconVisuals.MASTERWORK_COLOR;
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

            main.prewarm = true;
            main.startSize = 2;
            main.startLifetime = 4;
            main.simulationSpeed = 1;
            emission.rateOverTime = 10;

            rotationLifetime.enabled = true; 
            rotationLifetime.separateAxes = true;
            rotationLifetime.x = rotationLifetime.z = 0f;
            rotationLifetime.y = 7.5f;

            if (color.HasValue) main.startColor = new ParticleSystem.MinMaxGradient(new Color32(color.Value.r, color.Value.g, color.Value.b, 30));
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

                if (!IsRegularEquipment(item))
                    continue;

                slot.Quantity = (int) blessLevel + (int) quality;

                if (i < lootTable.qualUps.Count) // qualUps appears to be used as a cache for blessing status per item if loot window has been opened before.
                    lootTable.qualUps[i] = slot.Quantity;
            }
        }

        protected bool IsRegularEquipment(Item item)
        {
            return (item.RequiredSlot != Item.SlotType.General && item.RequiredSlot != Item.SlotType.Aura && item.RequiredSlot != Item.SlotType.Charm);
        }

        protected bool ShouldBless()
        {
            return Random.Range(0, 100) == 0;
        }

        protected Quality GenerateQualityLevel()
        {
            var chance = Random.Range(0, 100);
            if (chance < 1.5f)
                return Quality.MASTERWORK;
            else if (chance < 8f)
                return Quality.SUPERIOR;
            else if (chance < 28)
                return Quality.JUNK;
            
            return Quality.NORMAL;
        }
    }
}
