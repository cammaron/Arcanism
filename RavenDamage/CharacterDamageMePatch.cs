using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace RavenDamage
{
	[HarmonyPatch(typeof(Character), nameof(Character.DamageMe))]
	public class CharacterDamageMePatch
	{

		/*
		 To get damage ticks from status effects the player applies to show damage popups, the _attacker must be set to the player
		 */
		static void Prefix(int _incdmg, bool _fromPlayer, GameData.DamageType _dmgType, ref Character _attacker, bool _animEffect, bool _criticalHit)
		{
			// removed this for now as for some reason *other party member's* DoT effects have _fromPlayer = true
			//if (_fromPlayer && _attacker == null) _attacker = GameData.PlayerControl.Myself;
		}

	}
}
