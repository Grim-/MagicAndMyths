using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Verse;

namespace MagicAndMyths
{
    public static class XMLUtil
    {

        public static string CaptureAreaToDefXml(Map map, CellRect rect, IntVec3 origin)
        {
            // Create an XML document for output
            XmlDocument doc = new XmlDocument();

            // Create the root element as direct def XML
            XmlElement rootElement = doc.CreateElement("MagicAndMyths.StructureLayoutDef");
            doc.AppendChild(rootElement);

            // Add basic Def properties
            XmlElement defNameElement = doc.CreateElement("defName");
            defNameElement.InnerText = "CapturedStructure_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            rootElement.AppendChild(defNameElement);

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

            // Track things we've already processed to avoid duplicates
            HashSet<int> processedThingIds = new HashSet<int>();

            // Calculate positions relative to the selected origin cell
            foreach (IntVec3 cell in rect.Cells)
            {
                // Calculate relative position from the selected origin
                IntVec3 relPos = cell - origin;

                // Handle terrain/floor
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
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
                List<Thing> things = cell.GetThingList(map);
                foreach (Thing thing in things)
                {
                    // Skip certain types that we don't want to include
                    if (thing is Pawn || thing is Filth || thing is Mote || thing is Corpse || thing is Plant)
                        continue;

                    // Skip if we've already processed this thing
                    if (processedThingIds.Contains(thing.thingIDNumber))
                        continue;

                    // Only add the thing once - check if current cell is the "origin" cell of the thing
                    // For multi-tile things, only process when we're at the first/origin cell
                    if (cell != thing.Position)
                        continue;

                    // Mark this thing as processed
                    processedThingIds.Add(thing.thingIDNumber);

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

                    XmlElement thingSizeElement = doc.CreateElement("thingSize");
                    thingSizeElement.InnerText = thing.def.Size.ToStringSafe();
                    placementElement.AppendChild(thingSizeElement);

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

                    if (thing.Stuff != null)
                    {
                        XmlElement stuffElement = doc.CreateElement("stuff");
                        stuffElement.InnerText = thing.Stuff.defName;
                        placementElement.AppendChild(stuffElement);
                    }

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

            return FormatXml(doc);
        }

        //public static string CaptureAreaToDefXml(Map map, CellRect rect, IntVec3 origin)
        //{
        //    // Create an XML document for output
        //    XmlDocument doc = new XmlDocument();

        //    // Create the root element as direct def XML
        //    XmlElement rootElement = doc.CreateElement("MagicAndMyths.StructureLayoutDef");
        //    doc.AppendChild(rootElement);

        //    // Add basic Def properties
        //    XmlElement defNameElement = doc.CreateElement("defName");
        //    defNameElement.InnerText = "CapturedStructure_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //    rootElement.AppendChild(defNameElement);

        //    // Create stages list element
        //    XmlElement stagesElement = doc.CreateElement("stages");
        //    rootElement.AppendChild(stagesElement);

        //    // Add size element
        //    XmlElement sizeElement = doc.CreateElement("size");
        //    rootElement.AppendChild(sizeElement);

        //    XmlElement sizeXElement = doc.CreateElement("x");
        //    sizeXElement.InnerText = rect.Width.ToString();
        //    sizeElement.AppendChild(sizeXElement);

        //    XmlElement sizeZElement = doc.CreateElement("z");
        //    sizeZElement.InnerText = rect.Height.ToString();
        //    sizeElement.AppendChild(sizeZElement);

        //    // Create a single stage (we'll just capture everything as one stage)
        //    XmlElement stageElement = doc.CreateElement("li");
        //    stagesElement.AppendChild(stageElement);

        //    // Create the stage's inner elements for each category
        //    XmlElement terrainElement = doc.CreateElement("terrain");
        //    XmlElement wallsElement = doc.CreateElement("walls");
        //    XmlElement doorsElement = doc.CreateElement("doors");
        //    XmlElement powerElement = doc.CreateElement("power");
        //    XmlElement furnitureElement = doc.CreateElement("furniture");
        //    XmlElement otherElement = doc.CreateElement("other");

        //    stageElement.AppendChild(terrainElement);
        //    stageElement.AppendChild(wallsElement);
        //    stageElement.AppendChild(doorsElement);
        //    stageElement.AppendChild(powerElement);
        //    stageElement.AppendChild(furnitureElement);
        //    stageElement.AppendChild(otherElement);

        //    // Calculate positions relative to the selected origin cell
        //    foreach (IntVec3 cell in rect.Cells)
        //    {
        //        // Calculate relative position from the selected origin
        //        IntVec3 relPos = cell - origin;

        //        // Handle terrain/floor
        //        TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
        //        if (terrain != null && terrain != TerrainDefOf.Soil && terrain != TerrainDefOf.Sand)
        //        {
        //            XmlElement terrainItemElement = doc.CreateElement("li");
        //            terrainElement.AppendChild(terrainItemElement);

        //            // Add terrain def reference
        //            XmlElement terrainDefElement = doc.CreateElement("terrain");
        //            terrainDefElement.InnerText = terrain.defName;
        //            terrainItemElement.AppendChild(terrainDefElement);

        //            // Add position
        //            XmlElement positionElement = doc.CreateElement("position");
        //            terrainItemElement.AppendChild(positionElement);

        //            XmlElement posXElement = doc.CreateElement("x");
        //            posXElement.InnerText = relPos.x.ToString();
        //            positionElement.AppendChild(posXElement);

        //            XmlElement posZElement = doc.CreateElement("z");
        //            posZElement.InnerText = relPos.z.ToString();
        //            positionElement.AppendChild(posZElement);
        //        }

        //        // Process things in the cell
        //        List<Thing> things = cell.GetThingList(map);
        //        foreach (Thing thing in things)
        //        {
        //            // Skip certain types that we don't want to include
        //            if (thing is Pawn || thing is Filth || thing is Mote || thing is Corpse || thing is Plant)
        //                continue;

        //            // Create the placement element
        //            XmlElement placementElement = doc.CreateElement("li");

        //            // Add the thing def reference
        //            XmlElement thingDefElement = doc.CreateElement("thing");
        //            thingDefElement.InnerText = thing.def.defName;
        //            placementElement.AppendChild(thingDefElement);

        //            // Add position
        //            XmlElement positionElement = doc.CreateElement("position");
        //            placementElement.AppendChild(positionElement);

        //            XmlElement posXElement = doc.CreateElement("x");
        //            posXElement.InnerText = relPos.x.ToString();
        //            positionElement.AppendChild(posXElement);

        //            XmlElement posZElement = doc.CreateElement("z");
        //            posZElement.InnerText = relPos.z.ToString();
        //            positionElement.AppendChild(posZElement);


        //            XmlElement thingSizeElement = doc.CreateElement("thingSize");
        //            thingSizeElement.InnerText = thing.def.Size.ToStringSafe();
        //            placementElement.AppendChild(thingSizeElement);

        //            // Add rotation if not default
        //            if (thing.Rotation != Rot4.North)
        //            {
        //                XmlElement rotationElement = doc.CreateElement("rotation");

        //                // Convert rotation to North/East/South/West
        //                string rotationName = "";
        //                switch (thing.Rotation.AsInt)
        //                {
        //                    case 0: rotationName = "North"; break;
        //                    case 1: rotationName = "East"; break;
        //                    case 2: rotationName = "South"; break;
        //                    case 3: rotationName = "West"; break;
        //                }
        //                rotationElement.InnerText = rotationName;
        //                placementElement.AppendChild(rotationElement);
        //            }

        //            if (thing.Stuff != null)
        //            {
        //                XmlElement stuffElement = doc.CreateElement("stuff");
        //                stuffElement.InnerText = thing.Stuff.defName;
        //                placementElement.AppendChild(stuffElement);
        //            }

        //            if (thing is Building_Door)
        //            {
        //                doorsElement.AppendChild(placementElement);
        //            }
        //            else if (thing.def.fillPercent > 0.8f && thing.def.passability == Traversability.Impassable)
        //            {
        //                wallsElement.AppendChild(placementElement);
        //            }
        //            else if (thing.def.building != null && thing.def.building.isPowerConduit)
        //            {
        //                powerElement.AppendChild(placementElement);
        //            }
        //            else if (thing is Building && thing.def.building != null)
        //            {
        //                furnitureElement.AppendChild(placementElement);
        //            }
        //            else
        //            {
        //                otherElement.AppendChild(placementElement);
        //            }
        //        }
        //    }

        //    return FormatXml(doc);
        //}

        public static string FormatXml(XmlDocument doc)
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
    }
}
