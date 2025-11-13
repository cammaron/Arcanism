using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;


namespace Arcanism.Patches
{

	[HarmonyPatch(typeof(SpellDB), "Start")]
	public class SpellDB_Start
    {
		public const string HARDENED_SKIN_SPELL_ID = "7281576";
		public const string COMA_SPELL_ID = "6669884";
		public const string BRAXS_FURY_SPELL_ID = "81596368";
		public const string BRAXS_RAGE_SPELL_ID = "6047104";
		public const string INFERNIS_SPELL_ID = "59622570";
		public const string MAJOR_LIGHTNING_SPELL_ID = "24045294";
		public const string SHORT_STUN_SPELL_ID = "9000000";
		public const string JOLT_SPELL_ID = "27880688";
		public const string PARASITIC_TWIN_SPELL_ID = "9000001";
		public const string DESERT_COFFIN_SPELL_ID = "9000002";
		public const string EXHAUSTION_SPELL_ID = "9000003";
		public const string LUCK_OF_SOLUNA_1_SPELL_ID = "9000004";
		public const string LUCK_OF_SOLUNA_2_SPELL_ID = "9000005";
		public const string LUCK_OF_SOLUNA_3_SPELL_ID = "9000006";
		public const string LUCK_OF_SOLUNA_4_SPELL_ID = "9000007";
		public const string LUCK_OF_SOLUNA_5_SPELL_ID = "9000008";
		public const string SCORCHED_FURY_SPELL_ID = "9000009";
		public const string FUNERAL_PYRE_NAME_CHANGE = "Eldritch Warping";

		static bool isInitialised = false;
		public static bool IsInitialised()
		{
			return isInitialised;
		}

		static Spell UpdateTargetSpell(SpellDB db, string id, int manaCost, int damage, float castSeconds, float cooldownSeconds, float? duration = null, int? incorrectDamageInDescription = null)
        {
			var spell = UpdateSpell(db, id, manaCost, castSeconds, cooldownSeconds, duration);
			int damageTextToReplace = spell.TargetDamage;
			if (incorrectDamageInDescription.HasValue && spell.SpellDesc.Contains(incorrectDamageInDescription.Value.ToString()))  // a couple of spell listings have incorrect damage in their descriptions, so can't use their dmg value to work out what to replace
				damageTextToReplace = incorrectDamageInDescription.Value;
			UpdateDescValue(spell, damageTextToReplace, damage);
			spell.TargetDamage = damage;
			return spell;
		}

		static Spell UpdateShieldSpell(SpellDB db, string id, int manaCost, int shielding, float castSeconds, float cooldownSeconds, float duration)
		{
			var spell = UpdateSpell(db, id, manaCost, castSeconds, cooldownSeconds, duration);
			UpdateDescValue(spell, spell.ShieldingAmt, shielding);
			spell.ShieldingAmt = shielding;
			return spell;
		}

		static Spell UpdateControlSpell(SpellDB db, string id, int manaCost, int targetLevel)
        {
			var spell = UpdateSpell(db, id, manaCost);
			UpdateDescValue(spell, spell.MaxLevelTarget, targetLevel);
			spell.MaxLevelTarget = targetLevel;
			return spell;
        }

		static Spell UpdateSpell(SpellDB db, string id, int manaCost, float? castSeconds = null, float? cooldownSeconds = null, float? duration = null)
		{
			var spell = db.GetSpellByID(id);
			return UpdateSpell(spell, manaCost, castSeconds, cooldownSeconds, duration);
		}

		static Spell UpdateSpell(Spell spell, int manaCost, float? castSeconds = null, float? cooldownSeconds = null, float? duration = null)
		{
			// NB: "SpellDurationInTicks" -- AFAICT a tick is 5 seconds [update: seems to be 3? sure looked like it was iterating over 300ms deducting 60ms per second, which looks like 5 seconds per tick...]
			// No idea why Sleep shows a duration of 27s; in theory it must be 30 due to duration in ticks = 6... but I just tested with stopwatch, and it lasted 25!!
			// Spell charge time is measured in 60ths of a second. So 60 = 1 second, 90 = 1.5 seconds.
			// Cooldown is in straight up seconds (float)
			// Jolt and Concuss both trigger the "GEN - STUN" spell effect, which has a  duration of 2 ticks which in theory might be 10 seconds but I recorded and it was maybe 7 - 7.5.
			// Aha! If you apply a status effect in the middle of a tick, it loses some or all of that tick, because they're processed in 5 second blocks!

			// Jolt stun effect has "UnstableDuration=true" which means even though it doesn't outright break immediately on damage, there's still a 20% chance of it ending each time dmg is taken

			// Immolation hits about 8 times over its "24" duration. Somehow.

			spell.ManaCost = manaCost;
			if (castSeconds.HasValue) spell.SpellChargeTime = castSeconds.Value * 60;
			if (cooldownSeconds.HasValue) spell.Cooldown = cooldownSeconds.Value;
			if (duration.HasValue) spell.SpellDurationInTicks = Mathf.CeilToInt(duration.Value / 3f);

			spell.ResistModifier = 0; // Holy shit, I didn't even know this was a thing. THAT'S what was throwing off my damage balancing by arbitrarily returning different values for spells. They all have a hidden stat! Off you go, my friend

			Main.Log.LogInfo($"Spell updated: {spell.SpellName}");
			return spell;
		}

		static Spell WithScreenshake(Spell spell, float? amp = null, float? duration = null)
        {
			spell.ShakeAmp = amp.GetValueOrDefault(1f);
			spell.ShakeDur = duration.GetValueOrDefault(0.2f);
			return spell;
        }

		static void UpdateDescValue(Spell spell, System.Object oldValue, System.Object newValue)
        {
			spell.SpellDesc = spell.SpellDesc.Replace(oldValue.ToString(), newValue.ToString());
        }

		static bool Prefix(SpellDB __instance)
        {
			return !isInitialised;

			// I'm calling Start manually to get around a load order issue w/ Item Database which rears its head when Loot Rarity plugin is present. This is to prevent the organic Start from running a second time.
        }

		public static void Postfix(SpellDB __instance)
		{
			if (isInitialised) return;

			Main.Log.LogInfo("Overriding SpellDB with Arcanism balance tweaks.");
			List<Spell> spellsToAdd = new List<Spell>();

			// Single target damage
			__instance.GetSpellByID("27243775").ResistModifier = 0; // Magic Bolt just removing resist mod
			UpdateTargetSpell(__instance, "13758096", 15, 19, 0.5f, 2f); // Ice Bolt
			UpdateTargetSpell(__instance, "34664684", 70, 145, 3f, 6); // Brax's Touch 
			WithScreenshake(UpdateTargetSpell(__instance, "441842", 170, 510, 7, 24, null, 185)); // Ethereal Rending -- passing incorrect description param
			UpdateTargetSpell(__instance, BRAXS_RAGE_SPELL_ID, 280, 665, 3f, 6); // Brax's Rage 
			WithScreenshake(UpdateTargetSpell(__instance, "63468487", 300, 1550, 7.5f, 24f)); // Winter's Bite 
			UpdateTargetSpell(__instance, "5069675", 180, 360, 0.5f, 2); // Ice Shock 
			WithScreenshake(UpdateTargetSpell(__instance, "4343577", 600, 1920, 5f, 12)); // Ice Spear 
			UpdateTargetSpell(__instance, BRAXS_FURY_SPELL_ID, 900, 1300, 1, 3); // Brax's Fury 
			WithScreenshake(UpdateTargetSpell(__instance, "51152210", 1400, 2880, 3f, 7, null, 1600), 0.6f, 0.2f); // Aetherstorm -- passing incorrect description param
			WithScreenshake(UpdateTargetSpell(__instance, "48295394", 2000, 6000, 8.5f, 36), 1.75f, 0.35f); // Tenebris 

			UpdateTargetSpell(__instance, "57673295", 360, 1400, 1, 3); // Mithril Shards -- only used on weapons

			UpdateShieldSpell(__instance, HARDENED_SKIN_SPELL_ID, 80, 650, 0.5f, 300, 8); // Hardened Skin
			UpdateShieldSpell(__instance, "9867251", 600, 4000, 0.5f, 300, 13); // Magical Skin
			UpdateShieldSpell(__instance, "5379426", 1400, 12000, 0.5f, 300, 18); // Magical Skin II

			// Sleep
			UpdateTargetSpell(__instance, "61166248", 160, 0, 2.5f, 4, 27f); // Sleep
			var coma = UpdateTargetSpell(__instance, COMA_SPELL_ID, 500, 0, 1f, 60, 10); // Coma
			coma.UnstableDuration = true;
			coma.BreakOnDamage = false;
			coma.SpellDesc = "Send your target into a short but intense coma, from which they will have trouble waking even if hurt";

			// Quick stuns
			Spell baseStun = __instance.GetSpellByID("9692298");
			Spell shortStun = GameObject.Instantiate(baseStun);
			shortStun.Id = SHORT_STUN_SPELL_ID;
			shortStun.SpellDurationInTicks = 1; // Actually wanted 1.5 seconds, but without very hacky mods to the tick system... this will have to do for now. NB there *is* a short stun effect with 1 tick duration already in the spell DB but it isn't unstable, and this must be.
			
			spellsToAdd.Add(shortStun);
			UpdateTargetSpell(__instance, JOLT_SPELL_ID, 200, 200, 0.5f, 1).StatusEffectToApply = shortStun; // Jolt --  much shorter stun duration, but can be cast repeatedly
			UpdateTargetSpell(__instance, "16834290", 350, 500, 0.5f, 1).StatusEffectToApply = shortStun; // Concuss

			// DoTs
			UpdateTargetSpell(__instance, "10108096", 190, 72, 1.5f, 3, 30).Line = Spell.SpellLine.Arc_Element_DOT; // Burning Chains -- manually assigning Arc_Element_DOT lines to all these because vanilla, some are incorrectly assigned as Druid line so you can stack them.
			UpdateTargetSpell(__instance, "8513952", 400, 530, 1.5f, 3, 30).Line = Spell.SpellLine.Arc_Element_DOT; // Immolation
			var funeralPyre = UpdateTargetSpell(__instance, "41721290", 1200, 1400, 2.5f, 24, 21); // Funeral Pyre being replaced w/ Eldritch Warping
			funeralPyre.MovementSpeed = 0;
			funeralPyre.Haste = 0;
			funeralPyre.MyDamageType = GameData.DamageType.Void;
			funeralPyre.Line = Spell.SpellLine.Arc_Void_DOT;
			funeralPyre.SpellDesc = $"Unseen forces tear at the target's spirit, dealing ({funeralPyre.TargetDamage} / tick) damage over a short duration.";
			funeralPyre.StatusEffectMessageOnNPC = $"feels dark forces stretching apart the fabric of their soul!";
			funeralPyre.name = funeralPyre.SpellName = FUNERAL_PYRE_NAME_CHANGE;
			funeralPyre.SpellIcon = __instance.GetSpellByID("5998729").SpellIcon; // Druid's Theft of Life icon.
			var weightOfSea = __instance.GetSpellByID("7550883");
			funeralPyre.SpellChargeFXIndex = 94; // borrowed from um... Druid's Arterial Decay spell
			funeralPyre.SpellResolveFXIndex = 59;
			funeralPyre.ChargeSound = weightOfSea.ChargeSound;
			funeralPyre.CompleteSound = weightOfSea.CompleteSound;
			funeralPyre.color = new Color32(137, 16, 130, 255);
			UpdateTargetSpell(__instance, INFERNIS_SPELL_ID, 800, 1250, 1f, 2.5f, 30).Line = Spell.SpellLine.Arc_Element_DOT; // Infernis
			UpdateTargetSpell(__instance, "10644536", 0, 1800, 0, 0, 18); // Linfering Inferno

			var parasiticTwin = GameObject.Instantiate(funeralPyre); // Basically just copy Funeral Pyre since they're both void DoTs
			parasiticTwin.Id = PARASITIC_TWIN_SPELL_ID;
			parasiticTwin.SpellDesc = "A parasitic twin saps at the target's life force";
			parasiticTwin.StatusEffectMessageOnNPC = $"feels their life force being drained by a parasitic twin!";
			parasiticTwin.name = parasiticTwin.SpellName = "Parasitic Twin";
			parasiticTwin.Line = Spell.SpellLine.Global_Void_DOT;
			parasiticTwin.SpellDurationInTicks = 39 / 3;
			parasiticTwin.TargetDamage = 1; // The actual damage is calculated as a bonus and I'm setting this to > 0 just to make sure the code all works as it should -- see TwinSpell
			parasiticTwin.SpellIcon = __instance.GetSpellByID("10488989").SpellIcon; // Score! Found a ref to the icon I want. Druid's Soul Tap spell.
			spellsToAdd.Add(parasiticTwin);


			// The exhaustion debuff effect after a Perfect Release w/ Control Chant -- not a spell that can be cast.
			// NB " exhaustion from control." Not... "control your exhaustion" :|
			var controlExhaustion = GameObject.Instantiate(funeralPyre); 
			controlExhaustion.Id = EXHAUSTION_SPELL_ID;
			UpdateSpell(controlExhaustion, 0, 0, 0, 120);
			controlExhaustion.MovementSpeed = controlExhaustion.TargetDamage = (int) (controlExhaustion.Haste = 0f);
			controlExhaustion.SpellIcon = coma.SpellIcon;
			controlExhaustion.SpellDesc = "You are exhausted from using Perfect Release.";
			controlExhaustion.StatusEffectMessageOnNPC = $"is exhausted after going all out!";
			controlExhaustion.name = controlExhaustion.SpellName = "Perfect Release Exhaustion";
			controlExhaustion.Line = Spell.SpellLine.Generic; // it seems like statuses of Generic line can stack w/ each other -- works here, don't want some other random effect to prevent this applying, or for this to override another debuff from an enemy
			controlExhaustion.RequiredLevel = 100;
			spellsToAdd.Add(controlExhaustion);

			// Mind control
			UpdateControlSpell(__instance, "40924576", 200, 8); // Invasive Thoughts
			UpdateControlSpell(__instance, "23358166", 600, 14); // Intrusive Ideas
			UpdateControlSpell(__instance, "4865624", 1000, 24); // Call of the Void
			UpdateControlSpell(__instance, "39532889", 2000, 34); // Twisting Mind

			var desertCoffin = GameObject.Instantiate(GameData.SpellDatabase.GetSpellByID(MAJOR_LIGHTNING_SPELL_ID)); // Only used on an item -- not available as a scroll
			desertCoffin.Id = DESERT_COFFIN_SPELL_ID;
			desertCoffin.name = desertCoffin.SpellName = "Desert Coffin";
			desertCoffin.MyDamageType = GameData.DamageType.Physical;
			desertCoffin.TargetDamage = 2800;
			desertCoffin.Cooldown = 30;
			desertCoffin.ManaCost = 1000;
			desertCoffin.SpellChargeTime = 8f * 60f;
			desertCoffin.SpellIcon = __instance.GetSpellByID("9309339").SpellIcon; // Blessing of Brax icon. That'll do.
			desertCoffin.SpellDesc = $"Channel a portion of Brax's fury as he conjured a great whirlwind of sand to bury his own people. Deals {desertCoffin.TargetDamage} damage to the target and nearby enemies.";
			spellsToAdd.Add(desertCoffin);

			var scorchedFury = GameObject.Instantiate(__instance.GetSpellByID(BRAXS_FURY_SPELL_ID));
			scorchedFury.Id = SCORCHED_FURY_SPELL_ID;
			scorchedFury.SpellChargeTime = 2.5f * 60f;
			scorchedFury.Cooldown = 6;
			spellsToAdd.Add(scorchedFury);

			var meditativeTrance = UpdateSpell(__instance, "14737962", 0, 6, 90);
			meditativeTrance.PercentManaRestoration = 30;
			meditativeTrance.SpellDesc = $"Restores {meditativeTrance.PercentManaRestoration}% of your mana after a long cast time";

			var nourished = __instance.GetSpellByID("1735287");
			nourished.HP = 5; // Nerf regen bonuses from food because I'm buffing regen from stats
			nourished.Mana = 4;

			var manaCharge1 = __instance.GetSpellByID("49947232");
			manaCharge1.Mana = 14;

			var manaCharge2 = __instance.GetSpellByID("913744");
			manaCharge2.Mana = 40;

			var auraOfBrax1 = __instance.GetSpellByID("29302546");
			auraOfBrax1.Mana = 2;

			var auraOfBrax2 = __instance.GetSpellByID("20833458");
			auraOfBrax2.Mana = 5;

			var auraOfBrax3 = __instance.GetSpellByID("9382716");
			auraOfBrax3.Mana = 10;

			var auraOfBrax4 = __instance.GetSpellByID("1190816");
			auraOfBrax4.Mana = 32;

			var auraOfBrax5 = __instance.GetSpellByID("8447686");
			auraOfBrax5.Mana = 50;
			auraOfBrax5.Int = 35;
			auraOfBrax5.Cha = 35;

			spellsToAdd.Add(CreateLuckOfSolunaEffect(LUCK_OF_SOLUNA_1_SPELL_ID, 12, "I", nourished.ChargeSound, nourished.CompleteSound));
			spellsToAdd.Add(CreateLuckOfSolunaEffect(LUCK_OF_SOLUNA_2_SPELL_ID, 18, "II", nourished.ChargeSound, nourished.CompleteSound));
			spellsToAdd.Add(CreateLuckOfSolunaEffect(LUCK_OF_SOLUNA_3_SPELL_ID, 24, "III", nourished.ChargeSound, nourished.CompleteSound));
			spellsToAdd.Add(CreateLuckOfSolunaEffect(LUCK_OF_SOLUNA_4_SPELL_ID, 30, "IV", nourished.ChargeSound, nourished.CompleteSound));
			spellsToAdd.Add(CreateLuckOfSolunaEffect(LUCK_OF_SOLUNA_5_SPELL_ID, 100, "V", nourished.ChargeSound, nourished.CompleteSound));

			__instance.SpellDatabase = __instance.SpellDatabase.AddRangeToArray(spellsToAdd.ToArray());

			isInitialised = true;
		}

		static Spell CreateLuckOfSolunaEffect(string id, int spellLevel, string version, AudioClip chargeSound, AudioClip completeSound)
        {
			var luckOfSoluna = ScriptableObject.CreateInstance<Spell>();
			luckOfSoluna.Id = id;
			luckOfSoluna.name = luckOfSoluna.SpellName = $"Luck of Soluna {version}";
			luckOfSoluna.RequiredLevel = spellLevel;
			luckOfSoluna.SelfOnly = true;
			luckOfSoluna.CanHitPlayers = true;
			luckOfSoluna.Type = Spell.SpellType.Beneficial;
			luckOfSoluna.Line = Spell.SpellLine.Solunarian_Buff;
			luckOfSoluna.SpellResolveFXIndex = 10;
			luckOfSoluna.SpellChargeTime = 1;
			luckOfSoluna.SpellDurationInTicks = Mathf.CeilToInt((20 * 60) / 3f);
			luckOfSoluna.StatusEffectMessageOnNPC = "glows with the light of Soluna.";
			luckOfSoluna.StatusEffectMessageOnPlayer = "glow with the light of Soluna.";
			string levelLimitStr = spellLevel > 35 ? "of any Level" : $"below Level {spellLevel}";
			luckOfSoluna.SpellDesc = $"Triples the chance for items dropped by enemies {levelLimitStr} to be Blessed.";
			luckOfSoluna.ChargeSound = chargeSound;
			luckOfSoluna.CompleteSound = completeSound;
			luckOfSoluna.SpellIcon = Main.miscSpritesByName["LuckOfSoluna"];

			// Prevent NPEs
			luckOfSoluna.ChargeVariations = new List<AudioClip>();
			luckOfSoluna.CompleteVariations = new List<AudioClip>();
			luckOfSoluna.UsedBy = new List<Class>();

			return luckOfSoluna;
		}
	}
}