using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Arcanism.Patches
{
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

	/* Decrease the modifier for HP/Mana regeneration when in a "meditative state" */
	[HarmonyPatch(typeof(Stats), "Update")]
	public class Stats_Update
	{
		public const float MEDITATE_STATE_REGEN_FACTOR = 2.5f;
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return new CodeMatcher(instructions)
				.MatchStartForward(new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Stats), "RegenEffects")))
				.Advance(-1)
				.ThrowIfNotMatch("Unable to find float regen multiplier argument to be passed to RegenEffects method.", new CodeInstruction(OpCodes.Ldc_R4))
				.RemoveInstruction()
				.Insert(new CodeInstruction(OpCodes.Ldc_R4, MEDITATE_STATE_REGEN_FACTOR)
			).Instructions();
		}
	}

	/* Need to change the other regen methods anyway to make regen more stat dependent. Rather than change logic in 2 places, seems better to update this method (which previously duplicated the logic) */
	[HarmonyPatch(typeof(Stats), "RegenEffects")]
	public class Stats_RegenEffects
	{
		public static int GetHealthRegen(Stats stats)
		{
			return Mathf.RoundToInt(stats.Level + (2 * stats.EndScaleMod) / 100f * stats.GetCurrentEnd());
		}

		public static int GetManaRegen(Stats stats)
		{
			return Mathf.RoundToInt(((stats.Level * 0.5f) + (2 * stats.WisScaleMod) / 100f * stats.GetCurrentWis()));
		}

		static bool Prefix(Stats __instance, float _mod, ref int ___CurrentHP, int ___CurrentMaxHP, ref int ___CurrentMana, int ___CurrentMaxMana)
		{
			if (___CurrentHP < ___CurrentMaxHP) 
				___CurrentHP = Mathf.Min(___CurrentHP + Mathf.RoundToInt(GetHealthRegen(__instance) * _mod), ___CurrentMaxHP);

			if (___CurrentMana < ___CurrentMaxMana) 
				___CurrentMana = Mathf.Min(___CurrentMana + Mathf.RoundToInt(GetManaRegen(__instance) * _mod), ___CurrentMaxMana);
			return false;
		}
	}

	/* With regen in general being significantly nerfed, */
	[HarmonyPatch(typeof(Stats), nameof(Stats.GetCurrentHPRegen))]
	public class Stats_GetCurrentHPRegen
	{
		static bool Prefix(Stats __instance, ref int __result)
		{
			__result = Stats_RegenEffects.GetHealthRegen(__instance);
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
			__result = ___seManaRegen + Stats_RegenEffects.GetManaRegen(__instance);
			return false;
		}
	}


}