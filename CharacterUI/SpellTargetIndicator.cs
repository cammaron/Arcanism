using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.CharacterUI
{
    public class SpellTargetIndicator : MonoBehaviour
    {
        protected readonly HashSet<SpellVessel> targetingMe = new HashSet<SpellVessel>();
        protected SpriteRenderer indicator;

        protected void Start()
        {
            indicator = gameObject.AddComponent<SpriteRenderer>();
            indicator.sprite = Main.miscSpritesByName["target_indicator"];
            indicator.color = CharacterHoverUI.INDICATOR_COLOR;
        }

        public void AddTargetSource(SpellVessel v)
        {
            targetingMe.Add(v);
        }

        public void RemoveTargetSource(SpellVessel v)
        {
            targetingMe.Remove(v);
        }

        protected void Update()
        {
            targetingMe.RemoveWhere(v => v == null);
            indicator.enabled = IsSpellTarget();
        }

        public bool IsSpellTarget()
        {
            return targetingMe.Count > 0;
        }
    }
}
