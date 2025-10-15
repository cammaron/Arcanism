using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    interface HotkeyAbility
    {
        bool IsPlayer{ get; }
        void ApplyCooldown(float cooldownFactor = 1f);
    }

    
}
