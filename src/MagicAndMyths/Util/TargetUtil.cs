using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace MagicAndMyths
{
    public static class TargetUtil
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


        public static List<Pawn> GetPawnsInRadius(IntVec3 Position, Map map, float radius, Faction Faction, bool useCenter = true, Thing instigator = null, bool canTargetHostile = true, bool canTargetFriendly = false, bool canTargetNeutral = false)
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (ShouldTarget(item.Faction, Faction, canTargetHostile, canTargetFriendly, canTargetNeutral))
                    {
                        pawns.Add(pawn);
                    }
                }
            }

            return pawns;
        }


        public static bool ShouldTarget(Faction targetFaction, Faction sourceFaction, bool canTargetHostile, bool canTargetFriendly, bool canTargetNeutral)
        {
            if (targetFaction == null)
                return true;

            if (targetFaction == sourceFaction && canTargetFriendly)
                return true;

            if (canTargetHostile && targetFaction.HostileTo(sourceFaction))
                return true;

            return false;
        }



        public static List<IntVec3> GetAllCellsInRect(IntVec3 Origin, IntVec3 Target, int width, int height)
        {
            List<IntVec3> result = new List<IntVec3>();

            IntVec3 diff = Target - Origin;
            float distance = diff.LengthHorizontal;

            if (distance < 0.001f)
            {
                diff = new IntVec3(0, 0, 1);
                distance = 1f;
            }

            Vector3 dirNormalized = new Vector3(diff.x, 0, diff.z).normalized;

            Vector3 perpendicular = new Vector3(-dirNormalized.z, 0, dirNormalized.x);

            Vector3 halfWidthPerp = perpendicular * (width / 2f);
            Vector3 originV3 = Origin.ToVector3Shifted();

            int boundingRadius = Mathf.CeilToInt(Mathf.Sqrt(height * height + width * width) / 2f);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(Origin, boundingRadius, true))
            {
                Vector3 cellVec = cell.ToVector3Shifted() - originV3;

                float alongLine = Vector3.Dot(cellVec, dirNormalized);
                float perpLine = Vector3.Dot(cellVec, perpendicular);

                if (alongLine >= 0 && alongLine <= height &&
                    perpLine >= -width / 2f && perpLine <= width / 2f)
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        public static List<IntVec3> GetCellsInCone(IntVec3 Origin, IntVec3 Target, int length, float angle)
        {
            List<IntVec3> result = new List<IntVec3>();
            IntVec3 diff = Target - Origin;
            Vector3 direction;
            if (diff.x == 0 && diff.z == 0)
            {
                direction = Vector3.forward;
            }
            else
            {
                direction = new Vector3(diff.x, 0, diff.z).normalized;
            }

            float cosHalfAngle = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(Origin, length, true))
            {
                if (cell == Origin)
                {
                    result.Add(cell);
                    continue;
                }

                Vector3 toCellVec = (cell.ToVector3Shifted() - Origin.ToVector3Shifted()).normalized;

                float dot = Vector3.Dot(direction, toCellVec);

                if (dot >= cosHalfAngle)
                {
                    if ((cell - Origin).LengthHorizontalSquared <= length * length)
                    {
                        result.Add(cell);
                    }
                }
            }

            return result;
        }


        public static List<Thing> GetThingsInCells(List<IntVec3> Cells, Map map)
        {
            List<Thing> things = new List<Thing>();

            foreach (var item in Cells)
            {
                things.AddRange(item.GetThingList(map));
            }

            return things;
        }

        public static List<Pawn> GetPawnsInCells(List<IntVec3> Cells, Map map)
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (var item in Cells)
            {
                Pawn pawn = item.GetFirstPawn(map);
                if (pawn != null)
                {
                    pawns.Add(pawn);
                }
            }
            return pawns;
        }
    }
}