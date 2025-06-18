using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class StraightCorridorPath : CorridorPathBase
    {
        public float jitter = 0f;

        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            return GenSight.BresenhamCellsBetween(start, end);
        }
    }
}
