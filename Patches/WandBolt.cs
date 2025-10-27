using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;

namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(WandBolt), "CalcDmgBonusWand")]
    public class WandBolt_CalcDmgBonusWand
    {
        static bool Prefix(WandBolt __instance, ref int __result, int _baseDamage, Character _caster)
        {
            // Main goal here was to remove the additive bonus on wand damage, because it compleeeeeetely negates the purpose of wand damage and attack speed
            // (because when your additive bonus is a few hundred, a weapon that does 1 damage every 1 second will always be better than one that does 600 every 3 seconds)
            // However by also making the dmg scale consistent with spells, it'll be nice and easy to tune their base damage according to appropriate spells of similar level.
            // And now, not every weapon has to be exactly the same aside from int/wis/cha bonus!
            __result = SpellVessel_CalcDmgBonus.CalculateSpellDamage(_caster, _baseDamage, false, false);
            return false;
        }
    }
}
