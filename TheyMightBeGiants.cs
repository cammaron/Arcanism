using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcanism
{
    // All up, this might be the single nerdiest class I've ever written.
    public class TheyMightBeGiants : MonoBehaviour
    {
        public enum State
        {
            QUANTUM_SUPERPOSITION, // also known as, "not sure what state we're in yet, have to come back alive first"
            NORMAL,
            GIANT,
            SUPERMASSIVE
        }

        protected NPC npc;
        protected Character character;
        protected bool waitingForAlive;
        protected string originalName;
        protected Vector3 originalScale;
        protected int originalBaseHp;
        protected State state;

        protected void Awake()
        {
            npc = GetComponent<NPC>();
            character = npc.GetChar();
            originalScale = transform.localScale;
            waitingForAlive = true;
            originalBaseHp = character.MyStats.BaseHP;
            originalName = npc.NPCName;
            state = State.QUANTUM_SUPERPOSITION;

            bool isAdventurer = character.MyStats.PlayerOrSimPlayer;
            bool isBoss = character.BossXp > 1f;
            bool hasDialogue = GetComponent<NPCDialogManager>() != null;
            bool isEnemy =
                character.MyFaction == Character.Faction.PreyAnimal ||
                character.MyFaction == Character.Faction.PredatorAnimal ||
                character.MyFaction == Character.Faction.EvilHuman ||
                character.MyFaction == Character.Faction.EvilGuard ||
                character.MyFaction == Character.Faction.Undead ||
                character.MyFaction == Character.Faction.OtherEvil;
            bool isOtherEnemyThatShouldntBeBig =
                npc.NPCName == "The Abomination" ||
                npc.NPCName == "Hand of the King";

            if (isAdventurer || isBoss || hasDialogue || !isEnemy || isOtherEnemyThatShouldntBeBig)
                Destroy(this); // easier to plop this logic in here so it destroys itself when non-viable 'cause there's more than 1 place I'll be checking+adding this component
        }

        protected void OnDestroy()
        {
            if (state == State.GIANT || state == State.SUPERMASSIVE)
                BecomeSmoll();
        }

        protected void Update()
        {
            // Basically observe state so that we only test whether to become giant once per lifetime
            bool isDead = npc.GetChar().IsDead();
            if (isDead)
                state = State.QUANTUM_SUPERPOSITION;

            else if (state == State.QUANTUM_SUPERPOSITION)
            {
                float roll = Random.Range(0f, 1f) * 100f;
                if (roll < 0.25f)
                    BecomeSupermassive();
                else if (roll < 2.5f)
                    BecomeGiant();
                else
                    BecomeSmoll();
            }
        }

        void SetName(string name)
        {
            SetName(null, name);
        }

        void SetName(string prefix, string name)
        {
            if (prefix != null)
            {
                if (originalName.StartsWith("A "))
                {
                    name = name.Substring(2);
                    prefix = "A " + prefix;
                }
                else if (originalName.StartsWith("An "))
                {
                    name = name.Substring(3);
                    prefix = "A " + prefix;
                }

                name = prefix + " " + name;
            }
            
            character.name = npc.name = npc.NPCName = character.MyStats.MyName = character.MyStats.name = name;
            npc.NamePlateTxt.text = name;
        }

        protected void UpdateSize(float scaleMod, float xpMod, float hpMod, float aggroMod)
        {
            transform.localScale = originalScale * scaleMod;
            character.MyAggro.transform.localScale = (Vector3.one * character.AggroRange * aggroMod) / scaleMod;
            npc.GetChar().BossXp = xpMod;

            character.MyStats.BaseHP = Mathf.RoundToInt(originalBaseHp * hpMod);
            character.MyStats.CalcStats();
            character.MyStats.CurrentHP = character.MyStats.CurrentMaxHP;
        }

        public void BecomeSupermassive()
        {
            state = State.SUPERMASSIVE;

            SetName("Supermassive", originalName);
            UpdateSize(2.6f, 30f, 24f, 1.7f);
        }

        public void BecomeGiant()
        {
            state = State.GIANT;
            SetName("Giant", originalName);
            UpdateSize(1.7f, 8f, 6f, 1.25f);
        }

        public void BecomeSmoll()
        {
            state = State.NORMAL;
            SetName(originalName);
            UpdateSize(1f, 0f, 1f, 1f);
        }

        public State GetState()
        {
            return this.state;
        }
    }
}
