using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class BspNode : IExposable
    {
        public CellRect rect;
        public BspNode left;
        public BspNode right;
        public List<string> tags = new List<string>();
        public List<BspNode> connectedNodes = new List<BspNode>();
        public CellRect roomRect;
        public RoomShapeBase roomShape;
        public List<IntVec3> roomCells;
        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public void AddTag(string tag)
        {
            if (tags == null)
            {
                tags = new List<string>();
            }

            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        public bool HasTag(string tag)
        {
            return tags != null && tags.Contains(tag);
        }

        public void GenerateComplexRoomGeometry(DungeonGenerationContext context, RoomLayoutData roomLayoutData, int minPadding = 1, float roomSizeFactor = 1f)
        {
            //if (shapeGenerator == null)
            //{
            //    shapeGenerator = GetRandomRoomShape();
            //}

            roomShape = roomLayoutData.perferredLayouts.RandomElement().GetWorker();

            // Calculate room size using the size factor
            int roomWidth = (int)(rect.Width * roomSizeFactor);
            int roomHeight = (int)(rect.Height * roomSizeFactor);

            int minWidth = (int)(roomLayoutData.minSizeRequired.x);
            int minHeight = (int)(roomLayoutData.minSizeRequired.z);

            roomWidth = Math.Max(minWidth, roomWidth);
            roomHeight = Math.Max(minHeight, roomHeight);

            roomWidth = Math.Min(roomWidth, rect.Width - (minPadding * 2));
            roomHeight = Math.Min(roomHeight, rect.Height - (minPadding * 2));

            int roomX = rect.minX + minPadding + (rect.Width - (minPadding * 2) - roomWidth) / 2;
            int roomZ = rect.minZ + minPadding + (rect.Height - (minPadding * 2) - roomHeight) / 2;

            CellRect roomBounds = new CellRect(roomX, roomZ, roomWidth, roomHeight);

            roomCells = roomShape.GenerateRoomCells(context, roomBounds, UnityEngine.Random.Range(0.4f, 1f));
            int minX = roomCells.Min(c => c.x);
            int maxX = roomCells.Max(c => c.x);
            int minZ = roomCells.Min(c => c.z);
            int maxZ = roomCells.Max(c => c.z);
            roomRect = CellRect.FromCellList(roomCells);
        }

        private RoomShapeBase GetRandomRoomShape()
        {
             RoomShapeBase[] shapes = {
                new RectangleRoomShape(),
                new CircularRoomShape(),
                new CrossRoomShape(),
                new BlobRoomShape()
            };

            return shapes.RandomElement();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref rect, "rect");
            Scribe_Values.Look(ref roomRect, "roomRect");
            Scribe_Deep.Look(ref left, "left");
            Scribe_Deep.Look(ref right, "right");

            Scribe_Collections.Look(ref tags, "tags", LookMode.Value);
            Scribe_Collections.Look(ref connectedNodes, "connectedNodes", LookMode.Reference);
        }
    }

}
