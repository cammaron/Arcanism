using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    class SpellTargetIndicator : MonoBehaviour
    {
        protected static readonly Color32 INDICATOR_COLOR = new Color32(255, 204, 90, 255);

        HashSet<SpellVessel> targetingMe = new HashSet<SpellVessel>();
        SpriteRenderer indicator;

        void Awake()
        {
            indicator = new GameObject("Icon").AddComponent<SpriteRenderer>();
            indicator.gameObject.SetActive(false);
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = transform.localPosition + Vector3.up * .8f;
            indicator.sprite = Main.miscSpritesByName["target_indicator"];
            indicator.color = INDICATOR_COLOR;
            indicator.transform.rotation = Quaternion.identity;
            indicator.transform.localScale = new Vector3(0.45f, 0.45f, 1);
        }

        public void AddTargetSource(SpellVessel v)
        {
            targetingMe.Add(v);
        }

        public void RemoveTargetSource(SpellVessel v)
        {
            targetingMe.Remove(v);
        }

        void Update()
        {
            targetingMe.RemoveWhere(v => v == null);
            indicator.gameObject.SetActive(targetingMe.Count > 0);
        }
    }
}
