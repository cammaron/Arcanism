using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.CharacterUI
{
    public class TargetIndicator : HoverElement<StatsParam>
    {
        protected static readonly Color32 COLOR = new Color32(180, 180, 180, 255);
        protected static readonly Color32 MAIN_ASSIST_COLOR = new Color32(80, 80, 80, 255);

        protected SpriteRenderer indicator;

        protected override void _Init()
        {
            indicator = gameObject.AddComponent<SpriteRenderer>();
            indicator.enabled = false;
            indicator.sprite = Main.miscSpritesByName["triangleman"];
            indicator.color = COLOR;
        }

        protected void Update()
        {
            if (IsTargetedByPlayer())
            {
                indicator.enabled = true;
                if (GameData.Autoattacking)
                    indicator.color = CharacterHoverUI.INDICATOR_COLOR;
                else
                    indicator.color = COLOR;
            } else if (IsTargetedByMainAssist())
            {
                indicator.enabled = true;
                indicator.color = MAIN_ASSIST_COLOR;
            } else
            {
                indicator.enabled = false;
            }
            
        }

        public bool IsTargetedByPlayer()
        {
            return GameData.PlayerControl.CurrentTarget == args.stats.Myself; // return stats.Myself.TargetRing.activeSelf;
        }

        public bool IsTargetedByMainAssist()
        {
            var target = GameData.SimPlayerGrouping.MainAssist?.MyAvatar?.MyStats?.Myself?.MyNPC?.CurrentAggroTarget;

            return (target == args.stats.Myself);
        }
    }
}
