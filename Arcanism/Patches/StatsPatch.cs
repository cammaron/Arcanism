using UnityEngine;
using HarmonyLib;


namespace Arcanism
{
	[HarmonyPatch(typeof(Stats), nameof(Stats.RemoveBreakableEffects))]
	public class StatsRemoveBreakableEffectsPatch
	{

		/* This patch is just about making Coma a more effective spell, so that its sleep effect can still be interrupted by damage but is less likely to be than a standard "unstable duration" effect. */
		static bool Prefix(Stats __instance)
		{
			var effects = __instance.StatusEffects;
			for (int i = 0; i <= effects.Length - 1; i++)
			{
				var effect = effects[i];
				if (effect?.Effect != null)
                {
					if (effect.Effect.BreakOnDamage)
                    {
						__instance.RemoveStatusEffect(i);
                    } else if (effect.Effect.UnstableDuration)
                    {
						int interruptChance = 20;
						if (effect.Effect.Id == SpellDBStartPatch.COMA_SPELL_ID) interruptChance = 10;
						if (Random.Range(0, 100) < interruptChance)
                        {
							__instance.RemoveStatusEffect(i);
							if (Vector3.Distance(__instance.transform.position, GameData.PlayerControl.transform.position) < 30f)
							{
								UpdateSocialLog.LogAdd(__instance.transform.name + " comes to their senses!", "yellow");
							}
						}
                    }
                }
			}
			return false;
		}
	}
}