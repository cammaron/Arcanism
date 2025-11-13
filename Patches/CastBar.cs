using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using TMPro;

namespace Arcanism.Patches
{
    [HarmonyPatch(typeof(CastBar), nameof(CastBar.NewCast))]
    public class CastBar_NewCast
    {
        public static void Postfix(CastBar __instance)
        {
            __instance.GetSubtext().text = "";
        }
    }

    public static class CastBarExtensions
    {
        public const string SUBTEXT_NAME = "Subtext";

        public static void SetSubtext(this CastBar cb, string left, string right)
        {
            var subtext = cb.GetSubtext();
            subtext.transform.localPosition = Vector3.down * 20f;
            subtext.rectTransform.sizeDelta = new Vector2(275, 10f);
            subtext.fontSize = 15;
            subtext.fontWeight = TMPro.FontWeight.Black;
            subtext.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Flush;
            subtext.text = $"<align=left>{left}</align><line-height=0>\r\n<align=right>{right}</align></line-height>";
        }

        public static TextMeshProUGUI GetSubtext(this CastBar cb)
        {
            foreach(var txt in cb.castBar.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txt.gameObject.name.StartsWith(SUBTEXT_NAME))
                    return txt;
            }

            var subtext = GameObject.Instantiate(cb.OCTxt, cb.castBar.transform);
            subtext.gameObject.name = SUBTEXT_NAME;
            subtext.text = "";
            subtext.fontSize = 24;
            subtext.transform.localPosition += Vector3.down * .5f;
            
            return subtext;
        }

    }
}
