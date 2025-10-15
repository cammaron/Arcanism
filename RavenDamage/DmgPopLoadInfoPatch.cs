using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace RavenDamage
{
    [HarmonyPatch(typeof(DmgPop), nameof(DmgPop.LoadInfo), new[] { typeof(int), typeof(bool), typeof(GameData.DamageType), typeof(Transform) })]
    public class DmgPopLoadInfoPatch
    {
		static Vector3 initialScale;
		static bool initialised = false;

        static void Postfix(DmgPop __instance, int _dmg, bool _crit, GameData.DamageType _type, Transform _tar)
        {
			if (!initialised)
            {
				initialised = true;
				initialScale = __instance.transform.localScale; // Assuming they all have the same standard scale...
				Main.Log.LogInfo("Initialising DmgPop LoadInfo patch. Initial scale of DmgPop instance is: " + initialScale);
            }

			__instance.Num.enableVertexGradient = false;
			//__instance.speed = 5f;
			Traverse.Create(__instance).Field("sideDir").SetValue(new Vector3(Random.Range(-1f, 1f), 0, 0));

			float minDistance = 3.0f;
			float distanceFromCamera = Vector3.Distance(__instance.transform.position, GameData.CamControl.transform.position);
			float distanceScaleMultiplier = Mathf.Max(1, ((distanceFromCamera - minDistance) / 17f) * 2f);
			__instance.transform.localScale = initialScale * 2.5f * distanceScaleMultiplier;

			switch (_type)
			{
				case GameData.DamageType.Physical:
					__instance.Num.color = new Color32(140, 140, 140, 255); // Medium grey
					break;
				case GameData.DamageType.Magic:
					__instance.Num.color = new Color32(100, 190, 255, 255); // Sky blue
					return;
				case GameData.DamageType.Elemental:
					__instance.Num.color = new Color32(255, 130, 40, 255); // Orange
					return;
				case GameData.DamageType.Void:
					__instance.Num.color = new Color32(200, 80, 200, 255); // Purple
					return;
				case GameData.DamageType.Poison:
					__instance.Num.color = new Color32(140, 255, 80, 255); // Sorta acid green
					return;
				default:
					return;
			}
		}

    }
}
