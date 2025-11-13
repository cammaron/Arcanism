using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(Stats), "Start")]
	public class Stats_Start
    {
		public static void Postfix(Stats __instance)
        {
			var shield = __instance.gameObject.AddComponent<SpellShieldVisual>();
			shield.stats = __instance;
        }
    }

	[HarmonyPatch(typeof(Stats), nameof(Stats.RemoveBreakableEffects))]
	public class Stats_RemoveBreakableEffects
	{
		static bool DoesDamageInterruptStatusEffect(StatusEffect effect)
        {
			int interruptChance = 20;
			if (effect.Effect.Id == SpellDB_Start.COMA_SPELL_ID) interruptChance = 10;
			return (Random.Range(0, 100) < interruptChance);

		}
		/* This patch is just about making Coma a more effective spell, so that its sleep effect can still be interrupted by damage but is less likely to be than a standard "unstable duration" effect. */
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{

			/*ldc.i4.0
			ldc.i4.s  10
			call int32[UnityEngine.CoreModule]UnityEngine.Random::Range(int32, int32)*/

			var matcher = new CodeMatcher(instructions)
				.MatchStartForward(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(int), typeof(int) }))) // Random.Range(0, 10) < 2
				.Advance(2); // should put us on the boolean operand
			var randomGreaterEqual = matcher.Instruction;
			if (randomGreaterEqual == null)
				throw new System.InvalidProgramException("Unable to locate the Random.Range call to interrupt breakable status efffects -- expected a bge opcode 2 lines after Random.Range but result is null.");
			if (randomGreaterEqual.opcode != OpCodes.Bge && randomGreaterEqual.opcode != OpCodes.Bge_S)
				throw new System.InvalidProgramException("Unable to locate the Random.Range call to interrupt breakable status efffects -- expected a bge opcode 2 lines after Random.Range but result got " + randomGreaterEqual.opcode + "  " + randomGreaterEqual.operand);

			var goToOnFalse = randomGreaterEqual.operand;
			
			return matcher
				.Advance(-4) // Go to the beginning of the block and remove the whole thing: 2 arg declarations for Random.Range, the call, the comparison value and the boolean op...
				.RemoveInstructions(5) 
				// Now, replace with my own condition block, calling a method defined above and checking if it's true.
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Stats), "StatusEffects")))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem_Ref)) 
				.InsertAndAdvance(CodeInstruction.Call(() => DoesDamageInterruptStatusEffect(default)))
				//.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatsRemoveBreakableEffectsPatch), nameof(DoesDamageInterruptStatusEffect))))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, goToOnFalse))
				.Instructions();
		}
	}

	/* Prevent 'meditative state' message showing, because boosted regen will now only occur when sitting -- see RegenEffects patch below */
	[HarmonyPatch(typeof(Stats), "Update")]
	public class Stats_Update
	{
		public const float RECENT_DAMAGE_HACK = 0.1f; // medMsg gets reset to false if RecentDamage is 0, so in order to keep hiding that meditative state msg, never letting it go below 0.1 (and treating 0.1 as 0 in my regen code)
		public static void Prefix(ref bool ___medMsg, ref float ___RecentDmg)
        {
			___RecentDmg = RECENT_DAMAGE_HACK;
			___medMsg = true;  // to prevent meditative state msg ever showing as I'm controlling this logic myself now
        }
	}

	/* Arcanism has a heavy focus on an altered mana economy. The aim is to greatly slow down the amplified regen between fights, leaving it still useful but not near-instantaneous. 
	 * Previously patched the IL code to pass a different multi into RegenEffects, which was fragile, so now literally just ignoring the argument and deciding the appropriate multi
	 * here. 
	 * 
	 * The goal is to make regen slightly more stat dependent (so that End and Wis amplify base regen more), and now that the game features Sitting, I'll change it to only boost your
	 * regen speed whilst sitting rather than any time you've been out of combat. Aside from immersion, this fixes an issue in which you could regen mana *while casting*, which actually
	 * hugely offset the mana costs of Control Chant when doing very long casts (sometimes regenerating more mana than you're spending). */
	[HarmonyPatch(typeof(Stats), "RegenEffects")]
	public class Stats_RegenEffects
	{
		public const float RESTING_REGEN_MULTI = 3.5f;

		protected static bool AlreadyResting = false;

		public static int GetHealthRegen(Stats stats)
		{
			return Mathf.RoundToInt(stats.Level + (2 * stats.EndScaleMod) / 100f * stats.GetCurrentEnd());
		}

		public static int GetManaRegen(Stats stats)
		{
			return Mathf.RoundToInt(((stats.Level * 0.5f) + (2 * stats.WisScaleMod) / 100f * stats.GetCurrentWis()));
		}

		public static float GetCurrentRegenMulti(Stats stats)
        {
			if ((stats.Myself.MySpells?.isCasting()).GetValueOrDefault(false)) return 0f;  // No regen during cast
			
			bool isPlayer = (stats.Myself.MySkills?.isPlayer).GetValueOrDefault(false);
			bool isSitting = (stats.Myself.MyNPC?.ThisSim?.sitting).GetValueOrDefault(false) || (isPlayer && GameData.PlayerControl.Sitting);

			if (isSitting && stats.RecentCast <= 0f && stats.RecentDmg <= Stats_Update.RECENT_DAMAGE_HACK)
			{
				if (isPlayer && !AlreadyResting)
                {
					UpdateSocialLog.LogAdd($"You feel yourself relaxing (natural regeneration improved by {Mathf.RoundToInt(RESTING_REGEN_MULTI * 100f)}%)", "lightblue");
					AlreadyResting = true;
					GameData.PlayerInv.PlayerStatDisp.UpdateDisplayStats();
				}
				return RESTING_REGEN_MULTI;
			}

			if (isPlayer && AlreadyResting)
			{
				UpdateSocialLog.LogAdd("You finish resting.", "lightblue");
				AlreadyResting = false;
				GameData.PlayerInv.PlayerStatDisp.UpdateDisplayStats();
			}
			return 1f;
		}

		public static bool Prefix(Stats __instance, float _mod, ref int ___CurrentHP, int ___CurrentMaxHP, ref int ___CurrentMana, int ___CurrentMaxMana, float ___TickTime)
		{
			float regenMulti = GetCurrentRegenMulti(__instance);
			
			if (___CurrentHP < ___CurrentMaxHP) 
				___CurrentHP = Mathf.Min(___CurrentHP + Mathf.RoundToInt(GetHealthRegen(__instance) * regenMulti), ___CurrentMaxHP);

			if (___CurrentMana < ___CurrentMaxMana) 
				___CurrentMana = Mathf.Min(___CurrentMana + Mathf.RoundToInt(GetManaRegen(__instance) * regenMulti), ___CurrentMaxMana);
			return false;
		}
	}

	/* Updating the 2 Get Regen methods--which are just used for stat display purposes -- to reduce code duplication and use the above logic to determine current regen. 
	 * Notably, compared with vanilla, this method will also include the current regen multi, so you can see your effective regen when sitting. */
	[HarmonyPatch(typeof(Stats), nameof(Stats.GetCurrentHPRegen))]
	public class Stats_GetCurrentHPRegen
	{
		static bool Prefix(Stats __instance, ref int __result, int ___seRegen)
		{
			// NB "seRegen" prop on Stats appears unused in vanilla code. I'm adding it in here in case it ever gets assigned to, but I guess that means there's currently no status effects giving health regen.
			__result = ___seRegen + Mathf.RoundToInt(Stats_RegenEffects.GetHealthRegen(__instance) * Stats_RegenEffects.GetCurrentRegenMulti(__instance));
			return false;
		}
	}

	/* Remove a lil code duplication here, and ensure this display-only method is using the new formula 
	 * (NB to future me: *this* method includes seManaRegen i.e. status effects like food, but the RegenEffects method by default does not. 
	 * That's because THIS method is used for the stats display, whereas mechanically, standard regen ticks and the bonus from the status effect are separate things
	 * (y'know, the HP/MP regen from status happens in TickEffects)
	 */
	[HarmonyPatch(typeof(Stats), nameof(Stats.GetCurrentMPRegen))]
	public class Stats_GetCurrentMPRegen
	{
		static bool Prefix(Stats __instance, ref int __result, int ___seManaRegen)
		{
			// NB removed a call to CalcStats here that seemed like it would be very inefficient now that this method is being used on regen ticks. Seems like it must surely have been redundant anyway, but leaving this comment in case...
			__result = ___seManaRegen + Mathf.RoundToInt(Stats_RegenEffects.GetManaRegen(__instance) * Stats_RegenEffects.GetCurrentRegenMulti(__instance));
			return false;
		}
	}


}