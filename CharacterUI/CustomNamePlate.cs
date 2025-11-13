using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BigDamage;

namespace Arcanism.CharacterUI
{
    public class CustomNamePlate : HoverElement<StatsParam>
    {
        protected TextMeshPro ogNameplate;
        public TextMeshPro NewNameplate { get; private set; }

        protected override void _Init()
        {
            if (args.stats.Myself.MyNPC != null)
            {
                ogNameplate = args.stats.Myself.MyNPC.NamePlateTxt;

                NewNameplate = Instantiate(ogNameplate, transform);
                NewNameplate.transform.DestroyIfPresent<NamePlate>(); // the vanilla NamePlate class just handles the camera facing logic, which I'm doing separately (see FaceCamera component on CharacterHoverUI)
                NewNameplate.transform.DestroyIfPresent<FlashUIColors>(); // as for this, I'm just going to leverage the existing og nameplate's colour
                NewNameplate.transform.DestroyAllChildren();

                NewNameplate.transform.localPosition = Vector3.zero;
                NewNameplate.transform.localRotation = Quaternion.identity;
                NewNameplate.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        protected void Update()
        {
            if (NewNameplate != null)
            {
                ogNameplate.color = new Color(ogNameplate.color.r, ogNameplate.color.g, ogNameplate.color.b, 0);
                NewNameplate.color = new Color(ogNameplate.color.r, ogNameplate.color.g, ogNameplate.color.b, 1f);
                NewNameplate.text = ogNameplate.text;
            }
        }
    }
}
