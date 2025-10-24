using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;


namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(UseSkill), nameof(UseSkill.DoSkill))]
	public class UseSkill_DoSkill
	{
		/* Detecting and testing against any existant ExtendedSkill's usage criteria. If an ExtendedSkill exists for the skill being used,
		 * it will be added as a component to the Character using the skill during this method. */
		static bool Prefix(UseSkill __instance, ref bool __result, ref bool ___passCheck, Stats ___MyStats, Skill _skill, Character _target)
        {
			Character myself = ___MyStats.Myself;
			CastSpell mySpells = myself?.MySpells;
			if (__instance.isPlayer) Main.Log.LogInfo("UseSkill: Prefix for " + _skill.SkillName + ". Cooldown for skill is: " + myself.GetCooldownManager().GetCooldown(_skill));
			if (myself.GetCooldownManager().GetCooldown(_skill) > 0) return false; // Previously, whether a skill could be used was solely the domain of the hotkey itself. 

			SpellVessel vessel = null;

			if (myself != null && mySpells != null)
				vessel = Traverse.Create(mySpells).Field<SpellVessel>("CurrentVessel").Value;

			if (ExtensionManager.GetExtension(_skill, myself, vessel, out var extension)) { // If there's an extension, it'll completely override the default DoSkill stuff
				if (!extension.UseSkill(_target, out string failureReason))
                {
					___passCheck = false;
					UpdateSocialLog.LogAdd(failureReason, "yellow");
				}
				
				__result = false; // Returning true ultimately just sets the hotkey cooldown. We don't need that.
				return false;
			}

			return true;
        }

		static void Postfix(UseSkill __instance, ref bool __result, Skill _skill, Character _target, ref bool ___passCheck, Stats ___MyStats)
		{
			Character myself = ___MyStats.Myself;

			if (___passCheck && myself != null && __result)
				myself.GetCooldownManager().AddCooldown(_skill);
		}
	}
}