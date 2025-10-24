using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism
{
    public static class ExtensionManager
    {
        public delegate ExtendedSkill GetOrCreateType(Character caster, SpellVessel vessel);
        private static Dictionary<string, GetOrCreateType> extensionCreatorBySkillId = new Dictionary<string, GetOrCreateType>();

        public static void AddExtension(Skill skill, GetOrCreateType creator)
        {
            extensionCreatorBySkillId.Add(skill.Id, creator);
        }

        public static bool GetExtension(Skill skill, Character caster, SpellVessel vessel, out ExtendedSkill extension)
        {
            if (extensionCreatorBySkillId.TryGetValue(skill.Id, out var creator))
            {
                extension = creator(caster, vessel);
                return true;
            }
            extension = null;
            return false;
        }

    }
}
