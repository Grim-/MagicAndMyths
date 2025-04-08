using RimWorld;
using System;
using Verse;

namespace MagicAndMyths
{
    public class GenStep_PopulateHeaven : GenStep
    {
        public override int SeedPart => 1234567;

        public override void Generate(Map map, GenStepParams parms)
        {
            var deadColonists = Find.World.GetComponent<WorldComp_HeavenTracker>()?.GetDeadColonists();

            if (deadColonists == null || deadColonists.Count == 0)
            {
                return;
            }

            Faction angelicFaction = FactionUtility.DefaultFactionFrom(FactionDefOf.Ancients);
            angelicFaction.Name = "Ascended Souls";

            int spacing = 10;
            int colonistsPerRow = (int)Math.Sqrt(deadColonists.Count) + 1;

            IntVec3 mapCenter = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);
            for (int i = 0; i < deadColonists.Count; i++)
            {
                var record = deadColonists[i];

                if (record.Pawn == null)
                    continue;

                int row = i / colonistsPerRow;
                int col = i % colonistsPerRow;
                IntVec3 position = new IntVec3(
                    mapCenter.x - (colonistsPerRow * spacing / 2) + col * spacing,
                    0,
                    mapCenter.z - ((deadColonists.Count / colonistsPerRow) * spacing / 2) + row * spacing
                );

                if (!position.InBounds(map))
                    continue;

                Pawn angelicPawn = PawnUtility_Duplicator.DuplicateDeadPawn(record.Pawn, angelicFaction);

                if (angelicPawn == null)
                    continue;

                angelicPawn.mindState.canFleeIndividual = false;
                angelicPawn.mindState.wantsToTradeWithColony = false;

                GenSpawn.Spawn(angelicPawn, position, map);
            }
        }
    }
}
