using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class MagicUtil
    {
        public static bool IsInvisible(this Thing t)
        {
            if (t is ThingWithComps withComps)
            {
                if (withComps.TryGetComp(out Comp_ThingProperties thingProperties))
                {
                    InvisiblePropertyWorker invisiblePropertyWorker = (InvisiblePropertyWorker)thingProperties.GetProperty(MagicAndMythDefOf.ThingProp_Invisible);

                    if (invisiblePropertyWorker != null)
                    {
                        return invisiblePropertyWorker.IsInvisible(t);
                    }
                }
            }
            return false;
        }

        [DebugAction("Magic And Myths", "Add Thing Property", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddThingProperty()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetBuildings = true,
                canTargetCorpses = true, 
                canTargetHumans = true,
                canTargetItems = true,
                mustBeSelectable = true,
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null && target.Thing is ThingWithComps withComps)
                {
                    List<FloatMenuOption> Options = new List<FloatMenuOption>();

                    foreach (var item in DefDatabase<ThingPropertyDef>.AllDefs)
                    {
                        Options.Add(new FloatMenuOption($"Add {item.label} Property to {target.Thing.Label}", () =>
                        {
                            if (withComps.TryGetComp(out Comp_ThingProperties _ThingProperties))
                            {
                                Log.Message("Adding prop to thing");
                                _ThingProperties.AddProperty(item);
                            }
                            else
                            {
                                Log.Message("thing has no comp_thingproperties");
                            }
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(Options));
                }
            }
            );
        }


        [DebugAction("Magic And Myths", "Add Enchant To Item", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddEnchant()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetBuildings = false,
                canTargetCorpses = false,
                canTargetHumans = false,
                canTargetItems = true,
                mustBeSelectable = true,
                mapObjectTargetsMustBeAutoAttackable = false
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null && target.Thing is ThingWithComps withComps)
                {

                    if (withComps.TryGetComp(out Comp_EnchantProvider _EnchantProvider))
                    {
                        List<FloatMenuOption> Options = new List<FloatMenuOption>();

                        foreach (var item in DefDatabase<EnchantDef>.AllDefs)
                        {
                            Options.Add(new FloatMenuOption($"Add {item.label} to {target.Thing.Label}", () =>
                            {
                                _EnchantProvider.AddEnchant(item);
                            }));
                        }

                        if (Options.Count > 0)
                        {
                            Find.WindowStack.Add(new FloatMenu(Options));
                        }                     
                    }         
                }
            }
            );
        }

        [DebugAction("Magic And Myths", "Test Spawn Orbital Laser", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnOrbitalLaser()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    OrbitalLaser meteor = (OrbitalLaser)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("MagicAndMyths_OrbitalLaser"));
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Fire(target.Cell);
                }
            }
            );
        }


        [DebugAction("Magic And Myths", "Test Transmute Lightning", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FireTransmutationLightning()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    LightningStrike.GenerateLightningStrike(Find.CurrentMap, target.Cell, 5, out IEnumerable<IntVec3> affectedCells);
                    TerrainDef goldTile = DefDatabase<TerrainDef>.AllDefs.RandomElement();


                    List<ThingDef> naturalRockDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.building.isNaturalRock).ToList();

                    foreach (var item in affectedCells)
                    {
                        Find.CurrentMap.terrainGrid.SetTerrain(item, goldTile);



                        foreach (var thing in item.GetThingList(Find.CurrentMap))
                        {
                            if (thing.def.building != null && thing.def.building.isNaturalRock)
                            {
                                IntVec3 position = thing.Position;

                                thing.Destroy();

                                Thing replacementRock = ThingMaker.MakeThing(naturalRockDefs.RandomElement());
                                GenSpawn.Spawn(replacementRock, position, Find.CurrentMap);


                            }

                        }
                    }

                }
            }
            );


        }
        [DebugAction("Magic And Myth", "Test Spawn Meteor", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnMeteor()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    Meteor meteor = (Meteor)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_Meteor);
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Launch(target.Cell);
                }
            }
            );


        }

        [DebugAction("Magic And Myth", "Petrify Pawn", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PetrifyPawn()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetHumans = true,
                canTargetMechs = true,
                mapObjectTargetsMustBeAutoAttackable = false
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null && target.Thing is Pawn pawn)
                {
                    Map pawnMap = pawn.Map;
                    IntVec3 position = pawn.Position;
                    PetrifiedStatue.PetrifyPawn(
                        MagicAndMythDefOf.MagicAndMyths_PetrifiedStatue,
                        pawn,
                        position,
                        pawnMap
                    );
                    Messages.Message("Petrified " + pawn.LabelShort, MessageTypeDefOf.NeutralEvent);
                }
            });
        }


        [DebugAction("Magic And Myth", "Test EffecterEditor", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void OpenEffecterEditor()
        {
            Find.WindowStack.Add(new EffecterDefEditorWindow());
        }

        public static bool HasCooldownByTick(int LastTriggerTick, int CooldownTicks)
        {

            if (LastTriggerTick <= 0)
            {
                return false;
            }

            return Current.Game.tickManager.TicksGame <= LastTriggerTick + CooldownTicks;
        }


        public static void TrainPawn(Pawn PawnToTrain, Pawn Trainer = null)
        {
            if (PawnToTrain.training != null)
            {
                foreach (var item in DefDatabase<TrainableDef>.AllDefsListForReading)
                {
                    PawnToTrain.training.SetWantedRecursive(item, true);
                    PawnToTrain.training.Train(item, Trainer, true);
                }


                if (PawnToTrain.playerSettings != null)
                {
                    PawnToTrain.playerSettings.followDrafted = true;
                }
            }
        }

        //public static bool TryMakeSummonOf(this Pawn pawn, Pawn Master)
        //{
        //    Hediff_UndeadMaster master = (Hediff_UndeadMaster)Master.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);
        //    Hediff_Undead undeadSummon = (Hediff_Undead)pawn.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_Undead);
        //    if (master != null && undeadSummon != null)
        //    {
        //        undeadSummon.SetSquadLeader(Master);
        //        return true;
        //    }

        //    return false;
        //}

        public static bool HasWeaponEquipped(this Pawn pawn)
        {
            return pawn.def.race.Humanlike && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.PrimaryEq != null;
        }
        public static DamageInfo GetWeaponDamage(this CompEquippable Equippable, Pawn attacker, float damageMultiplier = 1, float overrideArmourPen = -1)
        {
            DamageDef damageDef = Equippable.PrimaryVerb.GetDamageDef();

            if (Equippable.PrimaryVerb == null || Equippable.PrimaryVerb.GetDamageDef() == null)
            {
                return default(DamageInfo);
            }


            float armourPen = overrideArmourPen > 0 ? overrideArmourPen : Equippable.PrimaryVerb.verbProps.AdjustedArmorPenetration(Equippable.PrimaryVerb, attacker);

            return new DamageInfo(damageDef, 
                Equippable.PrimaryVerb.verbProps.AdjustedMeleeDamageAmount(Equippable.PrimaryVerb, attacker) * Mathf.Min(1, damageMultiplier),
                armourPen,
                -1,
                attacker,
                null, 
                Equippable.parent.def);
        }


        public static bool TryGetWorstInjury(Pawn pawn, out Hediff hediff, out BodyPartRecord part, Func<Hediff, bool> filter = null, params HediffDef[] exclude)
        {
            part = null;
            hediff = null;

            bool PassesFilter(Hediff h) => filter == null || filter(h);

            Hediff lifeThreateningHediff = HealthUtility.FindLifeThreateningHediff(pawn, exclude);
            if (lifeThreateningHediff != null && PassesFilter(lifeThreateningHediff))
            {
                hediff = lifeThreateningHediff;
                return true;
            }

            if (HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 2500)
            {
                Hediff bleedingHediff = HealthUtility.FindMostBleedingHediff(pawn, exclude);
                if (bleedingHediff != null && PassesFilter(bleedingHediff))
                {
                    hediff = bleedingHediff;
                    return true;
                }
            }

            if (pawn.health.hediffSet.GetBrain() != null)
            {
                var brain = pawn.health.hediffSet.GetBrain();
                Hediff brainInjury = HealthUtility.FindPermanentInjury(pawn, Gen.YieldSingle(brain), exclude);
                if (brainInjury != null && PassesFilter(brainInjury))
                {
                    hediff = brainInjury;
                    return true;
                }
                brainInjury = HealthUtility.FindInjury(pawn, Gen.YieldSingle(brain), exclude);
                if (brainInjury != null && PassesFilter(brainInjury))
                {
                    hediff = brainInjury;
                    return true;
                }
            }

            float significantCoverage = ThingDefOf.Human.race.body.GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;
            part = HealthUtility.FindBiggestMissingBodyPart(pawn, significantCoverage);
            if (part != null)
            {
                return true;
            }

            Hediff eyeInjury = HealthUtility.FindPermanentInjury(
                pawn,
                from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                where x.def == BodyPartDefOf.Eye
                select x,
                exclude);
            if (eyeInjury != null && PassesFilter(eyeInjury))
            {
                hediff = eyeInjury;
                return true;
            }

            part = HealthUtility.FindBiggestMissingBodyPart(pawn, 0f);
            if (part != null)
            {
                return true;
            }

            Hediff permanentInjury = HealthUtility.FindPermanentInjury(pawn, null, exclude);
            if (permanentInjury != null && PassesFilter(permanentInjury))
            {
                hediff = permanentInjury;
                return true;
            }

            Hediff anyInjury = HealthUtility.FindInjury(pawn, null, exclude);
            if (anyInjury != null && PassesFilter(anyInjury))
            {
                hediff = anyInjury;
                return true;
            }

            return false;
        }
        public static void QuickHeal(this Pawn pawn, float healAmount)
        {
            if (TryGetWorstInjury(pawn, out Hediff hediff, out BodyPartRecord part, null))
            {
                if (hediff == null || hediff.def == null)
                {
                    return;
                }

                HealthUtility.AdjustSeverity(pawn, hediff.def, -healAmount);
            }
        }
        public static bool IsControlledSummon(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff<Hediff_Undead>();
        }
    }
}
