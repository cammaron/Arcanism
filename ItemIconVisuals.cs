using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism
{
    public class ItemIconVisuals : MonoBehaviour
    {


        public Image originalSparkler;
        public float backgroundScale = 1f;

        protected Image qualityBackground;
        protected Item lastItem;
        protected int lastQuantity;

        void OnDestroy()
        {
            Destroy(qualityBackground.gameObject);
        }

        public void SetItemAndQuantity(Item item, int quantity)
        {
            lastItem = item;
            lastQuantity = quantity;

            Blessing blessLevel;
            Quality quality;

            if (item == null || !item.IsUpgradeableEquipment())
            {
                blessLevel = Blessing.NONE;
                quality = Quality.NORMAL;
            } else
            {
                blessLevel = GetBlessLevel(quantity);
                quality = GetQualityLevel(quantity);
            }

            UpdateSparkler(blessLevel);
            UpdateQualityBackground(quality);            
        }

        protected void UpdateSparkler(Blessing blessLevel)
        {
            originalSparkler.gameObject.SetActive(blessLevel != Blessing.NONE);

            if (blessLevel == Blessing.BLESSED)
                originalSparkler.color = Color.cyan;
            else if (blessLevel == Blessing.GODLY)
                originalSparkler.color = Color.magenta;
        }

        void UpdateQualityBackground(Quality quality)
        {
            if (qualityBackground == null)
            {
                qualityBackground = new GameObject("QualityBG").AddComponent<Image>();
                qualityBackground.transform.SetParent(transform.parent);
                qualityBackground.transform.SetAsFirstSibling();
                qualityBackground.transform.localPosition = Vector3.zero;
                qualityBackground.rectTransform.sizeDelta = originalSparkler.rectTransform.sizeDelta;
            }

            qualityBackground.transform.localScale = new Vector3(backgroundScale, backgroundScale, backgroundScale);

            int alpha = quality == Quality.NORMAL ? 0 : 95;
            Color backgroundColor = GetQualityColor(quality);
            qualityBackground.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, alpha / 255f);
        }

        public int GetQuantity() => lastQuantity;
    }
}
