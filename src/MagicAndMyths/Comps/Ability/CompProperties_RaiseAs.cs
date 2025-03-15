using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_RaiseAs : CompProperties_AbilityEffect
    {
        public int raiseLimit = -1;
        public float radius = 15f;
        public bool canOnlyTargetHumanLike = true;

        public UndeadDef undeadDef;

        public CompProperties_RaiseAs()
        {
            compClass = typeof(CompAbilityEffect_RaiseAs);
        }
    }
    public class CompAbilityEffect_RaiseAs : CompAbilityEffect
    {
        new CompProperties_RaiseAs Props => (CompProperties_RaiseAs)props;
        Hediff_UndeadMaster master;

        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            master = (Hediff_UndeadMaster)this.parent.pawn.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<Thing> thingsInRadius = GenRadial.RadialDistinctThingsAround(target.Cell, this.parent.pawn.Map, Props.radius, true).ToList();


            int count = 0;
            foreach (var thing in thingsInRadius)
            {
                if (Props.raiseLimit > 0 && count > Props.raiseLimit)
                {
                    break;
                }

                if (thing is Corpse corwpse && corwpse.InnerPawn != null)
                {

                    if (Props.canOnlyTargetHumanLike && !corwpse.InnerPawn.RaceProps.Humanlike)
                    {
                        continue;
                    }

                    IntVec3 spawnPosition = corwpse.Position;
                    Pawn newPawn = null;
                    if (Props.undeadDef != null)
                    {
                        if (Props.undeadDef.kind != null)
                        {
                            newPawn = GeneratePawnFromDef();
                            if (newPawn != null)
                            {
                                newPawn.Name = corwpse.InnerPawn.Name;
                                if (corwpse != null && !corwpse.Destroyed)
                                {
                                    corwpse.Destroy();
                                }


                                SetupRaisedPawn(newPawn, spawnPosition);
                                count++;
                            }
                        }
                        else
                        {
                            //use the pawn as it is
                            newPawn = corwpse.InnerPawn;

                            if (ResurrectionUtility.TryResurrect(newPawn))
                            {
                                if (newPawn != null)
                                {
                                    SetupRaisedPawn(newPawn, spawnPosition);
                                    count++;
                                }
                            }
                        }
                    }



                }
            }
        }


        private void SetupRaisedPawn(Pawn newPawn, IntVec3 spawnPosition)
        {
            newPawn.story.Childhood = MagicAndMythDefOf.MagicAndMyths_LesserUndead;
            newPawn.story.Adulthood = MagicAndMythDefOf.MagicAndMyths_LesserUndead;

            Hediff_Undead undeadHediff = (Hediff_Undead)newPawn.health.GetOrAddHediff(Props.undeadDef.hediff);
            undeadHediff.SetSquadLeader(this.parent.pawn);

            master.SummonCreature(newPawn, spawnPosition);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            GenDraw.DrawFieldEdges(AffectedCells(target.Cell));
        }
        private List<IntVec3> AffectedCells(IntVec3 origin)
        {
            return GenRadial.RadialCellsAround(origin, Props.radius, true).ToList();
        }
        private Pawn GeneratePawnFromDef()
        {
            Pawn newPawn = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(Props.undeadDef.kind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1,
            true, false, false, false, true, 0, false, false, false, false, false, false, false, false, false, 0, 0, null, 0, null, null,
            new List<TraitDef>()
            {
                TraitDefOf.Bloodlust,
                TraitDefOf.Psychopath,
            },
            new List<TraitDef>()
            {
                TraitDefOf.Pyromaniac,
                TraitDefOf.Wimp,
                TraitDefOf.DislikesMen,
                TraitDefOf.DislikesWomen
            }, null, null, null, null, null, null, null));

            return newPawn;
        }
    }


    public class UndeadDef : Def
    {
        public int baseWillCost = 1;
        public float willCostMultiplier = 1f;
        public HediffDef hediff;
        public PawnKindDef kind;

        public List<BackstoryDef> childhoodBackstories;
        public List<BackstoryDef> adulthoodBackstories;
    }
}
