using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class GameComp_Transformation : GameComponent
    {

        private Dictionary<Pawn, Pawn> activeTransformations = new Dictionary<Pawn, Pawn>();

       // private List<TransformationData> activeTransformations = new List<TransformationData>();

        public GameComp_Transformation(Game game) : base ()
        {
            activeTransformations = new Dictionary<Pawn, Pawn>();
        }


        public bool RegisterTransformation(Pawn OriginalPawn, PawnKindDef kindDef, out Pawn transformationPawn)
        {
            transformationPawn = null;

            if (!HasTransformationFor(OriginalPawn))
            {
                Pawn generatedPawn = CreateForm(kindDef, OriginalPawn.Faction);
                if (generatedPawn != null)
                {
                    Map map = OriginalPawn.Map;
                    IntVec3 position = OriginalPawn.Position;

                    Hediff_Transformation transformation = generatedPawn.AddTransformationHediff();

                    if (transformation != null)
                        transformation.SetOriginalPawn(OriginalPawn);



                    if (generatedPawn.abilities == null)
                    {
                        generatedPawn.abilities = new Pawn_AbilityTracker(generatedPawn);
                    }

                    if (OriginalPawn.Spawned)
                    {
                        OriginalPawn.DeSpawn();

                        if (!Find.WorldPawns.Contains(OriginalPawn))
                        {
                            Find.WorldPawns.PassToWorld(OriginalPawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
                        }
                    }
                  
                    GenSpawn.Spawn(generatedPawn, position, map);
                    DraftingUtility.MakeDraftable(generatedPawn);
                    transformationPawn = generatedPawn;

                    activeTransformations.Add(OriginalPawn, generatedPawn);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Messages.Message($"Already transformed", MessageTypeDefOf.NegativeEvent);
                return false;
            }
        }


        public bool UnregisterTransformation(Pawn TransformationPawn)
        {
            if (IsTransformationPawn(TransformationPawn, out Pawn OriginalPawn))
            {
                Map map = TransformationPawn.Map;
                IntVec3 position = TransformationPawn.Position;

                if (TransformationPawn.Spawned)
                {
                    TransformationPawn.DeSpawn();
                }

                if (Find.WorldPawns.Contains(OriginalPawn))
                {
                    Find.WorldPawns.RemovePawn(OriginalPawn);
                }

                OriginalPawn.SetFaction(TransformationPawn.Faction);

                GenSpawn.Spawn(OriginalPawn, position, map);

                activeTransformations.Remove(OriginalPawn);

                if (TransformationPawn.health.hediffSet.HasHediff(MagicAndMythDefOf.MagicAndMyths_Transformation))
                {
                    TransformationPawn.health.RemoveHediff(TransformationPawn.health.hediffSet.GetFirstHediffOfDef(MagicAndMythDefOf.MagicAndMyths_Transformation));
                }

                return true;
            }

            return false;
        }


        private Pawn CreateForm(PawnKindDef kindDef, Faction faction)
        {      
           return PawnGenerator.GeneratePawn(kindDef == null ? PawnKindDefOf.Alphabeaver : kindDef, faction);
        }

        public bool HasTransformationFor(Pawn Pawn)
        {
            return activeTransformations.ContainsKey(Pawn) && activeTransformations[Pawn] != null;
        }

        public bool IsTransformationPawn(Pawn Pawn, out Pawn OriginalPawn)
        {
            OriginalPawn = null;

            foreach (var item in activeTransformations)
            {
                if (item.Value == Pawn)
                {
                    OriginalPawn = item.Key;
                    return true;
                }
            }

            return false;
        }

        public Pawn GetOriginalPawnForTransformation(Pawn Pawn)
        {
            if (IsTransformationPawn(Pawn, out Pawn OriginalPawn))
            {
                return OriginalPawn;
            }
            return null;
        }


        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref activeTransformations, "activeTransformations", LookMode.Reference, LookMode.Reference);
        }
    }


    public class TransformationData : IExposable
    {
        public Pawn OriginalPawn;
        public Pawn TransformationPawn;

        public void ExposeData()
        {
            Scribe_References.Look(ref OriginalPawn, "originalPawn");
            Scribe_References.Look(ref TransformationPawn, "transformationPawn");
        }
    }
}
