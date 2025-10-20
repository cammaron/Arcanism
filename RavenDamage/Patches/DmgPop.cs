using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace BigDamage.Patches
{
    [HarmonyPatch(typeof(DmgPop), nameof(DmgPop.LoadInfo), new[] { typeof(int), typeof(bool), typeof(GameData.DamageType), typeof(Transform) })]
    public class DmgPop_LoadInfo
    {
		static Vector3 initialScale;
		static bool initialised = false;

        static void Postfix(DmgPop __instance, int _dmg, bool _crit, GameData.DamageType _type, Transform _tar)
        {
			if (!initialised)
            {
				initialised = true;
				initialScale = __instance.transform.localScale; // Assuming they all have the same standard scale...
            }

			__instance.Num.enableVertexGradient = false;
			Traverse.Create(__instance).Field("sideDir").SetValue(new Vector3(Random.Range(-1f, 1f), 0, 0));

			float minDistance = 3.0f;
			float distanceFromCamera = Vector3.Distance(__instance.transform.position, GameData.CamControl.transform.position);
			float distanceScaleMultiplier = Mathf.Max(1, ((distanceFromCamera - minDistance) / Main.distanceToDoubleSize.Value) * 2f);
			__instance.transform.localScale = initialScale * Main.baseSizeFactor.Value * distanceScaleMultiplier;

			switch (_type)
			{
				case GameData.DamageType.Physical:
					__instance.Num.color = Main.physColour;
					break;
				case GameData.DamageType.Magic:
					__instance.Num.color = Main.magColour;
					return;
				case GameData.DamageType.Elemental:
					__instance.Num.color = Main.eleColour;
					return;
				case GameData.DamageType.Void:
					__instance.Num.color = Main.voidColour;
					return;
				case GameData.DamageType.Poison:
					__instance.Num.color = Main.poisonColour;
					return;
				default:
					return;
			}
		}

    }
}
