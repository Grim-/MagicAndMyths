using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class ApplyFogStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            context.Map.fogGrid.Refog(CellRect.FromCellList(context.Dungeon.Map.AllCells.Except(context.Dungeon.Rooms.First(x => x.def.roomType == RoomType.Start).roomCells)));
        }
    }
}
