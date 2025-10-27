using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Arcanism.SkillExtension;

namespace Arcanism.Patches
{
    /* redoes the CalculateCharismaModifier method, buffing it so it's useful and takes into account charisma proficiency */
    [HarmonyPatch(typeof(SpellVessel), nameof(SpellVessel.CreateSpellChargeEffect))]
    class SpellVessel_CreateSpellChargeEffect
    {
        static float CalculateResistModifier(CastSpell source)
        {
            if (source.isPlayer || (source.MyChar.MyNPC != null && source.MyChar.MyNPC.SimPlayer))
                return source.MyChar.MyStats.GetCurrentCha() * (source.MyChar.MyStats.ChaScaleMod / 100f) * .64f;
            return 0f;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var calcCharismaModCall = CodeInstruction.Call(typeof(SpellVessel), nameof(SpellVessel.CalculateCharismaModifier), new System.Type[] { typeof(int), typeof(int) });
            return new CodeMatcher(instructions)
                .MatchStartForward(calcCharismaModCall)
                .MatchStartBackwards(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.LoadField(typeof(SpellVessel), "SpellSource"),
                    CodeInstruction.LoadField(typeof(CastSpell), nameof(CastSpell.MyChar)),
                    CodeInstruction.LoadField(typeof(Character), nameof(Character.MyStats)),
                    CodeInstruction.LoadField(typeof(Stats), nameof(Stats.Level))
                )
                .RemoveInstruction() // only need *one* of those "this" references now that we're calling a non-local calc method
                .Advance(2) // to the MyChar line, from which point we'll start removing stuff -- wanna keep the SpellSource field to pass it as an argument to our own method
                .RemovesLinesUntilMatchEndForward(calcCharismaModCall)
                .InsertAndAdvance(CodeInstruction.Call(() => CalculateResistModifier(default)))
                .Instructions();
        }
    }

    /* Replaces the call that calculates bonus damage for spell DoTs -- it was entirely additive and inconsistent with the other new damage formula */
    [HarmonyPatch(typeof(SpellVessel), "ResolveSpell")]
    class SpellVessel_ResolveSpell
    {
        static int CalculateDamageOverTimeDamageBonus(CastSpell source, SpellVessel vessel)
        {
            return SpellVessel_CalcDmgBonus.CalculateSpellDamage(source, vessel.spell.TargetDamage, false) - vessel.spell.TargetDamage; // we're actually just returning the ADDITIONAL damage on top of base, which will get added back in elsewhere
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(
                    new CodeInstruction(OpCodes.Ldarg_0), /* Look for: spell source my char stats .level / 4 */
                    CodeInstruction.LoadField(typeof(SpellVessel), "SpellSource"), 
                    CodeInstruction.LoadField(typeof(CastSpell), nameof(CastSpell.MyChar)),
                    CodeInstruction.LoadField(typeof(Character), nameof(Character.MyStats)),
                    CodeInstruction.LoadField(typeof(Stats), nameof(Stats.Level)),
                    new CodeInstruction(OpCodes.Ldc_I4_4),
                    new CodeInstruction(OpCodes.Div))
                .Advance(2) // keeping 'this.SpellSource' to pass in as an arg
                .RemovesLinesUntilMatchEndForward(CodeInstruction.Call(typeof(Mathf), nameof(Mathf.RoundToInt), new System.Type[] { typeof(float) }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)) // also straight up want to pass 'this' SpellVessel as an arg
                .InsertAndAdvance(CodeInstruction.Call(() => CalculateDamageOverTimeDamageBonus(default, default)))
                .Instructions();
        }

        static bool Prefix(SpellVessel __instance, CastSpell ___SpellSource, ref float ___scaleDmg)
        {
            var damageModifier = ___SpellSource.MyChar.GetComponent<ISpellDamageModifier>();
            if (damageModifier != null)
                ___scaleDmg = damageModifier.GetSpellDamageMulti();
            
            if (___scaleDmg == 0)
            {
                Traverse.Create(__instance).Method("EndSpell").GetValue();
                return false;
            }

            return true;
        }
    }

    /* Rewrites spell damage formula to have a lower additive component and higher multiplicative component to balance it out, resulting in something *fairly* similar to the old values,
     * but with lower values for weaker spells when used later in the game.
     * Also makes Roaring Echoes have an exponential power gain (see comments)
     */
    [HarmonyPatch(typeof(SpellVessel), "CalcDmgBonus")]
    class SpellVessel_CalcDmgBonus
    {
        private const string ROARING_ECHOES_ASCENSION_ID = "32723648";

        static float ExponentialDamageScale(float baseDamage, int res, int resRangeStart, int resRangeEnd, float exponentPerResInRange)
        {
            int resInRange;
            if (resRangeEnd <= resRangeStart)
                resInRange = Mathf.Max(0, res - resRangeStart);
            else
                resInRange = Mathf.Clamp(res - resRangeStart, 0, resRangeEnd - resRangeStart);
            return baseDamage * Mathf.Pow(exponentPerResInRange, resInRange);
        }

        public static int CalculateSpellDamage(CastSpell source, int baseDamage, bool resonating)
        {
            return CalculateSpellDamage(source.MyChar, baseDamage, resonating);
        }
        public static int CalculateSpellDamage(Character source, int baseDamage, bool resonating, bool includeFlatAddition = true)
        {
            float fullDamage = baseDamage;
            if (resonating)
            {
                var roaringEchoesRank = source.MySkills.GetAscensionRank(ROARING_ECHOES_ASCENSION_ID);
                int res = source.MyStats.GetCurrentRes();
                if (res > 100 && roaringEchoesRank > 0 && Random.Range(0, 100) < roaringEchoesRank * 30)
                {
                    if (Vector3.Distance(source.transform.position, GameData.PlayerControl.transform.position) < 50f) // text range increased from vanilla 15
                        UpdateSocialLog.CombatLogAdd("A ROARING ECHO courses through the air");

                    // Vanilla, roaring echo does full base spell damage + 1% per res over 100.
                    fullDamage *= 0.45f; // Meet in the middle with a higher base damage than a standard resonance so it's still immediately rewarding...
                    fullDamage = ExponentialDamageScale(fullDamage, res, 100, 104, 1.21f); // but with lots of scaling for the first few res over 100, 
                    fullDamage = ExponentialDamageScale(fullDamage, res, 104, 108, 1.075f); // and a decreasing but still exponential returns for climbing higher
                    fullDamage = ExponentialDamageScale(fullDamage, res, 108, 112, 1.05f);
                    fullDamage = ExponentialDamageScale(fullDamage, res, 112, 0, 1.025f);
                }
                else
                {
                    fullDamage *= 0.3f;
                }
            }


            int intelligence = source.MyStats.GetCurrentInt();
            int proficiency = source.MyStats.IntScaleMod;
            int level = source.MyStats.Level;

            // Basically just tweaked numbers in JavaScript a billion times to get a formula that makes scaling with prof, level and int *all* feel useful,
            // while resulting in damage similar to vanilla on higher spells (just much lower on weak ones due to less additive dmg)
            // Have done another pass balancing all spells to get their damage consistent with new formula
            float effectiveProficiency = proficiency * (1 + level / 5f) * 2.5f;
            float effectiveInt = intelligence * 0.14f * (1 + (effectiveProficiency / 100));
            fullDamage *= (1 + effectiveInt / 100f); 
            
            if (includeFlatAddition)
            {
                float intAddition = intelligence * .3f; // Go for a tiiiiiiiny bit of additive damage from int, to help with low level chars scaling their weak spells, as it makes the most significant difference there
                fullDamage += intAddition;
            }
            
            return Mathf.RoundToInt(fullDamage);
        }

        /* With the default formula, additive damage means that spells like Ice Bolt (22 damage, 15 mana at time of writing) for me are dealing around 330 damage, whilst Ice Shock (280, 160) do 660.
         * It goes without saying, that completely fucks the mana economy this mod shoots for, so time for a re-scale!
         
         * Also boost the benefit of a Roaring Echo so that it's more than the vanilla 1% bonus damage per resonance over 100 (at best with BiS gear you were looking at 10% bonus damage, although
         * it's not quite that clear cut as the majority of the real bonus of Roaring Echo in vanilla actually comes from the fact that the resonating spell does full damage rather than the standard 30%)
         */
        static bool Prefix(ref int __result, int _baseDamage, bool ___resonating, CastSpell ___SpellSource)
        {
            __result = CalculateSpellDamage(___SpellSource, _baseDamage, ___resonating);
            return false;
        }
    }

    
    [HarmonyPatch(typeof(SpellVessel), "EndSpell")]
    class SpellVessel_EndSpell
    {
        static bool Prefix(SpellVessel __instance, CastSpell ___SpellSource, ref Stats ___targ, ref float ___CDMult)
        {
            var nextTarget = ___SpellSource.MyChar.GetComponent<IRetargetingSkill>()?.GetNextTarget();
            if (nextTarget != null) /* If an IRetargetingSkill was used and there are still more targets, cancel EndSpell, update target, resolve again 'til no targets left */
            {
                ___targ = nextTarget.MyStats;
                __instance.ResolveEarly();
                return false;
            }

            // There are instances in which we end up in EndSpell now without going through ResolveSpell, so the state clean-up -- removing the cast bar -- should happen here.
            if (___SpellSource.MyChar.MySkills.isPlayer) GameData.CB.CloseBar();

            var cdModifier = ___SpellSource.MyChar.GetComponent<ISpellCooldownModifier>();
            if (cdModifier != null)
                ___CDMult = cdModifier.GetSpellCooldownMulti();

            ___SpellSource.MyChar.GetCooldownManager().AddCooldown(__instance.spell, ___CDMult);

            return true;
        }
    }
}
