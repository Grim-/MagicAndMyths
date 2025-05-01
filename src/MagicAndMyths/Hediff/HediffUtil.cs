using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public static class HediffUtil
    {
        public static void ApplyHediffInRadius(HediffDef hediffDef, IntVec3 Position, Map map, float radius, Faction Faction, bool useCenter = true, bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {

                    bool HasNoFaction = pawn.Faction == null;
                    bool IsHostileCanTarget = pawn.Faction.HostileTo(Faction) && canTargetHostile;
                    bool IsAllyCanTarget = (pawn.Faction.RelationWith(Faction).kind == FactionRelationKind.Ally || pawn.Faction == Faction) && canTargetFriendly;
                    bool IsNeutralCanTarget = pawn.Faction.RelationWith(Faction).kind == FactionRelationKind.Neutral && canTargetNeutral;

                    if (HasNoFaction || IsHostileCanTarget || IsAllyCanTarget || IsNeutralCanTarget)
                    {
                       pawn.health.GetOrAddHediff(hediffDef);
                    }
                }
            }
        }

        public static void ApplyDamageInRadius(DamageDef damageDef, float damageAmount, float armourPenArmount, IntVec3 Position, Map map, float radius, Faction Faction, bool useCenter = true, Thing instigator = null,  bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                bool HasNoFaction = item.Faction == null;
                bool IsHostileCanTarget = item.Faction.HostileTo(Faction) && canTargetHostile;
                bool IsAllyCanTarget = (item.Faction.RelationWith(Faction).kind == FactionRelationKind.Ally || item.Faction == Faction) && canTargetFriendly;
                bool IsNeutralCanTarget = item.Faction.RelationWith(Faction).kind == FactionRelationKind.Neutral && canTargetNeutral;

                if (HasNoFaction || IsHostileCanTarget || IsAllyCanTarget || IsNeutralCanTarget)
                {
                    item.TakeDamage(new DamageInfo(damageDef, damageAmount, armourPenArmount, -1, instigator));
                }
            }
        }
    }

}