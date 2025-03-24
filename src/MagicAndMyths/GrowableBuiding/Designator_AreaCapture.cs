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
    public class Designator_AreaCapture : Designator
    {
        private Vector3 startPos;
        private IntVec3 startCell;
        private bool dragging;
        private CellRect currentRect;
        private IntVec3 originCell; // The selected origin point
        private bool originSelected = false; // Whether the origin has been selected

        public Designator_AreaCapture()
        {
            defaultLabel = "Capture Structure";
            defaultDesc = "Capture an area to XML format for direct pasting into defs.";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn", true);
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.InBounds(Map);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (!dragging)
            {
                startPos = c.ToVector3Shifted();
                startCell = c;
                dragging = true;
                currentRect = new CellRect(c.x, c.z, 1, 1);
                originSelected = false; // Reset origin selection
            }
            else
            {
                dragging = false;
                IntVec3 endCell = c;
                CellRect rect = CellRect.FromLimits(
                    Math.Min(startCell.x, endCell.x),
                    Math.Min(startCell.z, endCell.z),
                    Math.Max(startCell.x, endCell.x),
                    Math.Max(startCell.z, endCell.z));

                // Now prompt user to select origin within the rect
                Messages.Message("Select the cell that should be the origin (0,0) within the captured area", MessageTypeDefOf.NeutralEvent);
                Find.DesignatorManager.Select(new Designator_OriginSelector(rect, this));
            }
        }

        // This will be called by the OriginSelector after origin is selected
        public void CompleteCapture(CellRect rect, IntVec3 origin)
        {
            originCell = origin;
            originSelected = true;
            CaptureAreaToDefXml(rect, origin);
        }

        private void CaptureAreaToDefXml(CellRect rect, IntVec3 origin)
        {
            // Create an XML document for output
            XmlDocument doc = new XmlDocument();

            // Create the root element as direct def XML
            XmlElement rootElement = doc.CreateElement("StructureLayoutDef");
            doc.AppendChild(rootElement);

            // Add basic Def properties
            XmlElement defNameElement = doc.CreateElement("defName");
            defNameElement.InnerText = "CapturedStructure_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            rootElement.AppendChild(defNameElement);

            //// Add origin element to store where the origin point is relative to the rect
            //XmlElement originElement = doc.CreateElement("origin");
            //rootElement.AppendChild(originElement);

            //XmlElement originXElement = doc.CreateElement("x");
            //originXElement.InnerText = (origin.x - rect.minX).ToString();
            //originElement.AppendChild(originXElement);

            //XmlElement originZElement = doc.CreateElement("z");
            //originZElement.InnerText = (origin.z - rect.minZ).ToString();
            //originElement.AppendChild(originZElement);

            // Create stages list element
            XmlElement stagesElement = doc.CreateElement("stages");
            rootElement.AppendChild(stagesElement);

            // Add size element
            XmlElement sizeElement = doc.CreateElement("size");
            rootElement.AppendChild(sizeElement);

            XmlElement sizeXElement = doc.CreateElement("x");
            sizeXElement.InnerText = rect.Width.ToString();
            sizeElement.AppendChild(sizeXElement);

            XmlElement sizeZElement = doc.CreateElement("z");
            sizeZElement.InnerText = rect.Height.ToString();
            sizeElement.AppendChild(sizeZElement);

            // Create a single stage (we'll just capture everything as one stage)
            XmlElement stageElement = doc.CreateElement("li");
            stagesElement.AppendChild(stageElement);

            // Create the stage's inner elements for each category
            XmlElement terrainElement = doc.CreateElement("terrain");
            XmlElement wallsElement = doc.CreateElement("walls");
            XmlElement doorsElement = doc.CreateElement("doors");
            XmlElement powerElement = doc.CreateElement("power");
            XmlElement furnitureElement = doc.CreateElement("furniture");
            XmlElement otherElement = doc.CreateElement("other");

            stageElement.AppendChild(terrainElement);
            stageElement.AppendChild(wallsElement);
            stageElement.AppendChild(doorsElement);
            stageElement.AppendChild(powerElement);
            stageElement.AppendChild(furnitureElement);
            stageElement.AppendChild(otherElement);

            // Calculate positions relative to the selected origin cell
            foreach (IntVec3 cell in rect.Cells)
            {
                // Calculate relative position from the selected origin
                IntVec3 relPos = cell - origin;

                // Handle terrain/floor
                TerrainDef terrain = Map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain != TerrainDefOf.Soil && terrain != TerrainDefOf.Sand)
                {
                    XmlElement terrainItemElement = doc.CreateElement("li");
                    terrainElement.AppendChild(terrainItemElement);

                    // Add terrain def reference
                    XmlElement terrainDefElement = doc.CreateElement("terrain");
                    terrainDefElement.InnerText = terrain.defName;
                    terrainItemElement.AppendChild(terrainDefElement);

                    // Add position
                    XmlElement positionElement = doc.CreateElement("position");
                    terrainItemElement.AppendChild(positionElement);

                    XmlElement posXElement = doc.CreateElement("x");
                    posXElement.InnerText = relPos.x.ToString();
                    positionElement.AppendChild(posXElement);

                    XmlElement posZElement = doc.CreateElement("z");
                    posZElement.InnerText = relPos.z.ToString();
                    positionElement.AppendChild(posZElement);
                }

                // Process things in the cell
                List<Thing> things = cell.GetThingList(Map);
                foreach (Thing thing in things)
                {
                    // Skip certain types that we don't want to include
                    if (thing is Pawn || thing is Filth || thing is Mote || thing is Corpse || thing is Plant)
                        continue;

                    // Create the placement element
                    XmlElement placementElement = doc.CreateElement("li");

                    // Add the thing def reference
                    XmlElement thingDefElement = doc.CreateElement("thing");
                    thingDefElement.InnerText = thing.def.defName;
                    placementElement.AppendChild(thingDefElement);

                    // Add position
                    XmlElement positionElement = doc.CreateElement("position");
                    placementElement.AppendChild(positionElement);

                    XmlElement posXElement = doc.CreateElement("x");
                    posXElement.InnerText = relPos.x.ToString();
                    positionElement.AppendChild(posXElement);

                    XmlElement posZElement = doc.CreateElement("z");
                    posZElement.InnerText = relPos.z.ToString();
                    positionElement.AppendChild(posZElement);

                    // Add rotation if not default
                    if (thing.Rotation != Rot4.North)
                    {
                        XmlElement rotationElement = doc.CreateElement("rotation");

                        // Convert rotation to North/East/South/West
                        string rotationName = "";
                        switch (thing.Rotation.AsInt)
                        {
                            case 0: rotationName = "North"; break;
                            case 1: rotationName = "East"; break;
                            case 2: rotationName = "South"; break;
                            case 3: rotationName = "West"; break;
                        }
                        rotationElement.InnerText = rotationName;
                        placementElement.AppendChild(rotationElement);
                    }

                    // Add stuff (building material) if applicable
                    if (thing.Stuff != null)
                    {
                        XmlElement stuffElement = doc.CreateElement("stuff");
                        stuffElement.InnerText = thing.Stuff.defName;
                        placementElement.AppendChild(stuffElement);
                    }

                    // Add to the appropriate category
                    if (thing is Building_Door)
                    {
                        doorsElement.AppendChild(placementElement);
                    }
                    else if (thing.def.fillPercent > 0.8f && thing.def.passability == Traversability.Impassable)
                    {
                        wallsElement.AppendChild(placementElement);
                    }
                    else if (thing.def.building != null && thing.def.building.isPowerConduit)
                    {
                        powerElement.AppendChild(placementElement);
                    }
                    else if (thing is Building && thing.def.building != null)
                    {
                        furnitureElement.AppendChild(placementElement);
                    }
                    else
                    {
                        otherElement.AppendChild(placementElement);
                    }
                }
            }

            // Create a pretty-printed XML string
            string xmlString = FormatXml(doc);

            // Show in a window for copying
            ShowCapturedXml(xmlString);
        }

        private string FormatXml(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }

            return sb.ToString();
        }

        protected virtual void ShowCapturedXml(string areaXml)
        {
            Find.WindowStack.Add(new Dialog_AreaCaptured(areaXml));
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells) { }

        // Draw the selection rectangle while dragging
        public override void DrawPanelReadout(ref float curY, float width)
        {
            if (dragging)
            {
                Widgets.Label(new Rect(0f, curY, width, 24f), "Selecting area...");
                curY += 24f;
            }
        }

        // Draw a visual overlay of the area being captured
        public override void SelectedUpdate()
        {
            base.SelectedUpdate();

            if (dragging)
            {
                // Get current mouse position
                IntVec3 mouseCell = UI.MouseCell();
                if (mouseCell.InBounds(Map))
                {
                    // Update the current rectangle
                    currentRect = CellRect.FromLimits(
                        Math.Min(startCell.x, mouseCell.x),
                        Math.Min(startCell.z, mouseCell.z),
                        Math.Max(startCell.x, mouseCell.x),
                        Math.Max(startCell.z, mouseCell.z));

                    // Draw the selection rectangle
                    GenDraw.DrawFieldEdges(currentRect.Cells.ToList(), Color.cyan);

                    // Draw the center point marker
                    IntVec3 center = currentRect.CenterCell;
                    GenDraw.DrawFieldEdges(new List<IntVec3> { center }, Color.yellow);
                }
            }
        }
    }

    // New designator for selecting the origin point
    public class Designator_OriginSelector : Designator
    {
        private CellRect rect;
        private Designator_AreaCapture parentDesignator;

        public Designator_OriginSelector(CellRect rect, Designator_AreaCapture parent)
        {
            this.rect = rect;
            this.parentDesignator = parent;
            defaultLabel = "Select Origin";
            defaultDesc = "Select the cell that will be the (0,0) origin point for the captured structure.";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn", true);
            soundDragSustain = null;
            soundDragChanged = null;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return rect.Contains(c);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (rect.Contains(c))
            {
                parentDesignator.CompleteCapture(rect, c);
                Find.DesignatorManager.Deselect();
            }
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells) { }

        public override void SelectedUpdate()
        {
            // Draw the selection rectangle in a different color
            GenDraw.DrawFieldEdges(rect.Cells.ToList(), Color.yellow);

            // Get current mouse position to highlight potential origin point
            IntVec3 mouseCell = UI.MouseCell();
            if (rect.Contains(mouseCell))
            {
                GenDraw.DrawFieldEdges(new List<IntVec3> { mouseCell }, Color.green);
            }
        }
    }
    //public class Designator_AreaCapture : Designator
    //{
    //    private Vector3 startPos;
    //    private IntVec3 startCell;
    //    private bool dragging;
    //    private CellRect currentRect;

    //    public Designator_AreaCapture()
    //    {
    //        defaultLabel = "Capture Structure";
    //        defaultDesc = "Capture an area to XML format for direct pasting into defs.";
    //        icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn", true);
    //        useMouseIcon = true;
    //        soundDragSustain = SoundDefOf.Designate_DragStandard;
    //        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
    //    }

    //    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    //    {
    //        return c.InBounds(Map);
    //    }

    //    public override void DesignateSingleCell(IntVec3 c)
    //    {
    //        if (!dragging)
    //        {
    //            startPos = c.ToVector3Shifted();
    //            startCell = c;
    //            dragging = true;
    //            currentRect = new CellRect(c.x, c.z, 1, 1);
    //        }
    //        else
    //        {
    //            dragging = false;
    //            IntVec3 endCell = c;
    //            CellRect rect = CellRect.FromLimits(
    //                Math.Min(startCell.x, endCell.x),
    //                Math.Min(startCell.z, endCell.z),
    //                Math.Max(startCell.x, endCell.x),
    //                Math.Max(startCell.z, endCell.z));

    //            CaptureAreaToDefXml(rect);
    //        }
    //    }

    //    private void CaptureAreaToDefXml(CellRect rect)
    //    {
    //        // Create an XML document for output
    //        XmlDocument doc = new XmlDocument();

    //        // Create the root element as direct def XML
    //        XmlElement rootElement = doc.CreateElement("StructureLayoutDef");
    //        doc.AppendChild(rootElement);

    //        // Add basic Def properties
    //        XmlElement defNameElement = doc.CreateElement("defName");
    //        defNameElement.InnerText = "CapturedStructure_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //        rootElement.AppendChild(defNameElement);

    //        // Add size element
    //        XmlElement sizeElement = doc.CreateElement("size");
    //        rootElement.AppendChild(sizeElement);

    //        XmlElement sizeXElement = doc.CreateElement("x");
    //        sizeXElement.InnerText = rect.Width.ToString();
    //        sizeElement.AppendChild(sizeXElement);

    //        XmlElement sizeZElement = doc.CreateElement("z");
    //        sizeZElement.InnerText = rect.Height.ToString();
    //        sizeElement.AppendChild(sizeZElement);

    //        // Create stages list element
    //        XmlElement stagesElement = doc.CreateElement("stages");
    //        rootElement.AppendChild(stagesElement);

    //        // Create a single stage (we'll just capture everything as one stage)
    //        XmlElement stageElement = doc.CreateElement("li");
    //        stagesElement.AppendChild(stageElement);

    //        // Create the stage's inner elements for each category
    //        XmlElement terrainElement = doc.CreateElement("terrain");
    //        XmlElement wallsElement = doc.CreateElement("walls");
    //        XmlElement doorsElement = doc.CreateElement("doors");
    //        XmlElement powerElement = doc.CreateElement("power");
    //        XmlElement furnitureElement = doc.CreateElement("furniture");
    //        XmlElement otherElement = doc.CreateElement("other");

    //        stageElement.AppendChild(terrainElement);
    //        stageElement.AppendChild(wallsElement);
    //        stageElement.AppendChild(doorsElement);
    //        stageElement.AppendChild(powerElement);
    //        stageElement.AppendChild(furnitureElement);
    //        stageElement.AppendChild(otherElement);

    //        // Calculate the origin point (used to make coordinates relative)
    //        IntVec3 origin = new IntVec3(rect.minX, 0, rect.minZ);

    //        // Process all cells in the rectangle
    //        foreach (IntVec3 cell in rect.Cells)
    //        {
    //            // Calculate relative position
    //            IntVec3 relPos = cell - origin;

    //            // Handle terrain/floor
    //            TerrainDef terrain = Map.terrainGrid.TerrainAt(cell);
    //            if (terrain != null && terrain != TerrainDefOf.Soil && terrain != TerrainDefOf.Sand)
    //            {
    //                XmlElement terrainItemElement = doc.CreateElement("li");
    //                terrainElement.AppendChild(terrainItemElement);

    //                // Add terrain def reference
    //                XmlElement terrainDefElement = doc.CreateElement("terrain");
    //                terrainDefElement.InnerText = terrain.defName;
    //                terrainItemElement.AppendChild(terrainDefElement);

    //                // Add position
    //                XmlElement positionElement = doc.CreateElement("position");
    //                terrainItemElement.AppendChild(positionElement);

    //                XmlElement posXElement = doc.CreateElement("x");
    //                posXElement.InnerText = relPos.x.ToString();
    //                positionElement.AppendChild(posXElement);

    //                XmlElement posZElement = doc.CreateElement("z");
    //                posZElement.InnerText = relPos.z.ToString();
    //                positionElement.AppendChild(posZElement);
    //            }

    //            // Process things in the cell
    //            List<Thing> things = cell.GetThingList(Map);
    //            foreach (Thing thing in things)
    //            {
    //                // Skip certain types that we don't want to include
    //                if (thing is Pawn || thing is Filth || thing is Mote || thing is Corpse || thing is Plant)
    //                    continue;

    //                // Create the placement element
    //                XmlElement placementElement = doc.CreateElement("li");

    //                // Add the thing def reference
    //                XmlElement thingDefElement = doc.CreateElement("thing");
    //                thingDefElement.InnerText = thing.def.defName;
    //                placementElement.AppendChild(thingDefElement);

    //                // Add position
    //                XmlElement positionElement = doc.CreateElement("position");
    //                placementElement.AppendChild(positionElement);

    //                XmlElement posXElement = doc.CreateElement("x");
    //                posXElement.InnerText = relPos.x.ToString();
    //                positionElement.AppendChild(posXElement);

    //                XmlElement posZElement = doc.CreateElement("z");
    //                posZElement.InnerText = relPos.z.ToString();
    //                positionElement.AppendChild(posZElement);

    //                // Add rotation if not default
    //                if (thing.Rotation != Rot4.North)
    //                {
    //                    XmlElement rotationElement = doc.CreateElement("rotation");

    //                    // Convert rotation to North/East/South/West
    //                    string rotationName = "";
    //                    switch (thing.Rotation.AsInt)
    //                    {
    //                        case 0: rotationName = "North"; break;
    //                        case 1: rotationName = "East"; break;
    //                        case 2: rotationName = "South"; break;
    //                        case 3: rotationName = "West"; break;
    //                    }
    //                    rotationElement.InnerText = rotationName;
    //                    placementElement.AppendChild(rotationElement);
    //                }

    //                // Add stuff (building material) if applicable
    //                if (thing.Stuff != null)
    //                {
    //                    XmlElement stuffElement = doc.CreateElement("stuff");
    //                    stuffElement.InnerText = thing.Stuff.defName;
    //                    placementElement.AppendChild(stuffElement);
    //                }

    //                // Add to the appropriate category
    //                if (thing is Building_Door)
    //                {
    //                    doorsElement.AppendChild(placementElement);
    //                }
    //                else if (thing.def.fillPercent > 0.8f && thing.def.passability == Traversability.Impassable)
    //                {
    //                    wallsElement.AppendChild(placementElement);
    //                }
    //                else if (thing.def.building != null && thing.def.building.isPowerConduit)
    //                {
    //                    powerElement.AppendChild(placementElement);
    //                }
    //                else if (thing is Building && thing.def.building != null)
    //                {
    //                    furnitureElement.AppendChild(placementElement);
    //                }
    //                else
    //                {
    //                    otherElement.AppendChild(placementElement);
    //                }
    //            }
    //        }

    //        // Create a pretty-printed XML string
    //        string xmlString = FormatXml(doc);

    //        // Show in a window for copying
    //        ShowCapturedXml(xmlString);
    //    }

    //    private string FormatXml(XmlDocument doc)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        XmlWriterSettings settings = new XmlWriterSettings
    //        {
    //            Indent = true,
    //            IndentChars = "  ",
    //            NewLineChars = "\n",
    //            NewLineHandling = NewLineHandling.Replace
    //        };

    //        using (XmlWriter writer = XmlWriter.Create(sb, settings))
    //        {
    //            doc.Save(writer);
    //        }

    //        return sb.ToString();
    //    }

    //    protected virtual void ShowCapturedXml(string areaXml)
    //    {
    //        Find.WindowStack.Add(new Dialog_AreaCaptured(areaXml));
    //    }

    //    public override void DesignateMultiCell(IEnumerable<IntVec3> cells) { }

    //    // Draw the selection rectangle while dragging
    //    public override void DrawPanelReadout(ref float curY, float width)
    //    {
    //        if (dragging)
    //        {
    //            Widgets.Label(new Rect(0f, curY, width, 24f), "Selecting area...");
    //            curY += 24f;
    //        }
    //    }

    //    // Draw a visual overlay of the area being captured
    //    public override void SelectedUpdate()
    //    {
    //        base.SelectedUpdate();

    //        if (dragging)
    //        {
    //            // Get current mouse position
    //            IntVec3 mouseCell = UI.MouseCell();
    //            if (mouseCell.InBounds(Map))
    //            {
    //                // Update the current rectangle
    //                currentRect = CellRect.FromLimits(
    //                    Math.Min(startCell.x, mouseCell.x),
    //                    Math.Min(startCell.z, mouseCell.z),
    //                    Math.Max(startCell.x, mouseCell.x),
    //                    Math.Max(startCell.z, mouseCell.z));

    //                // Draw the selection rectangle
    //                GenDraw.DrawFieldEdges(currentRect.Cells.ToList(), Color.cyan);

    //                // Draw the center point marker
    //                IntVec3 center = currentRect.CenterCell;
    //                GenDraw.DrawFieldEdges(new List<IntVec3> { center }, Color.yellow);
    //            }
    //        }
    //    }
    //}
}
