using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BigDamage;

namespace Arcanism.CharacterUI
{
    public struct LevelDisplayParams
    {
        public readonly Stats stats;
        public readonly TextMeshPro basedOn;

        public LevelDisplayParams(Stats stats, TextMeshPro basedOn)
        {
            this.stats = stats;
            this.basedOn = basedOn;
        }
    }

    public class LevelDisplay : HoverElement<LevelDisplayParams>
    {

        protected TextMeshPro levelText;

        protected override void _Init() // The reason I'm basing it off existing NamePlate is because it has some property, likely on TextMeshPro, that I was missing in my own version and couldn't find what it was (prevented my text from rendering)
        {
            levelText = Instantiate(args.basedOn, transform); 
            levelText.transform.localPosition = Vector3.zero;
            levelText.transform.localRotation = Quaternion.identity;
            levelText.transform.localScale = new Vector3(-1f, 1, 1);
            levelText.rectTransform.sizeDelta = new Vector2(.6f, .01f);
            levelText.outlineColor = Color.black;
            levelText.outlineWidth = 0.25f;
            levelText.fontWeight = FontWeight.Bold;
            levelText.fontSize = 3.5f;
            levelText.verticalAlignment = VerticalAlignmentOptions.Bottom;
        }
        
        protected void Update()
        {
            if (levelText != null)
            {
                levelText.color = args.stats.Myself.TargetRing.GetComponent<ParticleSystem>().main.startColor.color;
                levelText.text = args.stats.Level.ToString();
            }
        }

        protected void DestroyIfPresent<T>(Transform parent) where T : MonoBehaviour
        {
            var t = parent.GetComponent<T>();
            if (t != null)
                Destroy(t);
        }
    }
}
