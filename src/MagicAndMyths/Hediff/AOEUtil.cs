using RimWorld;
using Verse;
namespace MagicAndMyths
{
    public static class AOEUtil
    {

        public static void QuickHealInRadius(float healAmount, IntVec3 Position, Map map, float radius, Faction Faction, bool useCenter = true, bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (ShouldTarget(pawn.Faction, Faction, canTargetHostile, canTargetFriendly, canTargetNeutral))
                    {
                        pawn.QuickHeal(healAmount);
                    }
                }
            }
        }



        public static void ApplyHediffInRadius(HediffDef hediffDef, IntVec3 Position, Map map, float radius, Faction Faction, bool useCenter = true, bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (ShouldTarget(pawn.Faction, Faction, canTargetHostile, canTargetFriendly, canTargetNeutral))
                    {
                        pawn.health.GetOrAddHediff(hediffDef);
                    }
                }
            }
        }

        public static void ApplyHediffSeverityInRadius(HediffDef hediffDef, IntVec3 Position, Map map, float radius, Faction Faction, float SeverityChange, bool useCenter = true, bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (ShouldTarget(pawn.Faction, Faction, canTargetHostile, canTargetFriendly, canTargetNeutral))
                    {
                        if (pawn.health.hediffSet.HasHediff(hediffDef))
                        {
                            if (pawn.health.hediffSet.TryGetHediff(hediffDef, out Hediff hediff))
                            {
                                hediff.Severity += SeverityChange;
                            }
                        }
                    }
                }
            }
        }

        public static void ApplyDamageInRadius(DamageDef damageDef, float damageAmount, float armourPenArmount, IntVec3 Position, Map map, float radius, Faction Faction, bool useCenter = true, Thing instigator = null, bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (ShouldTarget(item.Faction, Faction, canTargetHostile, canTargetFriendly, canTargetNeutral))
                {
                    item.TakeDamage(new DamageInfo(damageDef, damageAmount, armourPenArmount, -1, instigator));
                }
            }
        }

        private static bool ShouldTarget(Faction targetFaction, Faction sourceFaction, bool canTargetHostile, bool canTargetFriendly, bool canTargetNeutral)
        {
            if (targetFaction == null)
                return true;

            if (targetFaction == sourceFaction && canTargetFriendly)
                return true;

            if (canTargetHostile && targetFaction.HostileTo(sourceFaction))
                return true;

            return false;
        }
    }
}