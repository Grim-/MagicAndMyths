using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Building_FakeWall : Building
    {
        //considered discoverd if the pawn has a certain trait or its been walked on by any colonist
        private bool HasBeenDiscovered = false;
        public override Color DrawColor => HasBeenDiscovered ? Color.white * 0.4f : base.DrawColor;

        public override string GetInspectString()
        {

            return base.GetInspectString() + (HasBeenDiscovered ? "discovered" : "undiscovered");
        }


        public void SetDiscovered(bool newValue)
        {
            HasBeenDiscovered = newValue;

            if (HasBeenDiscovered)
            {
                this.Map.linkGrid.linkGrid[this.Map.cellIndices.CellToIndex(this.Position)] = LinkFlags.None;
            }
            else
            {
                this.Map.linkGrid.Notify_LinkerCreatedOrDestroyed(this);
            }
            this.Map.mapDrawer.MapMeshDirty(this.Position, MapMeshFlagDefOf.Buildings);
        }

        public override bool BlocksPawn(Pawn p)
        {
            if (HasBeenDiscovered)
            {
                return false;
            }

            return !p.Drafted;
        }


        public override bool IsDangerousFor(Pawn pawn)
        {
            if (HasBeenDiscovered)
            {
                return false;
            }

            if (pawn != null && pawn.Drafted)
            {
                return false;
            }

            return true;
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
            if (HasBeenDiscovered)
            {
                return 0;
            }

            return (ushort)(p.Drafted ? 0 : 4000);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (!HasBeenDiscovered)
            {
                base.DrawAt(drawLoc, flip);
            }
            else
            {
                GhostDrawer.DrawGhostThing(this.Position, this.Rotation, this.def, this.Graphic, Color.white, AltitudeLayer.Filth);
            }
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {

            if (!HasBeenDiscovered)
            {
                base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            }

       
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Toggle Discovered",
                action = () =>
                {
                    SetDiscovered(!HasBeenDiscovered);
                }
            };
        }
    }
}
