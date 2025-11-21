using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanism.SkillExtension
{
    interface ISpellRetargetingSkill
    {
        IEnumerable<Character> GetAllTargets();
        Character GetNextTarget();

        bool AllowResonatingOnCurrentTarget();
    }
}
