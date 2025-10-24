using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Arcanism.SkillExtension;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(TypeText), "CheckCommands")]
	public class TypeTextCheckCommandsPatch
	{
		static Spell testSpell = null;

		static Item lastItemEdit = null;
		static string lastItemSetting = null;

		/* Custom commands to assist with testing */
		static bool Prefix(Text ___typed)
		{
			var txt = ___typed.text?.Trim();
			if (txt == null) return true;


			

			if (txt.StartsWith("/arc.litem"))
			{
				if (lastItemEdit == null)
				{
					UpdateSocialLog.LogAdd("Last item is null. Edit an item first.");
					return false;
				}
				txt = txt.Replace("/arc.litem", $"/arc.item {lastItemEdit.ItemName.Replace(" ", "_")}");
			} else if (txt.StartsWith("/arc.sitem"))
			{
				if (lastItemEdit == null || lastItemSetting == null)
				{
					UpdateSocialLog.LogAdd("Last item setting is null. Edit an item first.");
					return false;
				}
				txt = txt.Replace("/arc.sitem", $"/arc.item {lastItemEdit.ItemName.Replace(" ", "_")} {lastItemSetting}");
			}

			txt = txt.Replace("  ", " ");
			var lowerTxt = txt.ToLower();

			var stats = GameData.PlayerStats;
			if (lowerTxt.StartsWith("/arc.items"))
            {
				var tokens = lowerTxt.Split(' ');
				int startIdx = int.Parse(tokens[1]);
				int endIdx = int.Parse(tokens[2]);
				for (var i = startIdx; i < GameData.ItemDB.ItemDB.Length && i <= endIdx; i++)
				{
					Main.Log.LogInfo($"ItemDB[{i}] = {GameData.ItemDB.ItemDB[i].name}, {GameData.ItemDB.ItemDB[i].ItemName}, {GameData.ItemDB.ItemDB[i].Id}");
				}
				return false;
            }
			else if (lowerTxt.StartsWith("/arc.reloadsprites"))
            {
				Main.LoadSprites();
				ItemDatabase_Start.RefreshSprites();
				return false;
            }
			else if (lowerTxt.StartsWith("/arc.item")) //    /arc.item Item_Name_With_Underscores_Not_Spaces (SettingType) (SettingValue)
            {
				var tokens = txt.Split(' ');
				if (tokens.Length < 3)
                {
					UpdateSocialLog.LogAdd($"Required syntax:   /arc.item Item_name_with_underscores type|shoulders|elbows|knees|c1|c2|l1|l2|m1|m2 val");
					return false;
                }
				UpdateSocialLog.LogAdd("Last setting is=" + lastItemSetting);
				for(var i = 0; i < tokens.Length; i ++)
                {
					UpdateSocialLog.LogAdd($"tokens[{i}]={tokens[i]}");
                }

				UpdateSocialLog.LogAdd("Entered command: " + txt);
				string name = tokens[1].Replace("_", " ");
				string setting = tokens[2];
				string newVal = tokens[3];
				string lowerName = name.ToLower();
				Item item = GameData.ItemDB.ItemDBList.Find(i => i.ItemName.ToLower().StartsWith(lowerName));
				GameData.ItemDB.ItemDB.Do(i => {
					if (i.ItemName.ToLower() == name) 
						item = i; 
				});
				if (item == null)
                {
					UpdateSocialLog.LogAdd($"No item found with name '{name}'. Provide item name as first argument with spaces replaced by underscores (case insensitive)");
					return false;
                }
				
				lastItemSetting = setting;
				lastItemEdit = item;

				if (setting == "type")
                {
					item.EquipmentToActivate = newVal;
				}
				else if (setting == "shoulders")
				{
					string[] subtokens = newVal.Split(',');
					item.ShoulderTrimL = subtokens[0];
					item.ShoulderTrimR = subtokens[1];
				}
				else if (setting == "elbows")
				{
					string[] subtokens = newVal.Split(',');
					item.ElbowTrimL = subtokens[0];
					item.ElbowTrimR = subtokens[1];
				}
				else if (setting == "knees")
				{
					string[] subtokens = newVal.Split(',');
					item.KneeTrimL = subtokens[0];
					item.KneeTrimR = subtokens[1];
				}
				else if (setting == "c1") // colour 1 (primary)
				{
					string[] subtokens = newVal.Split(',');
					item.ItemPrimaryColor = new Color32(byte.Parse(subtokens[0]), byte.Parse(subtokens[1]), byte.Parse(subtokens[2]), 255);
				}
				else if (setting == "c2") // colour 2 (secondary)
				{
					string[] subtokens = newVal.Split(',');
					item.ItemSecondaryColor = new Color32(byte.Parse(subtokens[0]), byte.Parse(subtokens[1]), byte.Parse(subtokens[2]), 255);
				}
				else if (setting == "l1") // leather primary
				{
					string[] subtokens = newVal.Split(',');
					item.ItemLeatherPrimary = new Color32(byte.Parse(subtokens[0]), byte.Parse(subtokens[1]), byte.Parse(subtokens[2]), 255);
				}
				else if (setting == "l2") // leather secondary
				{
					string[] subtokens = newVal.Split(',');
					item.ItemLeatherSecondary = new Color32(byte.Parse(subtokens[0]), byte.Parse(subtokens[1]), byte.Parse(subtokens[2]), 255);
				}
				else if (setting == "m1") // metal primary
				{
					string[] subtokens = newVal.Split(',');
					item.ItemMetalPrimary = new Color32(byte.Parse(subtokens[0]), byte.Parse(subtokens[1]), byte.Parse(subtokens[2]), 255);
				}
				else if (setting == "m2") // metal secondary
				{
					string[] subtokens = newVal.Split(',');
					item.ItemMetalSecondary = new Color32(byte.Parse(subtokens[0]), byte.Parse(subtokens[1]), byte.Parse(subtokens[2]), 255);
				} else
                {
					UpdateSocialLog.LogAdd($"Unrecognised setting: {setting}");
                }
				GameData.PlayerInv.UpdatePlayerInventory();
				return false;
			}
			if (lowerTxt.StartsWith("/arc.asc")) //    /arc.asc "Proficiency Name" level
            {
				var tokens = txt.Split(' ');
				string ascName = txt.Split('"')[1];
				int level = int.Parse(tokens[tokens.Length - 1]);
				var ascension = GameData.SkillDatabase.GetAscensionByName(ascName);
				if (ascension == null)
                {
					UpdateSocialLog.LogAdd($"No ascension found with name '{ascName}'. Ensure you put the name in quotes. Also, it's case sensitive.");
					return false;
                }

				stats.Myself.MySkills.AddAscension(ascension.Id);
				var ascensionEntry = stats.Myself.MySkills.MyAscensions.Find(x => x.id == ascension.Id);
				ascensionEntry.level = level;
				stats.CalcStats();
				UpdateSocialLog.LogAdd($"Ascension {ascension.SkillName} level set to {level}/{ascension.MaxRank}");
				return false;
			}

			if (lowerTxt.StartsWith("/arc.skill"))  //    /arc.skill "Skill Name" (adds if don't got, removes if got)
            {
				var tokens = txt.Split(' ');
				string skillName = txt.Split('"')[1];
				var skill = GameData.SkillDatabase.GetSkillByName(skillName);
				if (skill == null)
				{
					UpdateSocialLog.LogAdd($"No skill found with name '{skillName}'. Ensure you put the name in quotes. Also, it's case sensitive.");
					return false;
				}

				if (stats.Myself.MySkills.KnowsSkill(skill))
                {
					stats.Myself.MySkills.KnownSkills.Remove(skill);
					UpdateSocialLog.LogAdd($"Skill {skill.SkillName} removed.");
				} else
                {
					stats.Myself.MySkills.KnownSkills.Add(skill);
					UpdateSocialLog.LogAdd($"Skill {skill.SkillName} added.");
				}

				return false;
			}

			if (lowerTxt.StartsWith("/arc.twinrelax")) // allow twinning spells against same target, for testing purposes
			{
				TwinSpell.AllowSameTarget = !TwinSpell.AllowSameTarget;
				UpdateSocialLog.LogAdd("Twin Spell allow same target setting changed to: " + TwinSpell.AllowSameTarget);
				return false;
			}

			if (lowerTxt.StartsWith("/arc.twincd")) //    /arc.twincd  cooldown     -- change cooldown for Twin Spell skill
            {
				int cooldownSeconds = int.Parse(txt.Split(' ')[1]);
				GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.TWIN_SPELL_SKILL_ID).Cooldown = cooldownSeconds * 60f;
				UpdateSocialLog.LogAdd("Twin Spell cooldown changed to " + cooldownSeconds);
				return false;
            }

			if (lowerTxt.StartsWith("/arc.chantcd")) //    /arc.chantcd  cooldown     -- change cooldown for Control Chant skill
			{
				int cooldownSeconds = int.Parse(txt.Split(' ')[1]);
				GameData.SkillDatabase.GetSkillByID(SkillDBStartPatch.CONTROL_CHANT_SKILL_ID).Cooldown = cooldownSeconds * 60f;
				UpdateSocialLog.LogAdd("Control Chant cooldown changed to " + cooldownSeconds);
				return false;
			}

			if (lowerTxt.StartsWith("/arc.sdmg")) //   /arc.sdmg int prof spelldmg
			{
				var tokens = txt.Split(' ');
				int intelligence = int.Parse(tokens[1]);
				int prof = int.Parse(tokens[2]);
				int damage = int.Parse(tokens[3]);

				if (testSpell == null)
				{
					testSpell = GameObject.Instantiate(GameData.SpellDatabase.GetSpellByID("63468487"));
					testSpell.SpellChargeTime = 0f;
					testSpell.Cooldown = 0f;
					testSpell.name = testSpell.SpellName = "TestSpell";
				}
				testSpell.TargetDamage = damage;

				// just add a delta to BaseInt to get total int where we need it, since CurrentInt is private
				var intField = Traverse.Create(stats).Field<int>("CurrentInt");
				UpdateSocialLog.LogAdd("Changing CurrentInt from " + intField.Value + " to " + intelligence);
				intField.Value = intelligence;
				stats.IntScaleMod = Mathf.Clamp(prof, 1, 40);
				stats.Myself.MySpells.StartSpellNoAnim(testSpell, GameData.PlayerControl.CurrentTarget.MyStats, 0f);
				return false;
			}

			return true;
		}
	}
}