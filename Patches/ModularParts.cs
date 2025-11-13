using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(ModularParts), nameof(ModularParts.UpdateSlot))]
    public class ModularParts_UpdateSlot
    {
        public static void Prefix(ref int _quality)
        {
            _quality = ItemExtensions.ToOriginalBlessLevel(_quality);
        }
    }
}
