using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;


namespace Arcanism
{
	[HarmonyPatch(typeof(UseSkill), nameof(UseSkill.DoSkill))]
	public class UseSkillDoSkillPatch
	{
		static bool Prefix(UseSkill __instance, ref bool __result, Skill _skill, Character _target)
        {
			var traverse = Traverse.Create(__instance);
			Stats myStats = traverse.Field<Stats>("MyStats").Value;
			Character myself = myStats.Myself;
			CastSpell mySpells = myself?.MySpells;
			var vesselTraversal = Traverse.Create(mySpells).Field("CurrentVessel");
			SpellVessel vessel = vesselTraversal.GetValue<SpellVessel>();

			if (_skill.Id == SkillDBStartPatch.CONTROL_CHANT_SKILL_ID)
            {
				string failMsg = null;

				if (vessel.GetComponent<TwinSpell>() != null)
					failMsg = "Cannot be used while twinning a spell.";

				if (!vessel.UseMana)
					failMsg = "Can't be used with spells that don't channel mana.";

				if (failMsg != null)
                {
					__result = false;
					Traverse.Create(__instance).Field("passCheck").SetValue(false);
					UpdateSocialLog.LogAdd(failMsg, "yellow");
					return false;
				}
			}

			return true;
        }

		/* Implementation for the Twin Spell skill, and changes to Control Chant */
		static void Postfix(UseSkill __instance, ref bool __result, Skill _skill, Character _target)
		{
			var traverse = Traverse.Create(__instance);
			Stats myStats = traverse.Field<Stats>("MyStats").Value;
			Character myself = myStats.Myself;
			CastSpell mySpells = myself?.MySpells;
			var vesselTraversal = Traverse.Create(mySpells).Field("CurrentVessel");
			SpellVessel vessel = vesselTraversal.GetValue<SpellVessel>();
			bool isControllingChant = vesselTraversal.Field<bool>("ControlledChant").Value;

			var passCheckField = traverse.Field<bool>("passCheck"); // the method conveniently already stores the result of basic checks locally so I can just refer to that to ensure we're in a state ready to use an ability...

			if (passCheckField.Value && !__result && _skill.Id == SkillDBStartPatch.CONTROL_CHANT_SKILL_ID && vessel != null)
            {
				Main.Log.LogInfo("UseSkill Postfix: Using Control Chant.");
				if (vessel.GetComponent<ControlChant>() == null)
                {
					Main.Log.LogInfo("UseSkill Postfix: Adding ControlChant component to SpellVessel.");
					var controlChant = vessel.gameObject.AddComponent<ControlChant>();
					controlChant.Initialise(vessel, myself, mySpells, vesselTraversal.Field<float>("overChantLife"), vesselTraversal.Field<float>("overChantTotal"));
				} 
				return;
            }

			if (!passCheckField.Value || _skill.Id != SkillDBStartPatch.TWIN_SPELL_SKILL_ID || myself == null || mySpells == null || vessel == null)
				return;

			Stats existingCastTarget = vesselTraversal.Field<Stats>("targ").Value;

			TwinSpell twinSpell = vessel.GetComponent<TwinSpell>();

			if (!mySpells.isCasting())
            {
				__result = false;
				passCheckField.Value = false;
				UpdateSocialLog.LogAdd("You must be channeling a spell to use this skill", "yellow");
				return;
			}
			
			Spell currentCast = mySpells.GetCurrentCast();
			if (currentCast.Type != Spell.SpellType.Damage)
            {
				__result = false; 
				passCheckField.Value = false;
				UpdateSocialLog.LogAdd("This skill only works on DAMAGE spells", "yellow");
				return;
			}

			if (_target == null || _target == myself)
            {
				__result = false;
				passCheckField.Value = false;
				UpdateSocialLog.LogAdd("You can't hit yourself with this skill!", "yellow");
				return;
			}

			if (isControllingChant)
            {
				__result = false;
				passCheckField.Value = false;
				UpdateSocialLog.LogAdd("Cannot be used while controlling a chant.", "yellow");
				return;
			}

			if (_target == existingCastTarget.Myself || (twinSpell != null && twinSpell.IsTargeted(_target)))
            {
				__result = false;
				passCheckField.Value = false;
				UpdateSocialLog.LogAdd("Cannot be used on the same target twice.", "yellow");
				return;
			}

			bool success;
			if (twinSpell == null)
			{
				twinSpell = vessel.gameObject.AddComponent<TwinSpell>();
				twinSpell.Initialise(vessel, myself, __instance);
			}
			success = twinSpell.AddTarget(_target);
			if (!success)
            {
				if (twinSpell.TwinnedTargetCount == 0)
					GameObject.Destroy(twinSpell); // no point having the component on there if we weren't even able to add a single target

				__result = false;
				passCheckField.Value = false;
				return;
			}

			__result = twinSpell.TwinnedTargetCount >= twinSpell.MaxTwinTargets; // If we've hit the max twin count, apply cooldown immediately -- otherwise it'll be applied on spell end
		}
	}
}