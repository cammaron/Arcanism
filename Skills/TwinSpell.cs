using UnityEngine;
using System;
using System.Collections.Generic;
using Arcanism.Patches;
using HarmonyLib;
using Arcanism.SkillExtension;
using UnityEngine.Rendering.PostProcessing;

namespace Arcanism.Skills
{
    class TwinSpell : SpellAugmentationSkill, ISpellRetargetingSkill, ISpellDamageModifier
    {
        public const float FIRST_TWIN_MANA_COST = .5f;  // 1st twin target costs 50% extra mana on top of base spell
        public const float ADDITIONAL_TWIN_MANA_COST = .1f; // additional extra targets cost 10% each, so way more cost effective the more targets.

        public static bool AllowSameTarget = false; // just for dev testing ;)

        private Traverse<Stats> vesselTargetField;
        private Traverse<float> vesselEffectLifeField;
        private Character originalSpellTarget; 
        private readonly List<Character> allTargets = new List<Character>();

        int currentTargetIndex = 0;
        float showTextUntilTime;
        int maxTargets;
        bool hasAttacked = false;
        bool appliedParasiticTwin = false;
        bool isFinished = false;

        int extraResonanceAvailable;
        bool readyToResonate;

        HashSet<Character> npcsDiedWhileCasting = new HashSet<Character>();

        AudioSource audio;
        ColorGrading colorGrading;
        Bloom bloom;

        float origContrast;
        float origSaturation;
        float origBrightness;
        float origBloomIntensity;
        float origDiffusion;

        Vector3 origTargetTrackingScale;

        public static void CreateExtension(Skill coreSkill)
        {
            ExtensionManager.AddExtension(coreSkill, (caster, vessel) => {
                var component = caster.gameObject.GetComponent<TwinSpell>();
                if (component == null)
                {
                    component = caster.gameObject.AddComponent<TwinSpell>();
                    component.skill = coreSkill;
                    component.caster = caster;
                    component.Vessel = vessel;
                    component.audio = new GameObject("SlowMoAudio").AddComponent<AudioSource>();
                    component.audio.transform.SetParent(component.transform);
                    component.audio.volume = GameData.SFXVol * .5f;

                    if (Main.sfxByName.TryGetValue("slowmo", out var clip))
                        component.audio.clip = clip;
                    else
                        Main.Log.LogError("'slowmo' audio clip for Twin Spell not loaded!");

                    if (vessel != null)
                    {
                        var traversal = Traverse.Create(vessel);
                        component.vesselTargetField = traversal.Field<Stats>("targ");
                        component.vesselEffectLifeField = traversal.Field<float>("EffectLife");
                        component.originalSpellTarget = component.vesselTargetField.Value.Myself;
                        component.allTargets.Add(component.originalSpellTarget);
                    }

                    component.maxTargets = 1; // original target, plus one per skill v
                    if (caster.MySkills.KnowsSkill(SkillDB_Start.TWIN_SPELL_SKILL_ID)) component.maxTargets += 1;
                    if (caster.MySkills.KnowsSkill(SkillDB_Start.TWIN_SPELL_MASTERY_SKILL_ID)) component.maxTargets += 1;
                    if (caster.MySkills.KnowsSkill(SkillDB_Start.TWIN_SPELL_MASTERY_2_SKILL_ID)) component.maxTargets += 1;
                    component.extraResonanceAvailable = caster.MySkills.GetAscensionRank(SkillDB_Start.MIND_SPLIT_ASCENSION_ID);

                    var postProcessing = GameData.CamGetPPFX?.GetLivePPFX();
                    if (postProcessing != null)
                    {
                        if (postProcessing.TryGetSettings<ColorGrading>(out component.colorGrading))
                        {
                            component.origContrast = component.colorGrading.contrast;
                            component.origSaturation = component.colorGrading.saturation;
                            component.origBrightness = component.colorGrading.brightness;
                        } 
                        
                        if (postProcessing.TryGetSettings<Bloom>(out component.bloom))
                        {
                            component.origBloomIntensity = component.bloom.intensity;
                            component.origDiffusion = component.bloom.diffusion;
                        }
                    }

                    if (component.caster.MySkills.isPlayer)
                    {
                        var tracker = GameData.PlayerControl.GetComponentInChildren<TargetTracker>();
                        component.origTargetTrackingScale = tracker.transform.localScale;
                        tracker.transform.localScale *= 1.35f; // temporarily increase target tracker scaling as there are often enemies within spell range we can't tab too, which is super frustrating during Twin Spell 
                    }
                }

                return component;
            });
        }

        protected override void Update()
        {
            base.Update();

            if (caster.MySkills.isPlayer && Time.time >= showTextUntilTime)
                GameData.CB.OCTxt.transform.gameObject.SetActive(false);

            if (!hasAttacked)
            {
                foreach (var target in allTargets)
                {
                    if (IsTargetDead(target) && !npcsDiedWhileCasting.Contains(target))
                    {
                        npcsDiedWhileCasting.Add(target);
                        if (caster.MySkills.KnowsSkill(SkillDB_Start.VANISHING_TWIN_SKILL_ID))
                        {
                            GameData.Misc.CreateDmgPopClone("VANISHED!", target.transform).Num.color = Color.magenta;
                            UpdateSocialLog.LogAdd($"You channel the magic from {target.MyStats.MyName}'s dead body to the next living twin!", "#" + ColorUtility.ToHtmlStringRGB(Color.magenta));
                        }
                    }
                }
            }
        }

        protected void FixedUpdate()
        {
            if (vesselEffectLifeField != null)
                vesselEffectLifeField.Value += (60f * Time.deltaTime) * Mathf.Pow(2, allTargets.Count - 1);
        }

        void OnDestroy()
        {
            audio.Stop();
            Destroy(audio.gameObject);
            ResetCameraEffects();
            
            if (caster.MySkills.isPlayer)
            {
                GameData.CB.OCTxt.text = "-Controlled Chant-";
                var tracker = GameData.PlayerControl.GetComponentInChildren<TargetTracker>();
                tracker.transform.localScale = origTargetTrackingScale;
            }
                

        }

        void ResetCameraEffects()
        {
            Time.timeScale = 1f;
            if (colorGrading != null)
            {
                colorGrading.contrast.value = origContrast;
                colorGrading.saturation.value = origSaturation;
                colorGrading.brightness.value = origBrightness;
            }
            if (bloom != null)
            {
                bloom.intensity.value = origBloomIntensity;
                bloom.diffusion.value = origDiffusion;
            }
        }

        protected override IEnumerable<(Condition, string)> GetUseConditions()
        {
            return new List<(Condition, string)>()
            {
                ((caster, vessel, target) => caster.MySpells != null && vessel != null && caster.MySpells.isCasting(),                          "You must be channeling a spell to use this skill."),
                ((caster, vessel, target) => caster.MySpells.GetCurrentCast().Type == Spell.SpellType.Damage,                                   "This skill only works on DAMAGE spells."),
                ((caster, vessel, target) => target != null && target != caster,                                                                "Invalid target!"),
                ((caster, vessel, target) => caster.GetComponent<ControlChant>() == null,                                                       "Cannot be used while controlling a chant."),
                ((caster, vessel, target) => AllowSameTarget || !allTargets.Contains(target),                                                   "Cannot be used on the same target twice."),
                ((caster, vessel, target) => caster.MyStats.GetCurrentMana() - vessel.spell.ManaCost >= GetNextTargetManaCost(),                "You need more mana!"),
                ((caster, vessel, target) => CanAddMoreTargets(),                                                                               "Unable to twin this spell any further."),
                ((caster, vessel, target) => vessel.spell.SpellRange >= Vector3.Distance(caster.transform.position, target.transform.position), "Target is too far away!"),
            };
        }

        protected override bool ShouldApplyCooldownOnUse()
        {
            return false; // return !CanAddMoreTargets(); -- actually let's stick w/ cooling down only on spell release, that way we can cancel cast without punishment aside from lost mana.
        }

        protected override void ApplySkill(Character target)
        {
            caster.MyStats.ReduceMana(GetNextTargetManaCost());
            
            allTargets.Add(target);

            if (caster.MySkills.isPlayer)
            {
                string text;
                switch(allTargets.Count)
                {
                    case 2:
                        text = "Twinned!";
                        break;
                    case 3:
                        text = "TRIPLED!";
                        break;
                    case 4:
                        text = "FULL STACK!!";
                        break;
                    default:
                        text = "ERROR"; // all cases should have been handled
                        break;
                }
                var dmgPop = GameData.Misc.CreateDmgPopClone(text, target.transform);
                dmgPop.Num.color = Color.magenta;

                UpdateSocialLog.LogAdd($"Twinning spell to {target.name} (target {allTargets.Count}/{maxTargets})");
                GameData.CB.OCTxt.text = $"-{text}-";
                GameData.CB.OCTxt.transform.gameObject.SetActive(true);
                GameData.CB.OCTxt.fontSize = 48;
                showTextUntilTime = Time.time + 1f;

                GameData.CB.SetSubtext($"DMG {Mathf.RoundToInt(GetSiblingSynergyMulti() * 100f)}%", $"{allTargets.Count}/{maxTargets}");

                if (CanAddMoreTargets())
                {
                    if (colorGrading != null)
                    {
                        colorGrading.contrast.value += 10f;
                        colorGrading.saturation.value -= 80;
                        colorGrading.brightness.value += 5;
                    }
                    if (bloom != null)
                    {
                        bloom.intensity.value += 2.5f;
                        bloom.diffusion.value += 2;
                    }
                    Time.timeScale *= .5f;
                    if (allTargets.Count == 2)
                    {
                        if (audio.clip != null) audio.Play();
                    } else
                    {
                        audio.pitch *= .8f;
                    } 
                } else
                {
                    ResetCameraEffects();
                }
            }
        }

        public float GetSiblingSynergyMulti()
        {
            if (caster.MySkills.KnowsSkill(SkillDB_Start.SIBLING_SYNGERY_SKILL_ID))
                return 1f + (SkillDB_Start.SIBLING_SYNERGY_POWER * allTargets.Count);
            return 1f;
        }

        public float GetSpellDamageMulti()
        {
            float multi = 1;
            if (!hasAttacked) // This being called means SpellVessel is about to begin cycling targets. Whoever is already dead has died not by this spell's hand, so we process them for Vanishing Twin.
            {
                if (npcsDiedWhileCasting.Count > 0 && caster.MySkills.KnowsSkill(SkillDB_Start.VANISHING_TWIN_SKILL_ID))
                {
                    multi += npcsDiedWhileCasting.Count;
                    var remainingTarget = allTargets.Find(t => !IsTargetDead(t));
                    if (remainingTarget != null)
                    {
                        UpdateSocialLog.LogAdd($"{target.MyStats.MyName} receives {multi}x damage from Vanishing Twin!", "#" + ColorUtility.ToHtmlStringRGB(Color.magenta));
                        GameData.Misc.CreateDmgPopClone("x" + multi + "!!!", target.transform).Num.color = Color.magenta;
                    }
                }

                GameData.PlayerAud.PlayOneShot(Main.sfxByName["twin-release"]);

                // On the FIRST hit (which is the original spell, not a twin target), we're ready to resonate as long as it's off cooldown. 
                // This doesn't get checked by SpellVessel for this hit anyway, but it's necessary so we can compare whether resCD has *changed* before the next target,
                // implying the first hit resonated (which is a precondition to further resonating)
                readyToResonate = caster.MyStats.resonanceCD <= 0f;
            } else
            {
                readyToResonate = extraResonanceAvailable >= currentTargetIndex && readyToResonate && caster.MyStats.resonanceCD > 0f;
                if (readyToResonate)
                    UpdateSocialLog.LogAdd($"MIND SPLIT! You try to cascade your spell resonance!", "#" + ColorUtility.ToHtmlStringRGB(Color.magenta));
            }

            hasAttacked = true;

            multi *= GetSiblingSynergyMulti();

            return multi;
        }

        public Character GetNextTarget()
        {
            var lastTarget = allTargets[currentTargetIndex];

            // If the prior target is now dead, and wasn't already among those who died while waiting for cast, it means they were killed by Twin Spell, so proc Parasitic Twin
            if (!appliedParasiticTwin && IsTargetDead(lastTarget) && (npcsDiedWhileCasting == null || !npcsDiedWhileCasting.Contains(lastTarget)))
                ApplyParasiticTwin();

            // If we've just processed the final target, it's time to trigger cooldown for the skill (if it hasn't already been done)
            if (currentTargetIndex == allTargets.Count - 1)
            {
                isFinished = true;
                return null;
            }

            return allTargets[++currentTargetIndex];
        }

        public bool AllowResonatingOnCurrentTarget()
        {
            return readyToResonate;
        }

        private void ApplyParasiticTwin()
        {
            UpdateSocialLog.LogAdd($"Dead twins remain tethered to the living, draining their life force!", "#" + ColorUtility.ToHtmlStringRGB(Color.magenta));
            var parasiticTwinSpell = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.PARASITIC_TWIN_SPELL_ID);
            // Parasitic Twin *has* no base target damage, thus all its damage is the "bonus damage" -- it's completely calculated by int/prof, scaling with character growth. With a max int around 353ish (before blessings) and max int prof, this will hit for 1700 base (*BASE*, as in, before other int/prof scaling increases it. so, same as damage listing on any spell desc)
            int baseDamage = Mathf.RoundToInt(caster.MyStats.GetCurrentInt() * 5 * (caster.MyStats.IntScaleMod / 40f));
            int modifiedDamage = SpellVessel_CalcDmgBonus.CalculateSpellDamage(caster, baseDamage, false);

            foreach (var targ in allTargets)
            {
                if (!IsTargetDead(targ))
                {
                    targ.MyStats.AddStatusEffect(parasiticTwinSpell, caster.MySkills.isPlayer, modifiedDamage, caster);
                    var dmgPop = GameData.Misc.CreateDmgPopClone("PARASITIC TWIN!", targ.transform);
                    dmgPop.Num.color = Color.magenta;
                }
            }

            appliedParasiticTwin = true;
        }

        private int GetNextTargetManaCost() // NB the next target is NOT in allTargets yet
        {
            float factor;
            if (allTargets.Count == 1)
                factor = FIRST_TWIN_MANA_COST;
            else
                factor = ADDITIONAL_TWIN_MANA_COST;

            return Mathf.RoundToInt(Vessel.spell.ManaCost * factor); // [2025/11/13] - changing from 0 to test new formula that makes mroe targets rewarding in terms of mana ratio
            
            // Leaving the other code here in case I decide to rebalance this, but as of 04/11/2025 it seems like the mana cost made this skill undesirable to use, and the long cooldown already prevents it being OP
            // 1.3x exponent means 15% more mana than 2 casts for 2 targets; 33% more for 3; 54% more for 4. Remember, that's 54% more than *4 casts* which is already a HUGE amount of mana usage.
            /*float costExponent = 1.3f * (1 - caster.MySkills.GetAscensionRank(SkillDB_Start.MIND_SPLIT_ASCENSION_ID) * SkillDB_Start.MIND_SPLIT_COST_FACTOR);
            int additionalManaCost = (int)(Mathf.Pow(costExponent, allTargets.Count) * vessel.spell.ManaCost);

            return additionalManaCost;*/
        }

        protected override float CalculateCooldownFactor()
        {
            int refractionLevel = caster.MySkills.GetAscensionRank(SkillDB_Start.REFRACTION_ASCENSION_ID);
            return 1f - (refractionLevel * SkillDB_Start.REFRACTION_COOLDOWN_FACTOR);
        }

        private bool CanAddMoreTargets() => allTargets.Count < maxTargets;

        private bool IsTargetDead(Character target) => target == null || target.IsDead(); // it seems like NPCs are not destroyed, they just become dead bodies then respawn so in theory shouldn't null. Still...


        protected override bool IsInterrupted() => Vessel == null;

        protected override bool IsFinished() => isFinished;

        public IEnumerable<Character> GetAllTargets()
        {
            return allTargets;
        }
    }
}
