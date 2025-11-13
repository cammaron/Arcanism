using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Arcanism.Skills;

namespace Arcanism.Patches
{
	[HarmonyPatch(typeof(TypeText), "CheckCommands")]
	public class TypeText_CheckCommands
	{
		static Spell testSpell = null;

		static Item lastItemEdit = null;
		static string lastItemSetting = null;

		static ParticleSystem ps;

		/* Custom commands to assist with testing */
		static bool Prefix(Text ___typed)
		{
			var txt = ___typed.text?.Trim();
			if (txt == null) return true;



			if (txt.StartsWith("/arc.litem")) // LITEM = "last item" -- /arc.litem <setting> <val> -- shorthand for /arc.item <lastitem> <setting> <val>
			{
				if (lastItemEdit == null)
				{
					UpdateSocialLog.LogAdd("Last item is null. Edit an item first.");
					return false;
				}
				txt = txt.Replace("/arc.litem", $"/arc.item {lastItemEdit.ItemName.Replace(" ", "_")}");
			} else if (txt.StartsWith("/arc.sitem")) // SITEM = "last item setting" --  /arc.sitem <val> -- shorthand for /arc.item <lastitem> <lastsetting> <val>
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
			if (lowerTxt.StartsWith("/arc.shield"))
            {
				var shield = stats.GetComponent<SpellShieldVisual>();
				if (shield == null)
				{
					shield = stats.gameObject.AddComponent<SpellShieldVisual>();
					shield.stats = stats;
					shield.Init();
				}

				var tokens = txt.Split(' ');
				foreach(var t in tokens)
                {
					if (t.StartsWith("/")) continue;

					var subs = t.Split('=');
					if (subs[0] == "amt")
                    {
						stats.SpellShield = int.Parse(subs[1]);
						UpdateSocialLog.LogAdd("And setting shield amount to " + stats.SpellShield);
						continue;
					}

					char propPrefix = subs[0][0];
					string prop = subs[0].Substring(1, subs[0].Length - 1);
					if (propPrefix == 'c')
                    {
						var colorTokens = subs[1].Split('-');
						var color = new Color32(byte.Parse(colorTokens[0]), byte.Parse(colorTokens[1]), byte.Parse(colorTokens[2]), byte.Parse(colorTokens[3]));
						shield.material.SetColor(prop, color);
						UpdateSocialLog.LogAdd($"Setting color {prop} to {color}");
					} else if (propPrefix == 'f')
                    {
						float val = float.Parse(subs[1]);
						shield.material.SetFloat(prop, val);
						UpdateSocialLog.LogAdd($"Setting float {prop} to {val}");
					}
                }
				
				return false;
			}
			else if (lowerTxt.StartsWith("/arc.fixcam"))
            {
				GameData.CamControl.ActualCam.tag = "MainCamera";
				UpdateSocialLog.LogAdd("Main camera fixed.");
				return false;
			} else if (lowerTxt.StartsWith("/arc.giant"))
            {
				var npc = GameData.PlayerControl.CurrentTarget?.GetComponent<TheyMightBeGiants>();
				if (npc == null)
				{
					UpdateSocialLog.LogAdd("Target a non-sim NPC first.");
					return false;
				}
				if (lowerTxt.StartsWith("/arc.gianter"))
					npc.GetComponent<TheyMightBeGiants>().BecomeSupermassive();
				else
					npc.GetComponent<TheyMightBeGiants>().BecomeGiant();
			}
			else if (lowerTxt.StartsWith("/arc.spell"))
            {
				string lineName = lowerTxt.Split(' ')[1];
				UpdateSocialLog.LogAdd("Looking for spell with line " + lineName);
				foreach(var s in GameData.SpellDatabase.SpellDatabase)
                {
					if (s.Line.ToString().ToLower() == lineName)
						UpdateSocialLog.LogAdd("Found spell: " + s.SpellName + " with line " + s.Line.ToString());
                }
				return false;
            } else if (lowerTxt.StartsWith("/arc.youreaflamewellharry")) // ... worth it
			{
				UpdateSocialLog.LogAdd("Current target: " + GameData.PlayerControl.CurrentTarget);
				var npc = GameData.PlayerControl.CurrentTarget?.GetComponent<Character>();
				if (npc == null)
				{
					UpdateSocialLog.LogAdd("Target a non-sim NPC first.");
					return false;
				}
				var oldName = npc.gameObject.name;
				npc.gameObject.name = "Braxonian Flame Well";
				UpdateSocialLog.LogAdd($"{oldName} renamed to {npc.gameObject.name}. You really are hacking a sim to be a flame well, aren't you? Kind of cruel, really.");
				return false;
			}
			else if (lowerTxt.StartsWith("/arc.simupgrade") || lowerTxt.StartsWith("/arc.upgradesim"))
			{
				var sim = GameData.PlayerControl.CurrentTarget.GetComponent<SimPlayer>();
				if (sim == null)
				{
					UpdateSocialLog.LogAdd("Target a sim first.");
					return false;
				}
				sim.NearFlamewell = true;
				sim.NearForge = true;
				GameData.SimMngr.Sims[GameData.InspectSim.Who.myIndex].Sivaks = 25;
				GameData.SimMngr.Sims[GameData.InspectSim.Who.myIndex].Planars = 25;
				UpdateSocialLog.LogAdd("Adding sivaks/planars to sim and triggering 'near forge/near flamewell.'");
				return false;
			}
			else if (lowerTxt.StartsWith("/arc.loot"))
			{
				Character target = GameData.PlayerControl.CurrentTarget;
				if (target == null)
				{
					UpdateSocialLog.LogAdd("Target an enemy first.");
					return false;
				}
				string[] tokens = lowerTxt.Split(' ');
				bool requireItem = false;
				Item item = null;
				ItemExtensions.Blessing blessing = ItemExtensions.Blessing.NONE;
				ItemExtensions.Quality quality = ItemExtensions.Quality.NORMAL;
				var lootTable = target.GetComponent<LootTable>();
				lootTable.ActualDrops.Clear();
				lootTable.ActualDropsQual.Clear();
				foreach (var token in tokens)
				{
					if (token.StartsWith("item"))
					{
						string name = token.Split('=')[1];
						string lowerName = name.ToLower().Replace('_', ' ');
						requireItem = true;
						item = GameData.ItemDB.ItemDBList.Find(i => i.ItemName.ToLower().StartsWith(lowerName));
						if (item == null)
						{
							UpdateSocialLog.LogAdd($"No item found with name '{lowerName}'. Replace spaces with underscores!! (case insensitive)");
							return false;
						}
					}
					else if (token.Contains("bless"))
					{
						requireItem = true;
						blessing = ItemExtensions.Blessing.BLESSED;
					}
					else if (token.Contains("god"))
					{
						requireItem = true;
						blessing = ItemExtensions.Blessing.GODLY;
					}
					else if (token == "superior")
					{
						requireItem = true;
						quality = ItemExtensions.Quality.SUPERIOR;
					}
					else if (token == "masterwork")
					{
						requireItem = true;
						quality = ItemExtensions.Quality.MASTERWORK;
					}
					else if (token == "exquisite")
					{
						requireItem = true;
						quality = ItemExtensions.Quality.EXQUISITE;
					}
					else if (token == "map") {
						UpdateSocialLog.LogAdd("Adding map drop to enemy loot table. GM.Maps.Count is " + GameData.GM.Maps.Count);
						UpdateSocialLog.LogAdd("M aps[0].ItemName is " + GameData.GM.Maps[0].ItemName);
						lootTable.ActualDrops.Add(GameData.GM.Maps[0]);
					}
					else if (token == "sivak" || token == "siva")
						lootTable.ActualDrops.Add(GameData.GM.Sivak);
					else if (token == "charm")
						lootTable.ActualDrops.Add(GameData.GM.WorldDropMolds[Random.Range(0, GameData.GM.WorldDropMolds.Count)]);
					else if (token == "xppot")
						lootTable.ActualDrops.Add(GameData.GM.XPPot);
					else if (token == "diamond")
						lootTable.ActualDrops.Add(GameData.GM.InertDiamond);
					else if (token == "shard")
						lootTable.ActualDrops.Add(GameData.GM.PlanarShard);
					else if (token == "planar")
						lootTable.ActualDrops.Add(GameData.GM.Planar);
				}

				if (requireItem)
                {
					UpdateSocialLog.LogAdd("Adding item " + item + " to target loot table");
					if (item == null) item = GameData.ItemDB.GetItemByID(ItemId.ARMBANDS_OF_ORDER);
					lootTable.ActualDrops.Insert(0, item);
                }
				
				lootTable.ActualDropsQual = new List<int>(lootTable.ActualDrops.Count);
				var helper = lootTable.GetComponent<LootHelper>();
				helper.itemMeta?.Clear();
				helper.UpdateLootQuality();

				if (requireItem)
					helper.itemMeta[0] = (item, blessing, quality);
				UpdateSocialLog.LogAdd("Updated target's loot table.");
				return false;
			} else if (lowerTxt.StartsWith("/arc.ps")) // Particle System manipulation, various options below
            {
				if (ps != null) GameObject.Destroy(ps.gameObject);
				ps = GameObject.Instantiate(GameData.GM.SpecialLootBeam, GameData.PlayerControl.Myself.transform.position + Vector3.forward * 3f + Vector3.up, Quaternion.identity).GetComponent<ParticleSystem>();
				
				var emission = ps.emission;
				ps.startSize = 5;
				var main = ps.main;
				var sizeLifetime = ps.sizeOverLifetime;
				sizeLifetime.enabled = true;
				var colorLifetime = ps.colorOverLifetime;
				colorLifetime.enabled = true;
				var rotationLifetime = ps.rotationOverLifetime;
				main.duration = 1000000;
				main.prewarm = true;
				//main.startRotationZMultiplier = main.startRotationYMultiplier = main.startRotationXMultiplier = 1;
				rotationLifetime.enabled = true; rotationLifetime.separateAxes = true;
				rotationLifetime.y = rotationLifetime.x = rotationLifetime.z = 0f;
				rotationLifetime.y = 5f;
				main.simulationSpeed = 1;
				main.startLifetime = 5;
				emission.rateOverTime = 4;//ps.emissionRate = 4;
				colorLifetime.color = new ParticleSystem.MinMaxGradient(new Color32(200, 200, 255, 30), new Color32(200, 200, 255, 30));
				ps.transform.localScale = new Vector3(1, 1, 1);
				var tokens = lowerTxt.Substring(8).Split(' ');
				foreach(var token in tokens)
                {
					var subtokens = token.Split('=');
					var key = subtokens[0];
					if (key == "scale")
                    {
						var subs = subtokens[1].Split(',');
						ps.transform.localScale = new Vector3(float.Parse(subs[0]), float.Parse(subs[1]), float.Parse(subs[0]));
					}
					else if (float.TryParse(subtokens[1], out float val))
                    {
						if (key == "rate")
							emission.rateOverTime = val;
						else if (key == "speed")
							main.simulationSpeed = val;
						else if (key == "size")
							sizeLifetime.size = val;
						else if (key == "lifetime")
							main.startLifetime = val;
						else if (key == "sizelifex")
							sizeLifetime.x = new ParticleSystem.MinMaxCurve(sizeLifetime.size.constant, val);
						else if (key == "sizelifey")
							sizeLifetime.y = new ParticleSystem.MinMaxCurve(sizeLifetime.size.constant, val);
						else if (key == "sizelifez")
							sizeLifetime.z = new ParticleSystem.MinMaxCurve(sizeLifetime.size.constant, val);
						else if (key == "rotationx")
							rotationLifetime.x = val;
						else if (key == "rotationy")
							rotationLifetime.y = val;
						else if (key == "rotationz")
							rotationLifetime.z = val;
						else if (key == "rotationstartx")
							main.startRotationX = new ParticleSystem.MinMaxCurve(0, val);
						else if (key == "rotationstarty")
							main.startRotationY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 3, 90));
						else if (key == "rotationstartz")
							main.startRotationZ = new ParticleSystem.MinMaxCurve(0, val);
					} else
                    {
						var colorTokens = subtokens[1].Split('-');
						var color = new Color32(byte.Parse(colorTokens[0]), byte.Parse(colorTokens[1]), byte.Parse(colorTokens[2]), byte.Parse(colorTokens[3]));
						if (key == "colorstart")
							colorLifetime.color = new ParticleSystem.MinMaxGradient(color, colorLifetime.color.colorMax);
						else if (key == "colorend")
							colorLifetime.color = new ParticleSystem.MinMaxGradient(colorLifetime.color.colorMin, color);
					}
                }
				UpdateSocialLog.LogAdd("Made PS with args " + lowerTxt);
            }
			else if (lowerTxt.StartsWith("/arc.refresh"))
			{
				ItemDatabase_Start.UpdateItemDatabase(GameData.ItemDB);
				UpdateSocialLog.LogAdd("Refreshed databases (just item DB for now, and won't work on new items)");
				return false;
			}
			else if (lowerTxt.StartsWith("/arc.item.id")) // look up item details by ID
			{
				string id = txt.Split(' ')[1];
				var item = GameData.ItemDB.GetItemByID(id);
				if (item != null)
					UpdateSocialLog.LogAdd($"[{id}] {item.ItemName} - {item.RequiredSlot}");
				else
					UpdateSocialLog.LogAdd($"No item found for ID {id}");
				return false;
			}
			else if (lowerTxt.StartsWith("/arc.item.search")) // look up IDs of all items with name containing arg
			{
				string name = txt.Split('"')[1];
				string lowerName = name.ToLower();
				UpdateSocialLog.LogAdd("Search results for " + name + " (note: case insensitive, all items name contains string, use quotation marks)");
				foreach(var item in GameData.ItemDB.ItemDBList.FindAll(i => i.ItemName.ToLower().Contains(lowerName)))
                {
					UpdateSocialLog.LogAdd($"[{item.Id}] {item.ItemName} - {item.RequiredSlot}");
				}

				return false;
			}
			else if (lowerTxt.StartsWith("/arc.item.add"))
			{
				string[] tokens;
				Item item;
				if (!txt.Contains("\""))
				{
					// then maybe we're adding by ID
					tokens = lowerTxt.Split(' ');
					string id = tokens[1];
					item = GameData.ItemDB.GetItemByID(id);
					if (item == null)
                    {
						UpdateSocialLog.LogAdd($"No item found with ID '{id}'. NB syntax is either `/arc.item.add \"name in quotes\" qty` or `/arc.item.add id qty`. Qty is optional.");
						return false;
                    }
				} else
                {
					tokens = lowerTxt.Split('"');
					string name = tokens[1];
					string lowerName = name.ToLower();
					item = GameData.ItemDB.ItemDBList.Find(i => i.ItemName.ToLower() == (lowerName));
					if (item == null)
					{
						UpdateSocialLog.LogAdd($"No item found with name '{name}'.  NB syntax is either `/arc.item.add \"name in quotes\" qty` or `/arc.item.add id qty`. Qty is optional.");
						return false;
					}
				}

				foreach (var t in tokens) UpdateSocialLog.LogAdd("TOKENS: " + t);
				int qty;
				if (!(tokens.Length == 3 && int.TryParse(tokens[2], out qty)))
					qty = 1;

				GameData.PlayerInv.AddItemToInv(item, qty);
				UpdateSocialLog.LogAdd($"Added {item.ItemName} to inventory.");
				return false;
			}
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
				Main.Instance.LoadFiles();
				ItemDatabase_Start.RefreshSprites();
				SkillDB_Start.RefreshSprites();
				return false;
            }
			else if (lowerTxt.StartsWith("/arc.item")) //  for changing item appearance.   /arc.item Item_Name_With_Underscores_Not_Spaces (SettingType) (SettingValue)
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
				GameData.SkillDatabase.GetSkillByID(SkillDB_Start.TWIN_SPELL_SKILL_ID).Cooldown = cooldownSeconds * 60f;
				UpdateSocialLog.LogAdd("Twin Spell cooldown changed to " + cooldownSeconds);
				return false;
            }

			if (lowerTxt.StartsWith("/arc.chantcd")) //    /arc.chantcd  cooldown     -- change cooldown for Control Chant skill
			{
				int cooldownSeconds = int.Parse(txt.Split(' ')[1]);
				GameData.SkillDatabase.GetSkillByID(SkillDB_Start.CONTROL_CHANT_SKILL_ID).Cooldown = cooldownSeconds * 60f;
				UpdateSocialLog.LogAdd("Control Chant cooldown changed to " + cooldownSeconds);
				return false;
			}

			if (lowerTxt.StartsWith("/arc.sdmg")) //   /arc.sdmg int prof spelldmg
			{
				UpdateSocialLog.LogAdd("sdmg usage: /arc.sdmg lvl int prof basedmg [all params required]");
				var tokens = txt.Split(' ');
				int level = int.Parse(tokens[1]);
				int intelligence = int.Parse(tokens[2]);
				int prof = int.Parse(tokens[3]);
				int damage = int.Parse(tokens[4]);

				if (testSpell == null)
				{
					testSpell = GameObject.Instantiate(GameData.SpellDatabase.GetSpellByID("63468487"));
					testSpell.SpellChargeTime = 0f;
					testSpell.Cooldown = 0f;
					testSpell.name = testSpell.SpellName = "TestSpell";
				}
				testSpell.TargetDamage = damage;

				var intField = Traverse.Create(stats).Field<int>("CurrentInt");
				var oldInt = intField.Value;
				var oldLvl = stats.Level;
				var oldScale = stats.IntScaleMod;
				UpdateSocialLog.LogAdd($"Testing spell: level {level}, int {intelligence}, prof {prof}, base damage {damage}");
				intField.Value = intelligence;
				stats.IntScaleMod = Mathf.Clamp(prof, 1, 40);
				stats.Myself.MySpells.StartSpellNoAnim(testSpell, GameData.PlayerControl.CurrentTarget.MyStats, 0f);
				stats.StartCoroutine(ResetStats(stats, intField, oldLvl, oldInt, oldScale));
				return false;
			}

			return true;
		}
		static IEnumerator ResetStats(Stats stats, Traverse<int> intField, int oldLvl, int oldInt, int oldScale)
        {
			yield return new WaitForSeconds(3f);
			UpdateSocialLog.LogAdd("Resetting stats to old values.");
			intField.Value = oldInt;
			stats.Level = oldLvl;
			stats.IntScaleMod = oldScale;
        }
	}
}