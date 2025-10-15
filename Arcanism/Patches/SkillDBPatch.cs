using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;


namespace Arcanism
{
	[HarmonyPatch(typeof(SkillDB), "Start")]
	public class SkillDBStartPatch
    {
		public const string EXPERT_CONTROL_SKILL_ID = "90000000";
		public const string EXPERT_CONTROL_2_SKILL_ID = "90000001";
		public const string TWIN_SPELL_SKILL_ID = "90000002";
		public const string TWIN_SPELL_MASTERY_SKILL_ID = "90000003";
		public const string CONTROL_CHANT_SKILL_ID = "10823930";

		static bool isInitialised = false;
		public static bool IsInitialised()
        {
			return isInitialised;
        }

		static void Postfix(SkillDB __instance)
		{
			Main.Log.LogInfo("Overriding SkillDB with Arcanism balance tweaks.");
			
			var controlChant = __instance.GetSkillByID(CONTROL_CHANT_SKILL_ID);
			controlChant.Cooldown = 8f * 60f; // 60ths of seconds
			controlChant.SkillDesc = "Activate any time during the channeling of a DAMAGE SPELL to control your chant. Activate again to immediately end the spell. The earlier you release your spell, the less mana and cooldown time, but the greater the risk of losing control. Conversely, chanting for LONGER than normal greatly increases the mana cost and cooldown, but also the spell's power.";
			Main.Log.LogInfo($"Skill updated: {controlChant.SkillName}");

			List<Skill> skillsToAdd = new List<Skill>();
			skillsToAdd.Add(CreateSkill(controlChant, EXPERT_CONTROL_SKILL_ID, "Expert Control I", 13, Skill.SkillType.Innate, 
				"Having achieved new levels of the meditative trance-like state entered during chant control, your chance of an early release spell backfiring is reduced, and backfires deal half normal spell damage to the enemy."));

			skillsToAdd.Add(CreateSkill(controlChant, EXPERT_CONTROL_2_SKILL_ID, "Expert Control II", 29, Skill.SkillType.Innate,
				"Your very consciousness dissolves and mingles with the aether when chanting, reflecting your sheer mastery of weaving magic. Your early release spells are even less likely to backfire, and backfires deal full normal spell damage to the enemy."));

			controlChant.Cooldown = 1f * 60f; // TODO FIXME TMP REMOVE ME DEBUG LINE
			skillsToAdd.Add(CreateSkill(controlChant, TWIN_SPELL_SKILL_ID, "Twin Spell", 10, Skill.SkillType.Other,
				"While channeling a DAMAGE SPELL, target another enemy and activate this skill to split your mind in two and duplicate your channeling, hitting both targets with the full power of the spell -- but at more cost than simply casting twice!", 1f/*120f*/));

			skillsToAdd.Add(CreateSkill(controlChant, TWIN_SPELL_MASTERY_SKILL_ID, "Twin Spell Mastery", 25, Skill.SkillType.Innate,
				"Beginning to wield the boundless powers of an archmage, you are able to split your mind a second time, targeting a 3rd enemy with Twin Spell -- but with even greater mana cost."));

			foreach(var skill in skillsToAdd)
            {
				if (__instance.GetSkillByID(skill.Id) != null)
					throw new System.Exception($"Unable to add skill {skill.SkillName} as skill with ID {skill.Id} already exists: {__instance.GetSkillByID(skill.Id).SkillName}");
				Main.Log.LogInfo($"Skill to be added: {skill.SkillName}");
			}

			__instance.SkillDatabase = __instance.SkillDatabase.AddRangeToArray(skillsToAdd.ToArray());
			Main.Log.LogInfo($"Added {skillsToAdd.Count} skills.");

			isInitialised = true;
		}

		private static Skill CreateSkill(Skill baseSkill, string id, string name, int lvlRequired, Skill.SkillType skillType, string desc, float? cooldownSeconds = null)
		{
			var skill = GameObject.Instantiate(baseSkill);
			skill.Id = id;
			skill.name = skill.SkillName = name;
			skill.ArcanistRequiredLevel = lvlRequired;
			skill.TypeOfSkill = skillType;
			skill.SkillDesc = desc;
			skill.Cooldown = cooldownSeconds.GetValueOrDefault(0) * 60f;
			return skill;
		}
	}
}