using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class ThrowUtility
    {
        public static void ApplyDefaultThrowImpactThingBehavior(Pawn thrower, Thing thrownThing, IntVec3 position, Map map, Thing impactedThing)
        {
            if (map == null || thrownThing == null || impactedThing.Destroyed)
                return;
            ApplyBaseImpactDamage(thrower, thrownThing, position, map, impactedThing);
        }

        public static void ApplyDefaultThrowBehavior(Pawn thrower, Thing thrownThing,  IntVec3 position, Map map)
        {
            if (map == null || thrownThing == null)
                return;
        }

        private static void ApplyBaseImpactDamage(Pawn thrower, Thing thrownThing, IntVec3 position, Map map, Thing impactedThing = null)
        {
            List<Thing> pawnsInCell = position.GetThingList(map)
                .OfType<Thing>()
                .ToList();

            if (pawnsInCell.Count > 0)
            {      
                foreach (Thing victim in pawnsInCell)
                {
                    if (victim == thrower || victim == thrownThing || !victim.def.useHitPoints) 
                        continue;

                    DamageInfo damageInfo = new DamageInfo(
                        GetImpactDamageDefFor(thrownThing),
                        CalculateImpactDamage(thrower, thrownThing),
                        0f,
                        -1f,
                        thrower);

                    victim.TakeDamage(damageInfo);

                    if (thrower != null)
                    {
                        Messages.Message(
                            thrower.LabelShort + " hit " + victim.LabelShort + " with " + thrownThing.LabelShort + ".",
                            MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        Messages.Message(
                            victim.LabelShort + " was hit by " + thrownThing.LabelShort + ".",
                            MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
        }



        public static float CalculateMaxThrowMassForPawn(Pawn pawn)
        {
            float baseThrowMass = 50f;
            float bodySizeMultiplier = pawn.BodySize;
            float manipulationBonus = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            float strengthFactor = pawn.GetStatValue(StatDefOf.CarryingCapacity) / pawn.GetStatValue(StatDefOf.Mass);

            float finalThrowingMass = baseThrowMass * bodySizeMultiplier * manipulationBonus * strengthFactor;

            return finalThrowingMass;
        }


        public static DamageDef GetImpactDamageDefFor(Thing thing)
        {
            DamageDef chosenType = null;

            if (thing.def.IsMeleeWeapon)
            {
                if (thing.def.tools != null && thing.def.tools.Any())
                {
                    Tool firstTool = thing.def.tools.First();
                    if (firstTool.capacities != null && firstTool.capacities.Any())
                    {
                        ToolCapacityDef firstCapacity = firstTool.capacities.First();
                        VerbProperties verbProps = firstCapacity.VerbsProperties.FirstOrDefault();
                        if (verbProps != null && verbProps.meleeDamageDef != null)
                        {
                            chosenType = verbProps.meleeDamageDef;
                        }
                    }
                }
            }


            if (chosenType == null)
            {
                chosenType = DamageDefOf.Blunt;
            }

            return chosenType;
        }


        public static float CalculateImpactDamage(Pawn thrower, Thing thrownThing)
        {
            float finalDamage = 1f;
            float baseDamage = CalcMassDamage(thrownThing, 2.5f);
            float weaponDamage = 10f;
            float throwerBonus = 1.0f;

            if (thrower != null)
            {
                float manipulationFactor = thrower.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
                throwerBonus = Mathf.Lerp(1f, 2.0f, manipulationFactor);
            }

            if (thrownThing.def.IsMeleeWeapon)
            {
                // Get first tool and its first capacity
                if (thrownThing.def.tools != null && thrownThing.def.tools.Any())
                {
                    Tool firstTool = thrownThing.def.tools.First();
                    if (firstTool.capacities != null && firstTool.capacities.Any())
                    {
                        ToolCapacityDef firstCapacity = firstTool.capacities.First();
                        VerbProperties verbProps = firstCapacity.VerbsProperties.FirstOrDefault();
                        if (verbProps != null && verbProps.meleeDamageDef != null)
                        {
                            weaponDamage = verbProps.meleeDamageBaseAmount;
                        }
                    }
                }
            }

            finalDamage = baseDamage;
            finalDamage += weaponDamage;
            finalDamage *= throwerBonus;
            return finalDamage;
        }

        private static float CalcMassDamage(Thing Thing, float massMultiplier = 2.5f)
        {
            return 10f;
        }
    }
}