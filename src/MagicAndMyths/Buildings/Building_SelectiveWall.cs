using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public enum WallFilterMode
    {
        NONE,
        FRIENDLY,
        FRIENDLY_NEUTRAL,
        CUSTOM,
        ALL
    }

    public class Building_SelectiveWall : Building
    {
        protected WallFilterMode WallFilterMode = WallFilterMode.FRIENDLY;

        public override Color DrawColor
        {
            get
            {
                Color baseColor = base.DrawColor;
                baseColor.a = 0.35f;

                return baseColor;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                base.Graphic.color = DrawColor;
                return base.Graphic;
            }
        }

        private void SetFilterMode(WallFilterMode newMode)
        {
            WallFilterMode = newMode;
            PropagateFilterMode();

            if (this.Spawned)
            {
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Buildings);
            }
        }

        public override bool IsDangerousFor(Pawn pawn)
        {
            if (CanPassThrough(pawn))
            {
                return false;
            }
            return true;
        }
  
        public override bool BlocksPawn(Pawn pawn)
        {
            if (CanPassThrough(pawn))
            {
                return false;
            }
            return true;
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
            if (CanPassThrough(p))
            {
                return (ushort)0;
            }
            return (ushort)10;
        }

        public override ushort PathFindCostFor(Pawn p)
        {
            if (CanPassThrough(p))
            {
                return (ushort)0;
            }
            return (ushort)10;
        }

        public virtual bool CanPassThrough(Pawn p)
        {
            switch (WallFilterMode)
            {
                case WallFilterMode.ALL:
                    return true;
                case WallFilterMode.FRIENDLY:
                    return p.Faction == Faction.OfPlayer;
                case WallFilterMode.FRIENDLY_NEUTRAL:
                    return p.Faction == Faction.OfPlayer || p.Faction != null && !p.Faction.HostileTo(Faction.OfPlayer) && p.Faction != Faction.OfPlayer;
                case WallFilterMode.CUSTOM:
                    return CustomPawnFilter(p);
                case WallFilterMode.NONE:
                default:
                    return false;
            }
        }

        protected virtual bool CustomPawnFilter(Pawn p)
        {
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (this.Faction == Faction.OfPlayer)
            {
                Command_Action filterCommand = new Command_Action
                {
                    defaultLabel = "Set Wall Filter",
                    defaultDesc = "Set which pawns can pass through this selective wall.",
                    icon = TexButton.CodexButton,
                    action = () => OpenFilterSelectionMenu()
                };

                yield return filterCommand;
            }
        }

        public void PropagateFilterMode()
        {
            HashSet<Building_SelectiveWall> processed = new HashSet<Building_SelectiveWall>();
            PropagateFilterModeRecursive(processed);
            processed.Clear();
        }

        private void PropagateFilterModeRecursive(HashSet<Building_SelectiveWall> processed)
        {
            processed.Add(this);
            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(this))
            {
                if (!cell.InBounds(Map))
                {
                    continue;
                }
                List<Thing> things = cell.GetThingList(Map);

                foreach (Thing thing in things)
                {
                    Building_SelectiveWall wall = thing as Building_SelectiveWall;
                    if (wall != null && !processed.Contains(wall))
                    {
                        wall.WallFilterMode = this.WallFilterMode;
                        wall.PropagateFilterModeRecursive(processed);
                    }
                }
            }
        }

        private void OpenFilterSelectionMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("Allow Friendly Only", () => SetFilterMode(WallFilterMode.FRIENDLY)));
            options.Add(new FloatMenuOption("Allow Friend & Neutral Only", () => SetFilterMode(WallFilterMode.FRIENDLY_NEUTRAL)));
            options.Add(new FloatMenuOption("Allow All", () => SetFilterMode(WallFilterMode.ALL)));
            options.Add(new FloatMenuOption("Block All", () => SetFilterMode(WallFilterMode.NONE)));

            if (CanUseCustomFilter())
            {
                options.Add(new FloatMenuOption("Custom Filter", () => SetFilterMode(WallFilterMode.CUSTOM)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        protected virtual bool CanUseCustomFilter()
        {
            return false;
        }
        public override string GetInspectString()
        {
            return base.GetInspectString() + $"Mode : {WallFilterMode}";
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref WallFilterMode, "wallFilterMode", WallFilterMode.FRIENDLY);
        }
    }
}
