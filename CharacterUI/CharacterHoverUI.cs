using System;
using System.Collections.Generic;
using UnityEngine;
using BigDamage;
using TMPro;

namespace Arcanism.CharacterUI
{
    /* An element attached directly to the Character GameObject which manages the floating visuals that appear on them (like health bar).
     * The visuals themselves are NOT children of this; that's because TheyMightBeGiants changed root GameObject scale, thus would scale this UI up.
     * Instead, they are nested in a separate root Transform (just so they don't crowd the hierarchy) and are positioned relative to this component 
     * every Update.
     */
    public class CharacterHoverUI : MonoBehaviour
    {
        public static readonly Color32 INDICATOR_COLOR = new Color32(255, 204, 90, 255);

        protected Transform anchor;
        protected ScaleWithDistance scaler;
        protected Character character;
        
        public HealthBar HealthBar { get; private set; }
        public TargetIndicator TargetIndicator { get; private set; }
        public SpellTargetIndicator SpellTargetIndicator { get; private set; }
        public LevelDisplay LevelDisplay { get; private set; }
        public CustomNamePlate NamePlate { get; private set; }

        protected static Transform globalAnchorRoot;

        protected void Awake()
        {
            character = GetComponent<Character>();

            if (globalAnchorRoot == null)
                globalAnchorRoot = AddTransform(null, "ArcanismHoverUI");

            anchor = AddTransform(globalAnchorRoot, character.MyNPC?.NPCName + " Anchor");
            anchor.rotation = Quaternion.identity;

            scaler = anchor.gameObject.AddComponent<ScaleWithDistance>();
            scaler.maxScale = 5f;
            
            anchor.gameObject.AddComponent<FaceCamera>();

            //namePlateOrigLocalPos + (Vector3.up * .22f * scaler.transform.localScale.x);

            Vector3 namePlatePos = Vector3.up * .5f;
            Vector3 iconScale = new Vector3(.45f, .45f, 1f);

            var statsParam = new StatsParam(character.MyStats);
            
            HealthBar =             AddElementWithParams<HealthBar,       StatsParam>       (anchor, "HealthBar",            statsParam,     Vector3.zero,                      new Vector3(1f, 1.5f, 1f));
            NamePlate =             AddElementWithParams<CustomNamePlate, StatsParam>       (anchor, "NamePlate",            statsParam,     namePlatePos);
            var levelParams = new LevelDisplayParams(character.MyStats, NamePlate.NewNameplate);
            LevelDisplay =          AddElementWithParams<LevelDisplay, LevelDisplayParams>  (anchor, "LevelDisplay",         levelParams,    namePlatePos + Vector3.up * .2f);
            TargetIndicator =       AddElementWithParams<TargetIndicator,StatsParam>        (anchor, "TargetIndicator",      statsParam,     namePlatePos + Vector3.up * .9f,  iconScale);
            SpellTargetIndicator =  AddElement         <SpellTargetIndicator>               (anchor, "SpellTargetIndicator",                 namePlatePos + Vector3.up * 1.55f, iconScale);
        }

        protected void Update()
        {
            anchor.position = transform.position + new Vector3(0f, GetComponent<CapsuleCollider>().height * transform.localScale.y + 0f, 0f) + Vector3.up * 0.15f;

            bool showFullUi = TargetIndicator.IsTargetedByPlayer() || SpellTargetIndicator.IsSpellTarget() || IsEngagedInCombatWithPlayer();
            bool showNameplate = showFullUi || ShouldShowNpcName();
            HealthBar.gameObject.SetActive(showFullUi);
            LevelDisplay.gameObject.SetActive(showFullUi);
            NamePlate.NewNameplate.enabled = showFullUi || showNameplate;
            scaler.enabled = showFullUi; // scaler.OnDisable also resets scale to 1
        }

        protected void OnDestroy()
        {
            if (anchor != null)
                Destroy(anchor.gameObject);
        }

        T AddElement<T>(Transform parent, string name, Vector3 offset, Vector3? scale = null) where T : Component
        {
            var t = AddTransform(parent, name);
            t.localPosition = offset;
            if (scale.HasValue) t.localScale = scale.Value;
            return t.gameObject.AddComponent<T>();
        }

        T AddElementWithParams<T,Args>(Transform parent, string name, Args args, Vector3 offset, Vector3? scale = null) where T : HoverElement<Args>
        {
            var t = AddTransform(parent, name);
            if (scale.HasValue) t.localScale = scale.Value;
            var c = t.gameObject.AddComponent<T>();
            c.Initialise(args);
            t.localPosition = offset;
            return c;
        }

        Transform AddTransform(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            return obj.transform;
        }

        protected bool IsEngagedInCombatWithPlayer()
        {
            if (character.MyNPC?.CurrentAggroTarget == null
            || character.IsDead()
            || character.MyStats.GetCurrentHP() >= character.MyStats.CurrentMaxHP
            || Vector3.Distance(transform.position, GameData.PlayerControl.Myself.transform.position) > 80
            )
                return false; // Only show my UI if I'm injured, within range, currently attacking someone, and...


            if (character.MyNPC.CurrentAggroTarget == GameData.PlayerControl.Myself) // I'm attacking the player
                return true;

            foreach (var member in GameData.GroupMembers)
            {
                if (member?.MyStats?.Myself != null)
                {
                    if (member.MyStats.Myself == character.MyNPC.CurrentAggroTarget) // or I'm attacking a party member
                        return true;

                    if (character.MyNPC.SimPlayer && member.MyStats.Myself == character) // or I *AM* a party member
                        return true;
                }
            }

            return false;
        }

        protected bool ShouldShowNpcName()
        {
            return (character.MyNPC?.NamePlateTxt.enabled).GetValueOrDefault(false);
        }
    }
}
