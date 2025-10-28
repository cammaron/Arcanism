using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using Arcanism.Skills;

namespace Arcanism.Patches
{

	[HarmonyPatch(typeof(SkillDB), "Start")]
	public class SkillDBStartPatch
    {
		public const string CONTROL_CHANT_SKILL_ID = "10823930"; 
		public const string EXPERT_CONTROL_SKILL_ID = "90000000";
		public const string EXPERT_CONTROL_2_SKILL_ID = "90000001";
		public const string TWIN_SPELL_SKILL_ID = "90000002";
		public const string TWIN_SPELL_MASTERY_SKILL_ID = "90000003";
		public const string PERFECT_RELEASE_SKILL_ID = "90000004";
		public const string PERFECT_RELEASE_2_SKILL_ID = "90000005";
		public const string VANISHING_TWIN_SKILL_ID = "90000006";
		public const string SIBLING_SYNGERY_SKILL_ID = "90000007";
		public const string PARASITIC_TWIN_SKILL_ID = "90000008";
		public const string TWIN_SPELL_MASTERY_2_SKILL_ID = "90000009";


		public const string COOLDOWN_REDUCTION_ASCENSION_ID = "7758218";
		public const string MIND_SPLIT_ASCENSION_ID = "90000000";
		public const string REFRACTION_ASCENSION_ID = "90000001";


		public const float REFRACTION_COOLDOWN_FACTOR = 0.15f;
		public const float MIND_SPLIT_COST_FACTOR = 0.1f;
		public const float SIBLING_SYNERGY_POWER = 0.1f;

		static bool isInitialised = false;
		public static bool IsInitialised()
        {
			return isInitialised;
        }

		static void Postfix(SkillDB __instance)
		{
			Main.Log.LogInfo("SkillDB start patch registered; brute forcing SpellDB startup first.");
			
			Traverse.Create(GameObject.FindObjectOfType<SpellDB>()).Method("Start").GetValue(); 

			Main.Log.LogInfo("*********");
			Main.Log.LogInfo("Continuing with Overriding SkillDB with Arcanism balance tweaks.");

			

			AddSkills(__instance);
			AddAscensions(__instance);

			isInitialised = true;
		}

		static void AddSkills(SkillDB __instance)
        {
			var controlChant = __instance.GetSkillByID(CONTROL_CHANT_SKILL_ID);
			controlChant.Cooldown = 10f * 60f; // 60ths of seconds :/
			controlChant.SkillDesc = "Activate any time during the channeling of a DAMAGE SPELL to control your chant. Activate again to immediately end the spell. The earlier you release your spell, the less mana and cooldown time, but the greater the risk of losing control. Conversely, chanting for LONGER than normal greatly increases the mana cost and cooldown, but also the spell's power.";
			ControlChant.CreateExtension(controlChant); 
			Main.Log.LogInfo($"Skill updated: {controlChant.SkillName}");

			List<Skill> skillsToAdd = new List<Skill>();
			skillsToAdd.Add(CreateSkill(controlChant, EXPERT_CONTROL_SKILL_ID, "Expert Control I", 9, Skill.SkillType.Innate,
				"Having achieved new levels of the meditative trance-like state entered during chant control, your chance of an early release spell backfiring is reduced, and backfires deal half normal spell damage to the enemy."));

			skillsToAdd.Add(CreateSkill(controlChant, EXPERT_CONTROL_2_SKILL_ID, "Expert Control II", 17, Skill.SkillType.Innate,
				"Your very consciousness dissolves and mingles with the aether when chanting, reflecting your growing skill in weaving magic. Your early release spells are even less likely to backfire, and backfires deal full normal spell damage to the enemy."));

			skillsToAdd.Add(CreateSkill(controlChant, PERFECT_RELEASE_SKILL_ID, "Perfect Release I", 14, Skill.SkillType.Innate,
				"By releasing your spell with perfect timing around the standard cast time, you find you can reap the benefits of both worlds: the full damage of an over-chant, with zero cooldown or mana cost like an early release. The concentration required exhausts you, so this can only be done every 120 seconds."));

			skillsToAdd.Add(CreateSkill(controlChant, PERFECT_RELEASE_2_SKILL_ID, "Perfect Release II", 32, Skill.SkillType.Innate,
				"While a perfect release exhausts your mind, the moment of extreme clarity also gives you an adrenaline rush, completely resetting the cooldown for the Twin Spell skill."));

			var twinSpell = CreateSkill(controlChant, TWIN_SPELL_SKILL_ID, "Twin Spell", 10, Skill.SkillType.Other,
				"While channeling a DAMAGE SPELL, target another enemy and activate this skill to split your mind in two and duplicate your channeling, hitting both targets with the full power of the spell -- but at more cost than simply casting twice!", 120f);
			skillsToAdd.Add(twinSpell);
			TwinSpell.CreateExtension(twinSpell);

			skillsToAdd.Add(CreateSkill(controlChant, TWIN_SPELL_MASTERY_SKILL_ID, "Twin Spell Mastery I", 20, Skill.SkillType.Innate,
				"Magic is beginning to feel as natural as breathing to you, and while it's a strain, your trained mind can split a second time while casting, allowing you to twin your spells to a 3rd target -- albeit with greater mana cost."));

			skillsToAdd.Add(CreateSkill(controlChant, TWIN_SPELL_MASTERY_2_SKILL_ID, "Twin Spell Mastery II", 27, Skill.SkillType.Innate,
				"You're beginning to wield the boundless powers of an archmage, and splitting your mind a third time no longer seems the impossibility it once was. You may target a 4th enemy with Twin Spell, but the mana cost is ever greater."));

			skillsToAdd.Add(CreateSkill(controlChant, VANISHING_TWIN_SKILL_ID, "Vanishing Twin", 15, Skill.SkillType.Innate,
				"If an enemy targeted by a spell twin dies before you finish casting, where does that energy go? You've found a way to redirect it, so that rather than being wasted, it now hits the first remaining twin target, crippling them with the full force of each spell."));

			skillsToAdd.Add(CreateSkill(controlChant, SIBLING_SYNGERY_SKILL_ID, "Sibling Synergy", 18, Skill.SkillType.Innate,
				$"You suddenly perceive the hidden currents of force arcing between your connected Twin Spell targets. They conduct magic more efficiently, and you find that by channeling your spells through them, the damage increases by {SIBLING_SYNERGY_POWER * 100}% per target of your spell. The more there are, the harder they fall."));

			skillsToAdd.Add(CreateSkill(controlChant, PARASITIC_TWIN_SKILL_ID, "Parasitic Twin", 23, Skill.SkillType.Innate,
				$"Ever in search of greater efficiency, you learn that you can leverage the residual energy of your Twinned Spells when targets are killed -- after all, they're all still connected. You wrap that magic along the connective currents and then around each remaining target, covering them in lingering death magic, dealing damage over time based on your Intelligence."));

			foreach (var skill in skillsToAdd)
			{
				if (__instance.GetSkillByID(skill.Id) != null)
					throw new Exception($"Unable to add skill {skill.SkillName} as skill with ID {skill.Id} already exists: {__instance.GetSkillByID(skill.Id).SkillName}");
				Main.Log.LogInfo($"Skill to be added: {skill.SkillName}");
			}

			__instance.SkillDatabase = __instance.SkillDatabase.AddRangeToArray(skillsToAdd.ToArray());
			RefreshSprites(__instance);
			Main.Log.LogInfo($"Added {skillsToAdd.Count} skills.");
		}

		public static void RefreshSprites(SkillDB skillDb = null)
		{
			if (skillDb == null) skillDb = GameData.SkillDatabase;
			
			foreach (var entry in Main.skillSpriteById)
			{
				skillDb.GetSkillByID(entry.Key).SkillIcon = entry.Value;
			}
			Main.Log.LogInfo($"Updated graphics for {Main.itemSpriteById.Count} items");
		}

		static void AddAscensions(SkillDB __instance)
		{
			var baseAsc = __instance.GetAscensionByID(COOLDOWN_REDUCTION_ASCENSION_ID);

			List<Ascension> toAdd = new List<Ascension>();
			toAdd.Add(CreateAscension(baseAsc, MIND_SPLIT_ASCENSION_ID, "Mind Split", $"Further refine your ability to divide your focus, decreasing the exponential mana penalty for Twin Spell.", 3));
			toAdd.Add(CreateAscension(baseAsc, REFRACTION_ASCENSION_ID, "Refraction", $"Infused with your own magic, your supernatural vigour reduces the recovery period before you can use Twin Spell again by {REFRACTION_COOLDOWN_FACTOR * 100}% per level.", 5));

			foreach (var asc in toAdd)
			{
				if (__instance.GetAscensionByID(asc.Id) != null)
					throw new Exception($"Unable to add skill {asc.SkillName} as Ascension with ID {asc.Id} already exists: {__instance.GetAscensionByID(asc.Id).SkillName}");
				Main.Log.LogInfo($"Skill to be added: {asc.SkillName}");
			}

			__instance.AscensionDatabase = __instance.AscensionDatabase.AddRangeToArray(toAdd.ToArray());
			Main.Log.LogInfo($"Added {toAdd.Count} ascensions to Arcanist.");
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

		private static Ascension CreateAscension(Ascension baseAsc, string id, string name, string desc, int maxRank)
		{
			var ascension = GameObject.Instantiate(baseAsc);
			ascension.Id = id;
			ascension.name = ascension.SkillName = name;
			ascension.UsedBy = Ascension.Class.Arcanist;
			ascension.SkillDesc = desc;
			ascension.MaxRank = maxRank;
			return ascension;
		}
	}
}