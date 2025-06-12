using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Building_PitfallTile : Building_ObstacleBase
    {
        protected bool _IsPassable = false;
        public float SolutionProgress => IsSolved ? 1f : 0f;

        public override SolutionWorker WorkedSolution => currentWorker;

        private List<PitfallSolutionWorker> solutionWorkers = new List<PitfallSolutionWorker>();
        private PitfallSolutionWorker currentWorker;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            foreach (PitfallSolutionDef solutionDef in DefDatabase<PitfallSolutionDef>.AllDefs)
            {
                solutionWorkers.Add(solutionDef.CreateWorker(this));
            }
        }

        public override void SetCurrentWorkedSolution(SolutionWorker compSolution)
        {
            currentWorker = (PitfallSolutionWorker)compSolution;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (_IsPassable)
            {
                
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }
        }


        public IntVec3 GetVaultTileTarget(Pawn pawn)
        {
            IntVec3 pawnPos = pawn.Position;

            IntVec3 direction = this.Position - pawnPos;
            direction = new IntVec3(
                Mathf.Clamp(direction.x, -1, 1),
                0,
                Mathf.Clamp(direction.z, -1, 1)
            );

           return this.Position + direction;
        }


        public override void OnSolutionComplete(Pawn pawn)
        {
            base.OnSolutionComplete(pawn);
            _IsPassable = true;
            Map?.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Buildings);
        }

        public override void Reset()
        {
            base.Reset();
            _IsPassable = false;
            Map?.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Buildings);
        }


        public override ushort PathWalkCostFor(Pawn p)
        {
            if (!_IsPassable)
            {
                return 10000;
            }
            return 0;
        }

        public override ushort PathFindCostFor(Pawn p)
        {
            if (!_IsPassable)
            {
                return 10000;
            }
            return base.PathFindCostFor(p);
        }



        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (!_IsSolved)
            {
                foreach (var comp in solutionWorkers)
                {
                    foreach (var item in comp.GetSolutionFloatOption(selPawn, this))
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
