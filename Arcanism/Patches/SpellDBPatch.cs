using UnityEngine;
using HarmonyLib;


namespace Arcanism
{
	[HarmonyPatch(typeof(SpellDB), "Start")]
	public class SpellDBStartPatch
    {
        public const string COMA_SPELL_ID = "6669884";
		public const string FUNERAL_PYRE_NAME_CHANGE = "Eldritch Warping";

        static Spell UpdateTargetSpell(SpellDB db, string id, int manaCost, int damage, float castTime, float cooldownTime, float? duration = null)
        {
			var spell = UpdateSpell(db, id, manaCost, castTime, cooldownTime, duration);
			UpdateDescValue(spell, spell.TargetDamage, damage);
			spell.TargetDamage = damage;
			return spell;
		}

		static Spell UpdateShieldSpell(SpellDB db, string id, int manaCost, int shielding, float castTime, float cooldownTime, float duration)
		{
			var spell = UpdateSpell(db, id, manaCost, castTime, cooldownTime, duration);
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

		static Spell UpdateSpell(SpellDB db, string id, int manaCost, float? castTime = null, float? cooldownTime = null, float? duration = null)
		{
			// NB: "SpellDurationInTicks" -- AFAICT a tick is 5 seconds. No idea why Sleep shows a duration of 27s; in theory it must be 30 due to duration in ticks = 6... but I just tested with stopwatch, and it lasted 25!!
			// Spell charge time is measured in 60ths of a second. So 60 = 1 second, 90 = 1.5 seconds.
			// Cooldown is in straight up seconds (float)
			// Jolt and Concuss both trigger the "GEN - STUN" spell effect, which has a  duration of 2 ticks which in theory might be 10 seconds but I recorded and it was maybe 7 - 7.5.
			// Aha! If you apply a status effect in the middle of a tick, it loses some or all of that tick, because they're processed in 5 second blocks!

			// Jolt stun effect has "UnstableDuration=true" which means even though it doesn't outright break immediately on damage, there's still a 20% chance of it ending each time dmg is taken

			// Immolation hits about 8 times over its "24" duration. Somehow.

			var spell = db.GetSpellByID(id);
			spell.ManaCost = manaCost;
			if (castTime.HasValue) spell.SpellChargeTime = castTime.Value * 60;
			if (cooldownTime.HasValue) spell.Cooldown = cooldownTime.Value;
			if (duration.HasValue) spell.SpellDurationInTicks = Mathf.CeilToInt(duration.Value / 5f);

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

		static void Postfix(SpellDB __instance)
		{
			Main.Log.LogInfo("Overriding SpellDB with Arcanism balance tweaks.");
			// Single target damage
			UpdateTargetSpell(__instance, "13758096", 15, 22, 0.5f, 2f); // Ice Bolt
			UpdateTargetSpell(__instance, "34664684", 70, 140, 3f, 6); // Brax's Touch 
			var ethRending = WithScreenshake(UpdateTargetSpell(__instance, "441842", 170, 600, 7, 24)); // Ethereal Rending
			if (ethRending.SpellDesc.Contains("185")) UpdateDescValue(ethRending, "185", ethRending.TargetDamage.ToString()); // manual fix for Ethereal Rending -- replacing the old damage string with new doesn't work at present because the old description is incorrect (lists damage as 185 instead of 280)
			UpdateTargetSpell(__instance, "6047104", 280, 620, 3f, 6); // Brax's Rage 
			WithScreenshake(UpdateTargetSpell(__instance, "63468487", 300, 1050, 7.5f, 24f)); // Winter's Bite 
			UpdateTargetSpell(__instance, "5069675", 160, 280, 0.5f, 2); // Ice Shock 
			WithScreenshake(UpdateTargetSpell(__instance, "4343577", 430, 1300, 5.5f, 15)); // Ice Spear 
			UpdateTargetSpell(__instance, "81596368", 360, 1100, 1, 3); // Brax's Fury 
			WithScreenshake(UpdateTargetSpell(__instance, "51152210", 1100, 2000, 4f, 7), 0.6f, 0.2f); // Aetherstorm 
			WithScreenshake(UpdateTargetSpell(__instance, "48295394", 2000, 6000, 7.5f, 24), 1.75f, 0.35f); // Tenebris 

			UpdateShieldSpell(__instance, "7281576", 30, 300, 0.5f, 120, 12); // Hardened Skin
			UpdateShieldSpell(__instance, "9867251", 350, 1500, 0.5f, 200, 12); // Magical Skin
			UpdateShieldSpell(__instance, "7281576", 700, 6000, 0.5f, 200, 12); // Magical Skin II

			// Sleep
			UpdateTargetSpell(__instance, "61166248", 160, 0, 2.5f, 4, 27f); // Sleep
			var coma = UpdateTargetSpell(__instance, COMA_SPELL_ID, 500, 0, 1f, 60, 10); // Coma
			coma.UnstableDuration = true;
			coma.BreakOnDamage = false;
			coma.SpellDesc = "Send your target into a short but intense coma, from which they will have trouble waking even if hurt";

			// Quick stuns
			Spell baseStun = __instance.GetSpellByID("9692298");
			Spell shortStun = GameObject.Instantiate(baseStun);
			shortStun.SpellDurationInTicks = 1; // Actually wanted 1.5 seconds, but without very hacky mods to the tick system... this will have to do for now. NB there *is* a short stun effect with 1 tick duration already in the spell DB but it isn't unstable, and this must be.
			UpdateTargetSpell(__instance, "27880688", 200, 160, 0.5f, 1).StatusEffectToApply = shortStun; // Jolt --  much shorter stun duration, but can be cast repeatedly
			UpdateTargetSpell(__instance, "16834290", 350, 400, 0.5f, 1).StatusEffectToApply = shortStun; // Concuss

			// DoTs
			UpdateTargetSpell(__instance, "10108096", 190, 65, 1.5f, 3, 30); // Burning Chains 
			UpdateTargetSpell(__instance, "8513952", 400, 400, 1.5f, 3, 30); // Immolation
			var funeralPyre = UpdateTargetSpell(__instance, "41721290", 900, 1000, 2.5f, 12, 18); // Funeral Pyre being replaced w/ Eldritch Warping
			funeralPyre.MovementSpeed = 0;
			funeralPyre.Haste = 0;
			funeralPyre.MyDamageType = GameData.DamageType.Void;
			funeralPyre.Line = Spell.SpellLine.Arc_Void_DOT;
			funeralPyre.SpellDesc = $"Unseen forces tear at the target's spirit, dealing ({funeralPyre.TargetDamage} / tick) damage over a short duration.";
			funeralPyre.name = funeralPyre.SpellName = FUNERAL_PYRE_NAME_CHANGE;
			var weightOfSea = __instance.GetSpellByID("7550883");
			funeralPyre.SpellChargeFXIndex = 94; // borrowed from um... Druid's Arterial Decay spell
			funeralPyre.SpellResolveFXIndex = 59;
			funeralPyre.ChargeSound = weightOfSea.ChargeSound;
			funeralPyre.CompleteSound = weightOfSea.CompleteSound;
			funeralPyre.color = new Color32(137, 16, 130, 255);
			UpdateTargetSpell(__instance, "59622570", 1400, 1400, 1.5f, 3, 30); // Infernis
			UpdateTargetSpell(__instance, "10644536", 0, 1800, 0, 0, 18); // Linfering Inferno

			// Mind control
			UpdateControlSpell(__instance, "40924576", 200, 8); // Invasive Thoughts
			UpdateControlSpell(__instance, "23358166", 600, 14); // Intrusive Ideas
			UpdateControlSpell(__instance, "4865624", 1300, 24); // Call of the Void
			UpdateControlSpell(__instance, "39532889", 3000, 34); // Twisting Mind
		}
	}
}