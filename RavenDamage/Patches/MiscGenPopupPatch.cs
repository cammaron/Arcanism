using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BigDamage.Patches
{
	[HarmonyPatch(typeof(Misc), nameof(Misc.GenPopup))]
	public class MiscGenPopupPatch
	{

		/*
		 By default, GenPopup will only show a DmgPop if it's less than 20 units from the player. This patch increases that range to 55.
		 */
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return new CodeMatcher(instructions)
				.MatchStartForward(
					CodeInstruction.Call(() => Vector3.Distance(default, default)),
					new CodeInstruction(OpCodes.Ldc_R4, 20f)
				)
				.ThrowIfInvalid("Unable to find Vector3.Distance comparison for DmgPop display")
				.Advance(1)
				.Set(OpCodes.Ldc_R4, Main.maxDistance.Value)
				//.RemoveInstruction()
				//.Insert(new CodeInstruction(OpCodes.Ldc_R4, Main.maxDistance))
				.Instructions();
		}

	}
}
