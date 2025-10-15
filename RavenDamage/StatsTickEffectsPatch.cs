using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace RavenDamage
{
	[HarmonyPatch(typeof(Stats), "TickEffects")]
	public class StatsTickEffectsPatch
	{
		
		/*
		 A hell of a lot of copied code just to change the arguments that get passed to DamageMe: instead of null for the _attacker, now passing in effect.CreditDPS, which should... in theory... BE the attacker
		This is so that DamageMe knows whether attack is the player and thus whether to show DmgPops or not
		 */
		static bool Prefix(Stats __instance)
		{
			var traverse = Traverse.Create(__instance);
			int maxMana = traverse.Field<int>("CurrentMaxMana").Value;
			for (int i = 0; i < 30; i++)
			{
				if (__instance.StatusEffects[i].Effect != null)
				{
					Spell effect = __instance.StatusEffects[i].Effect;
					if (effect.TargetDamage > 0 && __instance.StatusEffects[i].Duration > 0f)
					{
						string text = "";
						float num = __instance.CheckResist(effect.MyDamageType, 0f, __instance.Myself);
						float num2 = 1f - num;
						if (num2 > 1f)
						{
							num2 = 1f;
						}
						if (Random.Range(0f, 10f) > 6.5f)
						{
							num2 = 1f;
						}
						int num3 = Mathf.RoundToInt(((float)effect.TargetDamage + (float)__instance.StatusEffects[i].bonusDmg) * num2);
						if (num3 < effect.TargetDamage / 2)
						{
							num3 = Mathf.RoundToInt((float)(effect.TargetDamage / 2));
						}
						if (__instance.StatusEffects[i].Owner != null && __instance.StatusEffects[i].Owner.MySkills != null && Random.Range(0, 100) < __instance.StatusEffects[i].Owner.MySkills.GetAscensionRank("19108265") * 33)
						{
							num3 = Mathf.RoundToInt((float)num3 * 1.5f);
							text = " CRITICAL";
						}
						__instance.Myself.DamageMe(num3, __instance.StatusEffects[i].fromPlayer, effect.MyDamageType, __instance.StatusEffects[i].CreditDPS, false, false);
						if (__instance.Myself.isNPC)
						{
							if (__instance.StatusEffects[i].fromPlayer || Vector3.Distance(GameData.PlayerControl.transform.position, __instance.transform.position) < 8f)
							{
								UpdateSocialLog.CombatLogAdd(string.Concat(new string[]
								{
								__instance.transform.name,
								" took ",
								num3.ToString(),
								text,
								" damage from ",
								effect.SpellName
								}), "lightblue");
							}
						}
						else
						{
							UpdateSocialLog.CombatLogAdd(string.Concat(new string[]
							{
							"You took ",
							num3.ToString(),
							text,
							" damage from ",
							effect.SpellName
							}), "red");
						}
						if (__instance.StatusEffects[i].Effect.ReapAndRenew && __instance.Myself.isNPC && __instance.Myself.MyNPC.CurrentAggroTarget != null)
						{
							__instance.Myself.MyNPC.CurrentAggroTarget.MyStats.HealMe(Mathf.RoundToInt((float)num3 * 0.5f));
							if (__instance.StatusEffects[i].Owner != null && Vector3.Distance(__instance.transform.position, GameData.PlayerControl.transform.position) < 15f)
							{
								UpdateSocialLog.LogAdd(string.Concat(new string[]
								{
								__instance.StatusEffects[i].Owner.transform.name,
								"'s affliction renews ",
								__instance.Myself.MyNPC.CurrentAggroTarget.transform.name,
								" health for ",
								Mathf.RoundToInt((float)num3 * 0.5f).ToString(),
								"!"
								}), "green");
							}
						}
					}
					if (effect.BleedDamagePercent > 0)
					{
						int num4 = Mathf.RoundToInt((float)__instance.CurrentHP * 0.07f);
						__instance.Myself.BleedDamageMe(num4, __instance.StatusEffects[i].fromPlayer, __instance.StatusEffects[i].CreditDPS);
						if (__instance.Myself.isNPC)
						{
							if (__instance.StatusEffects[i].fromPlayer || Vector3.Distance(GameData.PlayerControl.transform.position, __instance.transform.position) < 8f)
							{
								UpdateSocialLog.CombatLogAdd(string.Concat(new string[]
								{
								__instance.transform.name,
								" took ",
								num4.ToString(),
								" BLEED damage from ",
								effect.SpellName
								}), "lightblue");
							}
						}
						else
						{
							UpdateSocialLog.CombatLogAdd("You took " + num4.ToString() + " BLEED damage from " + effect.SpellName, "red");
						}
					}
					if (effect.TargetHealing > 0 && __instance.StatusEffects[i].Duration > 0f && effect.MyDamageType == GameData.DamageType.Physical)
					{
						int num5 = effect.TargetHealing;
						if (__instance.StatusEffects[i].Owner != null && __instance.StatusEffects[i].Owner.MyStats != null && !effect.WornEffect)
						{
							num5 += Mathf.RoundToInt((float)__instance.StatusEffects[i].Owner.MyStats.WisScaleMod / 100f * (float)__instance.StatusEffects[i].Owner.MyStats.GetCurrentWis() * 10f);
							if (__instance.StatusEffects[i].Owner.MyStats.CharacterClass == GameData.ClassDB.Druid)
							{
								num5 += __instance.StatusEffects[i].Owner.MyStats.GetCurrentWis();
							}
						}
						__instance.CurrentHP += num5;
						if (__instance.CurrentHP > __instance.CurrentMaxHP)
						{
							__instance.CurrentHP = __instance.CurrentMaxHP;
						}
						if (__instance.StatusEffects[i].fromPlayer && !effect.WornEffect)
						{
							UpdateSocialLog.LogAdd(string.Concat(new string[]
							{
							"Your ",
							effect.SpellName,
							" heals ",
							__instance.transform.name,
							" for ",
							num5.ToString(),
							" points of damage!"
							}), "green");
						}
					}
					if (effect.Mana > 0 && __instance.StatusEffects[i].Duration > 0f)
					{
						if (effect.Type == Spell.SpellType.Beneficial)
						{
							__instance.CurrentMana += effect.Mana;
						}
						if (__instance.CurrentMana > maxMana)
						{
							__instance.CurrentMana = maxMana;
						}
						if (effect.Type != Spell.SpellType.Beneficial)
						{
							__instance.CurrentMana -= effect.Mana;
						}
						if (__instance.CurrentMana < 0)
						{
							__instance.CurrentMana = 0;
						}
					}
					if (effect.PercentManaRestoration > 0)
					{
						if (effect.Type == Spell.SpellType.Beneficial)
						{
							__instance.CurrentMana += Mathf.RoundToInt((float)maxMana * (float)effect.PercentManaRestoration / 100f);
						}
						if (__instance.CurrentMana > maxMana)
						{
							__instance.CurrentMana = maxMana;
						}
					}
					if (effect.UnstableDuration && Random.Range(0, effect.RequiredLevel * 100) < __instance.CheckResistSimple(effect.MyDamageType))
					{
						__instance.StatusEffects[i].Duration = 0f;
					}
					__instance.StatusEffects[i].Duration -= 1f;
					if (__instance.StatusEffects[i].Duration <= 0f && __instance.StatusEffects[i].Effect != null)
					{
						__instance.RemoveStatusEffect(i);
					}
				}
				if (__instance.Myself == GameData.PlayerControl.CurrentTarget)
				{
					GameData.NPCEffects.UpdateTargetEffects(__instance.StatusEffects);
				}
			}
			if (__instance.StatusIcons.Count > 0)
			{
				traverse.Method("UpdateIcons").GetValue();
			}
			if (__instance.Myself == GameData.PlayerControl.CurrentTarget)
			{
				GameData.NPCEffects.UpdateTargetEffects(__instance.StatusEffects);
			}

			return false;
		}
	}
}
