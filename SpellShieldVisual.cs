using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    public class SpellShieldVisual : MonoBehaviour
    {
        protected static Shader shieldShader;

        public Stats stats;
        protected MeshRenderer shield;
        public Material material;

        int fullCapacity = 0;
        int lastShieldAmount = 0;
        float flashEndTime = 0f;
        float shatterEndTime = 0f;

        Vector3 baseScale = new Vector3(2f, 2f, 2f);
        Color32 baseColor = new Color32(255, 0, 255, 255);
        Color32 currentDamagedColor;
        float currentOpacity;
        AudioClip baseHurtSound;

        protected void Start()
        {
            Init();
        }

        public void Init()
        {
            if (shieldShader == null)
                shieldShader = Shader.Find("Toon/TFF_ToonWater"); // /VegetationShader

            if (shield != null) return;

            shield = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshRenderer>();
            // remove SphereCollider if present
            var collider = shield.GetComponent<SphereCollider>();
            if (collider != null)
                Destroy(collider);

            baseHurtSound = stats.Myself.MyHurtSound;

            shield.transform.SetParent(transform);
            shield.transform.localPosition = Vector3.up;
            shield.transform.localScale = baseScale;
            shield.enabled = false;
            shield.material.shader = shieldShader;
            shield.material.SetFloat("_OpacityDepth", 10000f);
            shield.material.SetFloat("_ShallowColorDepth", 2f);
            shield.material.SetFloat("_WaveSpeed", 0f);
            shield.material.SetFloat("_FoamScale", 1f);
            shield.material.SetFloat("_FoamDistance", 2f);
            shield.material.SetFloat("_FoamSpeed", .01f);
            shield.material.SetTexture("_NoiseTexture", Main.miscSpritesByName["water"].texture);
            this.material = shield.material;
        }

        void UpdateColor(Color32 c1, float opacity = .3f)
        {
            shield.material.SetColor("_ShallowColor", c1);
            shield.material.SetColor("_DeepColor", c1);
            shield.material.SetFloat("_Opacity", opacity);
            shield.material.SetFloat("_FoamOpacity", .05f + (1f-opacity)/10f);
        }

        protected void OnDestroy()
        {
            stats.Myself.MyHurtSound = baseHurtSound;
            Destroy(shield.gameObject);
        }

        protected void Update()
        {
            float shatterDuration = 0.125f;
            float flashDuration = 0.25f;

            if (stats.SpellShield == 0)
            {
                if (lastShieldAmount > 0)
                {
                    // Then we've just broken or effect giving shield has ended!
                    stats.Myself.MyAudio.PlayOneShot(Main.sfxByName["shield-shatter"], GameData.SFXVol * 1f);
                    shatterEndTime = Time.time + shatterDuration;
                    stats.Myself.MyHurtSound = baseHurtSound;
                }

                if (shatterEndTime > Time.time)
                {
                    float shatterProgresss = (shatterDuration - (shatterEndTime - Time.time)) / shatterDuration;
                    if (shatterProgresss > 0)
                    {
                        UpdateColor(Color.white, Mathf.Lerp(1f, 0f, shatterProgresss));
                        shield.transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.4f, shatterProgresss);
                    }
                } else
                {
                    shield.enabled = false;
                }
            } 
            else
            {
                if (lastShieldAmount == 0) // Turning on
                {
                    shield.enabled = true;
                    shield.transform.localScale = baseScale;
                    fullCapacity = stats.SpellShield;
                    shatterEndTime = 0f;
                    stats.Myself.MyHurtSound = Main.sfxByName["shield-hit-medium"];
                }

                if (stats.SpellShield != lastShieldAmount) // deliberately also playing this animation on turning on
                {
                    flashEndTime = Time.time + flashDuration; // one flash may interrupt another

                    if (stats.SpellShield < fullCapacity * .35f)
                        stats.Myself.MyHurtSound = Main.sfxByName["shield-hit-hard"];

                    float reductionFactor = 1f - ((float)stats.SpellShield / fullCapacity);
                    currentDamagedColor = Color.Lerp(baseColor, Color.red, reductionFactor);
                    currentOpacity = Mathf.Lerp(.3f, .1f, reductionFactor);
                }

                if (flashEndTime > Time.time)
                {
                    float flashProgress = (flashDuration - (flashEndTime - Time.time)) / flashDuration;
                    UpdateColor(Color.white, 0.9f);
                    if (flashProgress <= 0.5f)
                        shield.transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.1f, flashProgress / 0.5f);
                    else
                        shield.transform.localScale = Vector3.Lerp(baseScale * 1.1f, baseScale, (flashProgress - 0.5f) / 0.5f);
                } else 
                    UpdateColor(currentDamagedColor, currentOpacity);
            }

            lastShieldAmount = stats.SpellShield;
        }

    }
}
