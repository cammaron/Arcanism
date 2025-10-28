using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    class CharacterHoverUI : MonoBehaviour
    {
        Transform anchor;
        Character character;
        HealthBar healthBar;
        SpellTargetIndicator targetIndicator;

        void Awake()
        {
            character = GetComponent<Character>();

            anchor = AddTransform(transform, "Anchor");
            anchor.position = transform.position + new Vector3(0f, base.GetComponent<CapsuleCollider>().height * base.transform.localScale.y + 0.65f, 0f);
            anchor.rotation = Quaternion.identity;

            anchor.gameObject.AddComponent<FaceCamera>();

            healthBar = AddElement<HealthBar>(anchor, "HealthBar");
            targetIndicator = AddElement<SpellTargetIndicator>(anchor, "SpellTargetIndicator");
        }

        void Update()
        {
            anchor.position = transform.position + new Vector3(0f, GetComponent<CapsuleCollider>().height * transform.localScale.y + 0f, 0f);
        }

        void OnDestroy()
        {
            Destroy(anchor.gameObject);
        }

        T AddElement<T>(Transform parent, string name) where T : Component
        {
            return AddTransform(parent, name).gameObject.AddComponent<T>();
        }

        Transform AddTransform(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            return obj.transform;
        }
    }
}
