using HarmonyLib;
using UnityEngine;

namespace Arcanism.Patches
{
    /* Now that the chest type depends on the map used, guardians should be limited to the same range. */
    [HarmonyPatch(typeof(TreasureChestEvent), "SetGuardianStats")]
    public class TreasureChestEvent_SetGuardianStats
    {
        public static void Prefix(ref int attackerLevel)
        {
            var chest = SpellVessel_DoMiscSpells.TreasureHuntChest;
            if (chest == null) return;

            (int min, int max) levelClamp;
            if (chest == GameData.Misc.TreasureChest0_10)
                levelClamp = (1, 10);
            else if (chest == GameData.Misc.TreasureChest10_20)
                levelClamp = (11, 20);
            else if (chest == GameData.Misc.TreasureChest20_30)
                levelClamp = (21, 30);
            else
                levelClamp = (31, 35);

            attackerLevel = Mathf.Clamp(attackerLevel, levelClamp.min, levelClamp.max);
        }
    }
}
