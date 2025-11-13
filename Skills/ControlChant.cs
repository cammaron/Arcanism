using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using Arcanism.Patches;
using Arcanism.SkillExtension;
using UnityEngine.UI;

namespace Arcanism.Skills
{
    class ControlChant : ExtendedSkill, ISpellDamageModifier, ISpellCooldownModifier
    {
        // These values represent *additional* factors, i.e. a value of 0.5 means +50% compared to normal
        public static readonly float EXTRA_CAST_TIME_FACTOR = 0.6f;
        public static readonly float EXTRA_MANA_COST_FACTOR = 0.45f;
        public static readonly float EXTRA_DAMAGE_FACTOR = .8f;
        public static readonly float EXTRA_COOLDOWN_FACTOR = 0.75f;

        public static readonly float BEYOND_CHANT_CAST_TIME_FACTOR = 1f;
        public static readonly float BEYOND_CHANT_DAMAGE_FACTOR = 0.25f;  // Multiplicative with total damage -- so (damage + (damage*EXTRA_DAMAGE_FACTOR)) * BEYOND_CHANT_DAMAGE_FACTOR
        public static readonly float BEYOND_CHANT_MANA_COST_FACTOR = 1f;

        // 2.5% extra damage per second chanting over 6 seconds.
        // For a 5s spell, before BC, this is 5% dmg increase. AFter BC, 18.8%.
        // For a 6s spell, before BC, this is 9% dmg increase. AFter BC, 26%.
        // For an 8s spell, before BC 18%. After, 44%.
        public static readonly float TIME_IS_POWER_EXPONENT = 1.025f; 
        public static readonly float SECONDS_BEFORE_TIME_IS_POWER = 6f;

        public static readonly float PERFECT_RELEASE_TIMING_FACTOR = 0.022f; //.03f

        protected readonly static Color32 NORMAL_CAST_COLOR = new Color32(239, 18, 122, 255);
        protected readonly static Color32 PERFECT_RELEASE_COLOR = new Color32(255, 235, 72, 255); 
        protected readonly static Color32 OVER_CAST_COLOR = new Color32(178, 72, 250, 255);
        protected readonly static Color32 BEYOND_CHANT_COLOR = new Color32(6, 227, 250, 255);

        protected enum State
        {
            INITIALISING,
            ACTIVATED,
            LOCKED,
            FINISHED
        }

        protected enum CastBarState
        {
            NORMAL,
            PERFECT,
            OVER,
            BEYOND
        }

        protected State state;
        protected CastBarState castBarState = CastBarState.NORMAL;
        protected float castBarStateFactor;

        Traverse<float> effectLife;
        Traverse<float> overChantTotalField;
        Traverse<float> maxCastTime;
        Traverse<Transform> npcCastBarField;

        protected float timeSpentChanting;
        protected float normalCastEndTime;
        protected float overchantEndTime;
        protected float beyondChantEndTime;
        protected (float start, float end) perfectReleaseTiming;

        protected float beyondChantManaDrainPerSecond;

        protected int overchantTotalManaDrain;
        protected float overchantManaDrainPerSecond;
        protected float drainProgress = 0f;

        protected float damageMulti = 1f;
        protected float cooldownMulti = 1f;
        protected bool playingChargeSound;
        protected bool hasPlayedPerfectReleaseChime;

        protected Image castBar;
        protected Image overchantBar;
        protected Spell exhaustionEffect;

        protected AudioSource chargeAudioSource;

        protected bool knowsPerfectRelease1;
        protected bool knowsTimeIsPower1;
        protected bool knowsTimeIsPower2;
        protected bool knowsTimeIsPower3;
        protected bool knowsExpertControl2;
        protected int expertControlPower;

        public static void CreateExtension(Skill coreSkill)
        {
            ExtensionManager.AddExtension(coreSkill, (caster, vessel) => {
                var component = caster.gameObject.GetComponent<ControlChant>();
                if (component == null)
                {
                    component = caster.gameObject.AddComponent<ControlChant>();

                    component.state = State.INITIALISING;
                    component.skill = coreSkill;
                    component.caster = caster;
                    component.Vessel = vessel;

                    if (vessel != null)
                    {
                        var vesselTraversal = Traverse.Create(vessel);
                        component.effectLife = vesselTraversal.Field<float>("EffectLife");
                        component.maxCastTime = vesselTraversal.Field<float>("maxTime");
                        component.overChantTotalField = vesselTraversal.Field<float>("overChantTotal");
                        component.npcCastBarField = vesselTraversal.Field<Transform>("CastBar");
                    }

                }


                return component;
            });
        }

        protected override IEnumerable<(Condition, string)> GetUseConditions()
        {
            return new List<(Condition, string)>() {
                ((caster, vessel, target) => caster.MySpells != null && vessel != null && caster.MySpells.isCasting(),  "You must be channeling a spell to use this skill."),
                ((caster, vessel, target) => caster.MySpells.GetCurrentCast().Type == Spell.SpellType.Damage,           "This skill only works on DAMAGE spells."),
                ((caster, vessel, target) => caster.GetComponent<TwinSpell>() == null,                                  "You WISH you could stack this with Twin Spell... but no. You can't."),
                ((caster, vessel, target) => vessel.UseMana,                                                            "Can't be used with spells that don't channel mana.")
            };
        }

        protected override void ApplySkill(Character target)
        {
            // this could be the original use (beginning chant) or the final use (ending chant)
            // and that logic could all be handled here, however it's all hardcoded into vanilla UseSkill and SpellVessel
            // so... just leaving this blank for now?
            if (state == State.INITIALISING)
            {
                // The skill lookups are inefficient due to being lists rather than sets, so caching knowledge that might otherwise have to be done per frame
                knowsPerfectRelease1 = caster.MySkills.KnowsSkill(SkillDB_Start.PERFECT_RELEASE_SKILL_ID);
                knowsTimeIsPower1 = caster.MySkills.KnowsSkill(SkillDB_Start.TIME_IS_POWER_SKILL_ID);
                knowsTimeIsPower2 = caster.MySkills.KnowsSkill(SkillDB_Start.TIME_IS_POWER_2_SKILL_ID);
                knowsTimeIsPower3 = caster.MySkills.KnowsSkill(SkillDB_Start.TIME_IS_POWER_3_SKILL_ID);
                knowsExpertControl2 = caster.MySkills.KnowsSkill(SkillDB_Start.EXPERT_CONTROL_2_SKILL_ID);

                expertControlPower = 0;
                if (caster.MySkills.KnowsSkill(SkillDB_Start.EXPERT_CONTROL_SKILL_ID))
                    expertControlPower += 1;

                if (knowsExpertControl2)
                    expertControlPower += 2;

                // First, establish where we're at in the cast process and the various cast thresholds
                timeSpentChanting = effectLife.Value;

                normalCastEndTime = Vessel.spell.SpellChargeTime;
                float overchantDuration = Vessel.spell.SpellChargeTime * EXTRA_CAST_TIME_FACTOR;
                float beyondChantDuration = Vessel.spell.SpellChargeTime * BEYOND_CHANT_CAST_TIME_FACTOR;

                overchantEndTime = normalCastEndTime + overchantDuration;
                beyondChantEndTime = overchantEndTime + beyondChantDuration;

                
                if (knowsTimeIsPower2)
                {
                    overChantTotalField.Value = overchantDuration + beyondChantDuration;
                    maxCastTime.Value = beyondChantEndTime * 1.2f;
                } else
                {
                    overChantTotalField.Value = overchantDuration;
                    maxCastTime.Value = overchantEndTime * 1.2f;
                }

                perfectReleaseTiming = (normalCastEndTime - normalCastEndTime * PERFECT_RELEASE_TIMING_FACTOR, normalCastEndTime + normalCastEndTime * PERFECT_RELEASE_TIMING_FACTOR);

                // For overchanting, we drain additional mana per second to sustain the process
                // A full overchant should drain an additional X% of the spell's normal mana cost for up to Y% more damage
                overchantTotalManaDrain = Mathf.RoundToInt(Vessel.spell.ManaCost * EXTRA_MANA_COST_FACTOR);
                overchantManaDrainPerSecond = overchantTotalManaDrain / (overchantDuration / 60f); //    The "/ 60f" is because SpellChargeTime/castTime/overChantTotal is measured in 60ths of a second
                
                float beyondChantTotalManaDrain = Mathf.RoundToInt(Vessel.spell.ManaCost * BEYOND_CHANT_MANA_COST_FACTOR);
                beyondChantManaDrainPerSecond = beyondChantTotalManaDrain / (beyondChantDuration / 60f);

                chargeAudioSource = new GameObject("ChargeAudio").AddComponent<AudioSource>();
                chargeAudioSource.transform.SetParent(caster.transform);

                state = State.ACTIVATED;
            }

            // SpellVessel already internally handles (and depends on) some chant control state so we delegate back there and just intercept at the points when establishing damage and cooldown
            Vessel.DoControlledChant();
        }
        
        protected void Awake()
        {
            castBar = GameData.CB.TopBar.GetComponent<Image>();
            overchantBar = GameData.CB.OverchantBar.GetComponent<Image>();
            castBar.sprite = overchantBar.sprite = Main.miscSpritesByName["castbar"];
            overchantBar.transform.localScale = new Vector3(1f, 1.3f, 1f); // because the original sprite was 13px high as opposed to castBar's 10px
            overchantBar.transform.localPosition = castBar.transform.localPosition + Vector3.left * .1f; // correcting pixel alignment issue
            exhaustionEffect = GameData.SpellDatabase.GetSpellByID(SpellDB_Start.EXHAUSTION_SPELL_ID);
        }

        protected void OnDestroy()
        {
            if (castBar != null) castBar.color = NORMAL_CAST_COLOR;
            if (overchantBar != null) overchantBar.color = OVER_CAST_COLOR;
            if (chargeAudioSource != null) Destroy(chargeAudioSource.gameObject);
        }

        protected void FixedUpdate()
        {
            if (state == State.ACTIVATED)
            {
                timeSpentChanting += 1f * (60f * Time.deltaTime); // NB this probably should be Time.fixedDeltaTime but I'm just emulating the expectations of vanilla code
                UpdateBarState();

                float manaDrainPerSecond = 0f;
                if (castBarState == CastBarState.OVER)
                    manaDrainPerSecond = overchantManaDrainPerSecond;
                else if (castBarState == CastBarState.BEYOND)
                    manaDrainPerSecond = beyondChantManaDrainPerSecond;

                if (manaDrainPerSecond > 0)
                {
                    drainProgress += manaDrainPerSecond * Time.fixedDeltaTime; // mana is an int, so we build tiny increments per frame until we get a whole number to deduct
                    if (drainProgress >= 1f)
                    {
                        int manaReductionAmount = Mathf.FloorToInt(drainProgress);
                        drainProgress -= manaReductionAmount;
                        caster.MyStats.ReduceMana(manaReductionAmount);
                        
                        if (caster.MyStats.CurrentMana <= Vessel.spell.ManaCost) // At least enough mana to finish casting the spell (which isn't deducted 'til execution time) must remain in the bank
                        {
                            Backfire(Vessel.spell.ManaCost + overchantTotalManaDrain); // NB we use the same backfire damage even if we're actually in a BEYOND chant
                            damageMulti = 0f;
                            cooldownMulti = 1f;
                            state = State.LOCKED;
                            Vessel.ResolveEarly();
                        }
                    }
                }
            }
        }

        protected void UpdateBarState()
        {
            bool isPlayer = caster.MySkills.isPlayer;
            if (isPlayer) castBar.color = NORMAL_CAST_COLOR;

            if (!playingChargeSound)
            {
                float realEndTime = overchantEndTime;
                if (knowsTimeIsPower2)
                    realEndTime = beyondChantEndTime;

                float secondsRemaining = (realEndTime - timeSpentChanting) / 60f;
                if (secondsRemaining <= 7f)
                {
                    playingChargeSound = true;
                    chargeAudioSource.PlayOneShot(Main.sfxByName["overchant-charge"], GameData.SFXVol * caster.MyAudio.volume * 0.8f);
                }
            }
            
            if (IsInTimingWindow(0, normalCastEndTime, out castBarStateFactor))
            {
                // cast bar color should be normal
                // cast bar % should be timeSpent / normalCastEndTime
                if (isPlayer)
                    GameData.CB.TopBar.sizeDelta = new Vector2(castBarStateFactor * 275f, GameData.CB.TopBar.sizeDelta.y);
                castBarState = CastBarState.NORMAL;
            } else
            {
                if (isPlayer)
                    GameData.CB.TopBar.sizeDelta = new Vector2(275f, GameData.CB.TopBar.sizeDelta.y);

                if (IsInTimingWindow(normalCastEndTime, overchantEndTime, out castBarStateFactor))
                {
                    // NB: Overchant-charge sound is *exactly* 7 seconds (audioclip length is 8 but last second is silence)
                    if (isPlayer)
                    {
                        overchantBar.color = OVER_CAST_COLOR;
                        GameData.CB.OCBarRect.sizeDelta = new Vector2(castBarStateFactor * 275f, GameData.CB.OCBarRect.sizeDelta.y);
                    }

                    castBarState = CastBarState.OVER;
                }
                else if (knowsTimeIsPower2 && IsInTimingWindow(overchantEndTime, beyondChantEndTime, out castBarStateFactor))
                {
                    if (isPlayer)
                    {
                        castBar.color = OVER_CAST_COLOR;
                        overchantBar.color = BEYOND_CHANT_COLOR;
                        GameData.CB.OCBarRect.sizeDelta = new Vector2(castBarStateFactor * 275f, GameData.CB.OCBarRect.sizeDelta.y);
                    }

                    castBarState = CastBarState.BEYOND;
                }
            }

            if (!isPlayer) npcCastBarField.Value.localScale = new Vector2(castBarStateFactor, npcCastBarField.Value.localScale.y);

                // Perfect release overrides any other colour and state. Its factor is irrelevant.
            if (IsPerfectReleaseReady())
            {
                if (!hasPlayedPerfectReleaseChime)
                {
                    caster.MyAudio.PlayOneShot(Main.sfxByName["perfect-release-window"], GameData.SFXVol * caster.MyAudio.volume * 2f);
                    hasPlayedPerfectReleaseChime = true;
                }
                if (isPlayer) castBar.color = overchantBar.color = PERFECT_RELEASE_COLOR;
                castBarState = CastBarState.PERFECT;
            }

            if (isPlayer)
            {
                UpdateDamageMulti();
                UpdateCooldownMulti();
                
                if (castBarState == CastBarState.PERFECT)
                    GameData.CB.GetSubtext().color = PERFECT_RELEASE_COLOR;
                else
                    GameData.CB.GetSubtext().color = Color.white;

                string left;
                if (castBarState == CastBarState.NORMAL)
                    left = $"RISK {Mathf.RoundToInt(CalculateBackfireChance())}%";
                else
                    left = $"DMG {Mathf.RoundToInt(damageMulti * 100)}%";

                GameData.CB.SetSubtext(left, $"CD {Mathf.RoundToInt(cooldownMulti * 100f)}%");
            }
        }

        protected bool IsInTimingWindow(float start, float end, out float factor)
        {
            factor = Mathf.InverseLerp(start, end, timeSpentChanting);
            return timeSpentChanting >= start && timeSpentChanting < end;
        }

        protected bool IsPerfectReleaseReady()
        {
            if (!knowsPerfectRelease1)
                return false;

            if (caster.MyStats.CheckForStatus(exhaustionEffect))
                return false;

            return IsInTimingWindow(perfectReleaseTiming.start, perfectReleaseTiming.end, out var whatever);
        }

        protected float CalculateBackfireChance()
        {
            if (castBarState != CastBarState.NORMAL)
                return 0f;

            float backfireChance = (1 - (castBarStateFactor)) * 100f;
            for(var i = 0; i < expertControlPower; i ++)
                backfireChance = (backfireChance - 7.5f) * .85f; // Every Expert Control power level drops the backfire chance a fair bit

            return Mathf.Max(0f, backfireChance);
        }

        protected void HandleEarlyRelease()
        {
            if (Random.Range(0, 100) < CalculateBackfireChance())
            {
                // A FAILED early release backfires dealing damage to the caster and still using full mana+cooldown
                damageMulti = 0f;
                cooldownMulti = 1f;
                
                if (knowsExpertControl2)
                    damageMulti += .5f; // With 2nd skill, early release backfires still do 50% of their normal damage to enemies, so it's not a *complete* waste

                Backfire(Mathf.RoundToInt(Vessel.spell.ManaCost));
                
                state = State.LOCKED;
            } else
            {
                // Successful early release deals full damage with reduced cooldown and mana cost
                Vessel.UseMana = false;
                caster.MyStats.ReduceMana(Mathf.RoundToInt(Vessel.spell.ManaCost * castBarStateFactor));
            }
        }

        protected void HandlePerfectRelease()
        {
            var dmgPop = GameData.Misc.CreateDmgPopClone("PERFECT RELEASE!!", target.transform);
            dmgPop.Num.color = PERFECT_RELEASE_COLOR;

            if (caster.MySkills.isPlayer) UpdateSocialLog.CombatLogAdd("PERFECT RELEASE! Mana cost and cooldown not applied to spell! You cannot do this again whilst exhausted.", "green");

            Vessel.UseMana = false;

            var twinSpellSkill = GameData.SkillDatabase.GetSkillByID(SkillDB_Start.TWIN_SPELL_SKILL_ID);
            if (caster.MySkills.KnowsSkill(SkillDB_Start.PERFECT_RELEASE_2_SKILL_ID) && caster.MySkills.KnowsSkill(twinSpellSkill))
            {
                caster.GetCooldownManager().ResetCooldown(twinSpellSkill);
                if (caster.MySkills.isPlayer) UpdateSocialLog.CombatLogAdd("Twin Spell cooldown reset by Perfect Release!", "green");
            }

            caster.MyStats.AddStatusEffectNoChecks(exhaustionEffect, true, 0, caster);

            int refractionLevel = caster.MySkills.GetAscensionRank(SkillDB_Start.REFRACTION_ASCENSION_ID);
            if (refractionLevel > 0)
            {
                var exhaustionStatus = new List<StatusEffect>(caster.MyStats.StatusEffects).Find(se => se.Effect == exhaustionEffect);
                exhaustionStatus.Duration *= 1f - (refractionLevel * SkillDB_Start.REFRACTION_COOLDOWN_FACTOR);
            }
        }

        protected void UpdateDamageMulti()
        {
            if (state >= State.LOCKED) // LOCKED means damage has already been locked down, probably to 0 -- don't want to change it.
                return;

            bool applyTimeIsPower = true;

            switch (castBarState)
            {
                case CastBarState.NORMAL:
                    damageMulti = 1f;
                    applyTimeIsPower = false;
                    break;
                case CastBarState.OVER:
                    damageMulti = 1f + Mathf.Lerp(0f, EXTRA_DAMAGE_FACTOR, castBarStateFactor);
                    break;
                case CastBarState.BEYOND:
                    damageMulti = (1 + EXTRA_DAMAGE_FACTOR) * Mathf.Lerp(1f, 1 + BEYOND_CHANT_DAMAGE_FACTOR, castBarStateFactor);
                    break;

                case CastBarState.PERFECT: // lil cheeky recursion here, leveraging the relevant damage calc logic then swapping back to PERFECT
                    float origFactor = castBarStateFactor;
                    float origTimeSpentChanting = timeSpentChanting;
                    // PERFECT release does full Overcharge damage -- NOT including Beyond Chant. Beyond Chant is still the hardest hitter.
                    castBarStateFactor = 1f;
                    timeSpentChanting = overchantEndTime;
                    castBarState = CastBarState.OVER;
                    UpdateDamageMulti();

                    timeSpentChanting = origTimeSpentChanting;
                    castBarStateFactor = origFactor;
                    castBarState = CastBarState.PERFECT;
                    return; // <-- note return, not break.
            }

            if (knowsTimeIsPower1 && applyTimeIsPower)
            {
                float secondsPassed = timeSpentChanting / 60f; // usual 60 factor bullshit
                float timeIsPowerSeconds = secondsPassed - SECONDS_BEFORE_TIME_IS_POWER;
                if (timeIsPowerSeconds > 0)
                    damageMulti *= Mathf.Pow(TIME_IS_POWER_EXPONENT, timeIsPowerSeconds);
            }
        }

        protected void UpdateCooldownMulti()
        {
            if (state >= State.LOCKED) // LOCKED means cooldown has already been locked down
                return;

            cooldownMulti = 1f;
            switch (castBarState)
            {
                case CastBarState.NORMAL:
                    cooldownMulti = castBarStateFactor;
                    break;
                case CastBarState.PERFECT:
                    cooldownMulti = 0f;
                    break;
                case CastBarState.OVER:
                    if (!knowsTimeIsPower3)
                        cooldownMulti = 1 + Mathf.Lerp(0f, EXTRA_COOLDOWN_FACTOR, castBarStateFactor);
                    break;
                case CastBarState.BEYOND:
                    if (!knowsTimeIsPower3)
                        cooldownMulti = 1 + EXTRA_COOLDOWN_FACTOR;
                    break;
            }
        }

        public float GetSpellDamageMulti() // Called by spell release, this is more-or-less synonymous with a finish method
        {
            UpdateDamageMulti();

            switch (castBarState)
            {
                case CastBarState.OVER:
                case CastBarState.BEYOND:
                    caster.MyAudio.PlayOneShot(Main.sfxByName["overchant-release"], GameData.SFXVol * caster.MyAudio.volume * 0.8f);
                    break;

                case CastBarState.PERFECT:
                    HandlePerfectRelease();
                    break;

                case CastBarState.NORMAL:
                default:
                    HandleEarlyRelease();
                    break;
            }

            return this.damageMulti;
        }

        public float GetSpellCooldownMulti()
        {
            UpdateCooldownMulti();
            
            state = State.FINISHED; // Being finished means we're ready to destroy which will trigger skill cooldown.

            return cooldownMulti;
        }

        protected override float CalculateCooldownFactor() // Note that this entire behaviour is a SKILL primarily -- *this* method is for the skill's cooldown
        {
            if (castBarState == CastBarState.PERFECT) // With a perfect release, Control Chant gets no cooldown.
                return 0f;

            int refractionLevel = caster.MySkills.GetAscensionRank(SkillDB_Start.REFRACTION_ASCENSION_ID);
            return 1f - (refractionLevel * SkillDB_Start.REFRACTION_COOLDOWN_FACTOR);
        }

        protected void Backfire(int manaBurned)
        {
            int damage = Mathf.RoundToInt(Random.Range(1.5f, 3f) * manaBurned);
            var dmgPop = GameData.Misc.CreateDmgPopClone("BACKFIRE!!", caster.transform);
            dmgPop.Num.color = Color.red;
            if (caster.MySkills.isPlayer) UpdateSocialLog.CombatLogAdd($"BACKFIRE!! You fail to control the chaotic energy in time, and your own mana hurts you for {damage} damage!", "red");

            caster.DamageMe(damage, true, Vessel.spell.MyDamageType, caster, true, false);
            caster.MyAudio.PlayOneShot(Main.sfxByName["backfire"], GameData.SFXVol * caster.MyAudio.volume * 1f);
        }

        protected override bool IsInterrupted() {
            return Vessel == null || target == null || target.IsDead();
        }

        protected override bool ShouldApplyCooldownOnUse()
        {
            return false; // Never want to apply cooldown on 1st use, and beyond that -- cooldown is applied automatically as part of spell finishing anyway.
        }

        protected override bool IsFinished() => state == State.FINISHED;
    }
}
