using RimWorld;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace MagicAndMyths
{
    public class Designator_ZoneAdd_AreaCapture : Designator_ZoneAdd
    {
        protected override string NewZoneLabel => "AreaCapture".Translate();

        private Zone_AreaCapture zone;

        public Designator_ZoneAdd_AreaCapture()
        {
            this.zoneTypeToPlace = typeof(Zone_AreaCapture);
            this.defaultLabel = "Capture Structure";
            this.defaultDesc = "Capture an area to XML format for direct pasting into defs.";
            this.icon = TexButton.Play;
            this.useMouseIcon = true;
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }

        protected override Zone MakeNewZone()
        {
            return Zone_AreaCapture.GetOrCreateForMap(Find.CurrentMap, new IntVec2(3, 3));
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();


           
            if (Find.Selector.SelectedZone != null && Find.Selector.SelectedZone is Zone_AreaCapture areaCapture)
            {
                IntVec3 center = CellRect.FromCellList(areaCapture.cells).CenterCell;
                CellRect rect = CellRect.CenteredOn(center, 1);
                GenDraw.DrawFieldEdges(rect.Cells.ToList());
            }


        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(this.Map))
                return false;
            if (c.Fogged(this.Map))
                return false;
            if (c.InNoBuildEdgeArea(this.Map))
                return "TooCloseToMapEdge".Translate();

            Zone zone = this.Map.zoneManager.ZoneAt(c);
            if (zone != null && zone.GetType() != this.zoneTypeToPlace)
                return false;

            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            base.DesignateSingleCell(c);
            Zone zone = this.Map.zoneManager.ZoneAt(c);
            Zone_AreaCapture captureZone = zone as Zone_AreaCapture;
            if (captureZone != null)
            {
                captureZone.AddCell(c);
            }
        }
    }
}
