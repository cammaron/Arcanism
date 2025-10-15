using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace RavenDamage
{
	[HarmonyPatch(typeof(Misc), nameof(Misc.GenPopup))]
	public class MiscGenPopupPatch
	{

		/*
		 By default, GenPopup will only show a DmgPop if it's less than 20 units from the player. This patch increases that range to 55.
		 */
		static bool Prefix(Misc __instance, int _dmg, bool _crit, GameData.DamageType _type, Transform _tar)
		{
			float distanceFromCamera = Vector3.Distance(_tar.transform.position, GameData.PlayerControl.transform.position);
			if (distanceFromCamera < 55f)
			{
				var popIndexField = Traverse.Create(__instance).Field<int>("popIndex"); // popIndex is private, so we have to use reflection to access it
				int popIndex = popIndexField.Value;
				__instance.DmgPopup[popIndex].gameObject.SetActive(true);
				__instance.DmgPopup[popIndex].LoadInfo(_dmg, _crit, _type, _tar);
				popIndex++;
				if (popIndex > 198)
				{
					popIndex = 0;
				}
				popIndexField.Value = popIndex;
			}

			return false;
		}

	}
}
