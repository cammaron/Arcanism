using UnityEngine;
using HarmonyLib;

namespace Arcanism
{
    /*
     The grand purpose of this patch is to redo Control Chant so that:
    - Early releases do full damage in less time for proportionately less mana and cooldown, but also carry proportionate risk of backfiring based on chant time
    - Late releases can be charged longer, for disproportionately more damage and mana cost
    - Late releases drain mana while chanting, and running out before releasing results in a backfire
     
     */
    [HarmonyPatch(typeof(SpellVessel), nameof(SpellVessel.CompleteSpellEarlyWithScaling))] // AFAICT this method is used both for early and late release, despite the name
    class SpellVesselCompleteEarlyPatch
    {
        static bool Prefix(SpellVessel __instance)
        {
            var traverse = Traverse.Create(__instance);
            float effectLife = traverse.Field<float>("EffectLife").Value;
            float totalLife = traverse.Field<float>("totalLife").Value;

            var controlChant = __instance.GetComponent<ControlChant>();
            if (controlChant != null)
            {
                Main.Log.LogInfo("SpellVessel.CompleteSpellEarlyWithScaling: calling EndChant");
                controlChant.EndChant(effectLife / totalLife, traverse.Field<float>("scaleDmg"));
                return false;
            } else
                Main.Log.LogError("SpellVessel.CompleteSpellEarlyWithScaling: ControlChant component not pressent on SpellVessel. Regressing to default behaviour.");


            return true;
        }
    }

    [HarmonyPatch(typeof(SpellVessel), "EndSpell")]
    class SpellVesselEndSpellPatch
    {
        static bool Prefix(SpellVessel __instance)
        {
            var twinSpell = __instance.GetComponent<TwinSpell>();
            var nextTarget = twinSpell?.NextTarget;
            if (nextTarget != null)
            {
                __instance.UseMana = false; // Don't want twinned spells to use mana on their repeats -- deducting that already on skill usage
                Traverse.Create(__instance).Field<Stats>("targ").Value = nextTarget.MyStats;
                __instance.ResolveEarly();
                return false;
            }

            return true;
        }
    }
}
