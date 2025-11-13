using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Arcanism.CharacterUI
{
    public class HealthBar : HoverElement<StatsParam>  
    {
        protected SpriteRenderer barBackground;
        protected SpriteRenderer barShrinkDelayBackground;
        protected SpriteRenderer bar;

        protected float barFullSize;

        
        protected override void _Init()
        {
            barBackground = new GameObject("Background").AddComponent<SpriteRenderer>();
            barBackground.sprite = Main.miscSpritesByName["bar"];
            barBackground.color = new Color32(50, 50, 50, 255);
            barBackground.transform.SetParent(transform);
            barBackground.transform.localPosition = Vector3.zero;

            barShrinkDelayBackground = new GameObject("Bar").AddComponent<SpriteRenderer>();
            barShrinkDelayBackground.sprite = barBackground.sprite;
            barShrinkDelayBackground.color = new Color32(255, 255, 255, 255);
            barShrinkDelayBackground.drawMode = SpriteDrawMode.Sliced;
            barShrinkDelayBackground.transform.SetParent(barBackground.transform);
            barShrinkDelayBackground.transform.localPosition = Vector3.zero;
            barShrinkDelayBackground.transform.rotation = Quaternion.identity;

            bar = new GameObject("Bar").AddComponent<SpriteRenderer>();
            bar.sprite = barBackground.sprite;
            bar.color = new Color32(190, 100, 100, 255);
            bar.drawMode = SpriteDrawMode.Sliced;
            bar.transform.SetParent(barBackground.transform);
            bar.transform.localPosition = Vector3.zero;
            bar.transform.rotation = Quaternion.identity;
            barFullSize = bar.size.x;

            barShrinkDelayBackground.sortingOrder = barBackground.sortingOrder + 1;
            bar.sortingOrder = barShrinkDelayBackground.sortingOrder + 1;

            /* Below is an experimental tweak to make the health bar appear vertically alongside enemies instead of above. It's a bit easier to see in groups but also weird.
            transform.localPosition -= new Vector3(0.15f, .15f, 0);
            var capsule = GetComponentInParent<CapsuleCollider>();
            transform.position += new Vector3(capsule.radius * capsule.transform.localScale.x + 0.25f, -(capsule.height * capsule.transform.localScale.y * 0.4f + 0f), 0);
            transform.Rotate(new Vector3(0, 0, 270));*/
        }

        protected void Update()
        {
            bar.size = new Vector2(barFullSize * GetHpFactor(), bar.size.y);
            bar.transform.localPosition = new Vector3(barFullSize * (1 - GetHpFactor()) * 0.5f, 0, 0);

            float targetScale = 1;
            float maxScale = 1.3f;
            float currentScale = barBackground.transform.localScale.x;

            if (bar.size.x > barShrinkDelayBackground.size.x)
            {
                barShrinkDelayBackground.size = new Vector2(bar.size.x, bar.size.y);
                barShrinkDelayBackground.transform.localPosition = bar.transform.localPosition;
            }
            else
            {
                float sizeDiffSpeedFactor = 1f + ((barShrinkDelayBackground.size.x / bar.size.x) - 1f) * 0.5f;
                float maxChange = (barFullSize * 0.125f * Time.deltaTime) * sizeDiffSpeedFactor;
                barShrinkDelayBackground.size = new Vector2(Mathf.Max(barShrinkDelayBackground.size.x - maxChange, bar.size.x), bar.size.y);
                float barShrinkSizeFactor = barShrinkDelayBackground.size.x / barFullSize;
                barShrinkDelayBackground.transform.localPosition = new Vector3(barFullSize * (1 - barShrinkSizeFactor) * 0.5f, 0, 0);

                if (barShrinkDelayBackground.size.x > bar.size.x)
                    targetScale = maxScale;
            }

            if (currentScale != targetScale && bar.enabled) // This scaling is not related to the amount of health, but just a bouncy animation whenever damage is dealt
            {
                float scaleChange = targetScale - currentScale;
                float maxScaleChange = (maxScale - 1f) * Time.deltaTime / .125f;
                float newScale = Mathf.Clamp(currentScale + scaleChange, currentScale - maxScaleChange, currentScale + maxScaleChange);
                barBackground.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }


        protected float GetHpFactor()
        {
            return args.stats.GetCurrentHP() / (float)args.stats.CurrentMaxHP;
        }


    }
}
