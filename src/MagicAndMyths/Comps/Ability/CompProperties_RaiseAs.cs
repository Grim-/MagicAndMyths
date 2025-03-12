using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_RaiseAs : CompProperties_AbilityEffect
    {
        public float radius = 15f;
        public bool canOnlyTargetHumanLike = true;
        public PawnKindDef defToRaiseAs;

        public CompProperties_RaiseAs()
        {
            compClass = typeof(CompAbilityEffect_RaiseAs);
        }
    }
    public class CompAbilityEffect_RaiseAs : CompAbilityEffect
    {
        new CompProperties_RaiseAs Props => (CompProperties_RaiseAs)props;
        Hediff_UndeadMaster master;


        private List<Ticker> tickers = new List<Ticker>();

        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            master = (Hediff_UndeadMaster)this.parent.pawn.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<Thing> thingsInRadius = GenRadial.RadialDistinctThingsAround(this.parent.pawn.Position, this.parent.pawn.Map, Props.radius, true).ToList();
            foreach (var thing in thingsInRadius)
            {
                if (thing is Corpse corwpse && corwpse.InnerPawn != null)
                {

                    if (Props.canOnlyTargetHumanLike && !corwpse.InnerPawn.RaceProps.Humanlike)
                    {
                        continue;
                    }


                    Pawn newPawn = null;

                    if (Props.defToRaiseAs != null)
                    {
                        newPawn = GeneratePawnFromDef();
                    }
                    else
                    {
                        //use the pawn as it is

                        if (ResurrectionUtility.TryResurrect(corwpse.InnerPawn))
                        {
                            newPawn = corwpse.InnerPawn;
                        }
                    }


                    if (corwpse.InnerPawn.RaceProps.Humanlike)
                    {
                        IntVec3 spawnPosition = corwpse.Position;
                        if (newPawn != null)
                        {
                            newPawn.Name = corwpse.InnerPawn.Name;
                            corwpse.Destroy();
                            newPawn.story.Childhood = MagicAndMythDefOf.MagicAndMyths_LesserUndead;
                            newPawn.story.Adulthood = MagicAndMythDefOf.MagicAndMyths_LesserUndead;

                            GenSpawn.Spawn(newPawn, spawnPosition, this.parent.pawn.Map);

                            master.StoreCreature(newPawn);
                            master.SummonCreature(newPawn, spawnPosition);
                        }
                    }
                }
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            GenDraw.DrawFieldEdges(GenRadial.RadialDistinctThingsAround(this.parent.pawn.Position, this.parent.pawn.Map, Props.radius, true).Select(x => x.Position).ToList());
        }



        private Pawn GeneratePawnFromDef()
        {
            Pawn newPawn = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(Props.defToRaiseAs, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1,
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
}
