using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class BspStructureGenerator
    {
        private readonly DungeonGenerationContext context;

        public BspStructureGenerator(DungeonGenerationContext context)
        {
            this.context = context;
        }

        public void Generate()
        {
            CellRect mapArea = new CellRect(
                context.MapMargin / 2,
                context.MapMargin / 2,
                context.Dungeon.Map.Size.x - context.MapMargin,
                context.Dungeon.Map.Size.z - context.MapMargin);

            int mainRoomCount = context.MainPathLength;
            int sideRoomcount = context.Def.sideRoomCount.RandomInRange;
            int minRoomsRequired = mainRoomCount + sideRoomcount;

            BspNode rootNode = BspUtility.GenerateBspTreeWithSideRooms(
                mapArea,
                totalRoomCount: minRoomsRequired,
                mainRoomCount: mainRoomCount,
                sideRoomCount: sideRoomcount,
                minRoomSize: context.Def.roomSize.RandomInRange,
                maxSplitAttempts: 400);

            List<BspNode> leafNodes = new List<BspNode>();
            BspUtility.GetLeafNodes(rootNode, leafNodes);

            foreach (var node in leafNodes)
            {
                if (node.HasTag("side_path"))
                {
                    context.Dungeon.AddSidePathNode(node);
                }
            }

            context.Dungeon.SetBspStructure(rootNode, leafNodes);
        }
    }
}