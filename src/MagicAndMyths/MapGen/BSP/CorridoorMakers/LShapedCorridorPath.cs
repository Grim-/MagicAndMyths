using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class LShapedCorridorPath : CorridorPathBase
    {
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();

            bool horizontalFirst = Rand.Value > 0.5f;

            IntVec3 corner;
            if (horizontalFirst)
                corner = new IntVec3(end.x, 0, start.z);
            else
                corner = new IntVec3(start.x, 0, end.z);

            AddPointsAlongLine(path, start, corner);

            AddPointsAlongLine(path, corner, end);
            return path;
        }

    }
}
