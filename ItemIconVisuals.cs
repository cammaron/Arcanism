using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Arcanism.Patches.ItemExtensions;

namespace Arcanism
{
    class ItemIconVisuals : MonoBehaviour
    {

        public static Color JUNK_COLOR = new Color32(187, 187, 187, 255);
        public static Color SUPERIOR_COLOR = new Color32(0, 150, 255, 255);
        public static Color MASTERWORK_COLOR = new Color32(151, 255, 0, 255);

        public Image originalSparkler;
        public float backgroundScale = 1f;

        protected Image qualityBackground;
        protected Item lastItem;
        protected int lastQuantity;

        void OnDestroy()
        {
            qualityBackground.enabled = false; // this should be redundant but i'm just testing 'cause it seems like the backgrounds are hanging around during hot reloads sometimes
            Destroy(qualityBackground.gameObject);
        }

        public void SetItemAndQuantity(Item item, int quantity)
        {
            lastItem = item;
            lastQuantity = quantity;

            Blessing blessLevel;
            Quality quality;

            if (item == null || item.RequiredSlot == Item.SlotType.General || item.RequiredSlot == Item.SlotType.Aura)
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
            { originalSparkler.color = Color.cyan; }

            else if (blessLevel == Blessing.GODLY)
                originalSparkler.color = Color.magenta;
        }

        void UpdateQualityBackground(Quality quality)
        {
            if (qualityBackground == null)
            {
                qualityBackground = new GameObject("QualityBG").AddComponent<Image>();// Instantiate(originalSparkler, transform.parent);
                qualityBackground.transform.SetParent(transform.parent);
                qualityBackground.transform.SetAsFirstSibling();
                qualityBackground.transform.localPosition = Vector3.zero;
                qualityBackground.rectTransform.sizeDelta = originalSparkler.rectTransform.sizeDelta;
            }

            qualityBackground.transform.localScale = new Vector3(backgroundScale, backgroundScale, backgroundScale);

            byte alpha = 45;
            switch (quality)
            {
                case Quality.JUNK:
                    qualityBackground.color = JUNK_COLOR;
                    break;
                case Quality.NORMAL:
                    alpha = 0;
                    qualityBackground.color = Color.black;
                    break;
                case Quality.SUPERIOR:
                    qualityBackground.color = SUPERIOR_COLOR;
                    break;
                case Quality.MASTERWORK:
                    qualityBackground.color = MASTERWORK_COLOR;
                    break;
            }
            qualityBackground.color = new Color(qualityBackground.color.r, qualityBackground.color.g, qualityBackground.color.b, alpha / 255f);
        }

        public int GetQuantity() => lastQuantity;
    }
}
