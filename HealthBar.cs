using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    class HealthBar : MonoBehaviour
    {
        Stats stats;
        
        SpriteRenderer barBackground;
        SpriteRenderer bar;
        SpriteRenderer barTarget;

        float barFullSize;

        void Awake()
        {
            stats = GetComponentInParent<Stats>();

            barTarget = new GameObject("TargetBacking").AddComponent<SpriteRenderer>();
            barTarget.sprite = Main.miscSpritesByName["bar_target"];
            barTarget.color = new Color32(170, 155, 45, 255);
            barTarget.transform.SetParent(transform);
            barTarget.transform.localPosition = Vector3.zero;

            barBackground = new GameObject("Background").AddComponent<SpriteRenderer>();
            barBackground.sprite = Main.miscSpritesByName["bar_background"];
            barBackground.transform.SetParent(transform);
            barBackground.transform.localPosition = Vector3.zero;

            bar = new GameObject("Bar").AddComponent<SpriteRenderer>();
            bar.sprite = Main.miscSpritesByName["bar_foreground"];
            bar.drawMode = SpriteDrawMode.Sliced;
            bar.transform.SetParent(barBackground.transform);
            bar.transform.localPosition = Vector3.zero;
            bar.transform.rotation = Quaternion.identity;
            barFullSize = bar.size.x;

            barTarget.sortingOrder = barBackground.sortingOrder - 1;
            bar.sortingOrder = barBackground.sortingOrder + 1;

            /* Below is an experimental tweak to make the health bar appear vertically alongside enemies instead of above. It's a bit easier to see in groups but also weird.
            transform.localPosition -= new Vector3(0.15f, .15f, 0);
            var capsule = GetComponentInParent<CapsuleCollider>();
            transform.position += new Vector3(capsule.radius * capsule.transform.localScale.x + 0.25f, -(capsule.height * capsule.transform.localScale.y * 0.4f + 0f), 0);
            transform.Rotate(new Vector3(0, 0, 270));*/

            

        }

        void Update()
        {
            bool isTargeted = stats.Myself.TargetRing.activeSelf;
            bar.enabled = barBackground.enabled = isTargeted || ShouldShow();
            bar.size = new Vector2(barFullSize * GetHpFactor(), bar.size.y);
            bar.transform.localPosition = new Vector3(barFullSize * (1 - GetHpFactor()) * 0.5f, 0, 0);

            barTarget.enabled = isTargeted;
            if (barTarget.enabled)
            {
                if (GameData.Autoattacking)
                    barTarget.color = Color.white;
                else
                    barTarget.color = Color.black;
            }
        }

        bool ShouldShow()
        {
            float distanceFromPlayer = Vector3.Distance(transform.position, GameData.PlayerControl.Myself.transform.position);
            return distanceFromPlayer < 80 && !stats.Myself.IsDead() && stats.GetCurrentHP() < stats.CurrentMaxHP;
        }

        float GetHpFactor()
        {
            return stats.GetCurrentHP() / (float) stats.CurrentMaxHP;
        }
    }
}
