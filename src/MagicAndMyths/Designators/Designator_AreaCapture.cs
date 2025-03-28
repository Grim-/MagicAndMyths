using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
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
            return new Zone_AreaCapture(Find.CurrentMap.zoneManager, new IntVec2(3,3));
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
                // Generate XML and show it
                //
            }
        }

  
    }
    public class Zone_AreaCapture : Zone
    {
        public IntVec2 OriginSize;
        public bool CaptureFloors = true;
        public bool CaptureTerrain = true;
        public bool CaptureThings = true;

        public Zone_AreaCapture()
        {
        }

        public Zone_AreaCapture(ZoneManager zoneManager, IntVec2 originBuildingSize) : base("AreaCapture".Translate(), zoneManager)
        {
            this.OriginSize = originBuildingSize;
        }

        public override bool IsMultiselectable => true;

        protected override Color NextZoneColor => new Color(0.2f, 0.6f, 0.3f);

        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneExpand_AreaCapture>();
            yield break;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Capture to Xml",
                action = () =>
                {
                    CellRect cellRect = CellRect.FromCellList(cells);
                    ShowCapturedXml(XMLUtil.CaptureAreaToDefXml(Map, cellRect, cellRect.CenterCell));
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Capture Settings",
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_AreaCaptureSettings(this));
                }
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref OriginSize, "OriginSize");
            Scribe_Values.Look(ref CaptureFloors, "CaptureFloors", true);
            Scribe_Values.Look(ref CaptureTerrain, "CaptureTerrain", true);
            Scribe_Values.Look(ref CaptureThings, "CaptureThings", true);
        }




        protected virtual void ShowCapturedXml(string areaXml)
        {
            Find.WindowStack.Add(new Dialog_AreaCaptured(areaXml));
        }
    }


    public class Designator_ZoneExpand_AreaCapture : Designator_ZoneAdd_AreaCapture
    {
        public Designator_ZoneExpand_AreaCapture()
        {
            this.defaultLabel = "DesignatorAreaCaptureExpand".Translate();
            this.defaultDesc = "DesignatorAreaCaptureExpandDesc".Translate();
            this.hotKey = KeyBindingDefOf.Misc8;
        }
    }
}
