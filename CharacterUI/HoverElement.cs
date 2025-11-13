using UnityEngine;

namespace Arcanism.CharacterUI
{
    public struct StatsParam
    {
        public readonly Stats stats;

        public StatsParam(Stats stats)
        {
            this.stats = stats;
        }
    }

    public abstract class HoverElement<Params> : MonoBehaviour
    {
        protected Params args;

        public void Initialise(Params args)
        {
            this.args = args;
            _Init();
        }
        
        protected abstract void _Init();
    }
}
