using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public abstract class CorridorPathBase
    {
        public abstract List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map);

        protected void AddPointsAlongLine(List<IntVec3> path, IntVec3 start, IntVec3 end)
        {
            int dx = end.x - start.x;
            int dz = end.z - start.z;

            int steps = Math.Max(Math.Abs(dx), Math.Abs(dz));

            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0 : (float)i / steps;
                int x = start.x + (int)Math.Round(dx * t);
                int z = start.z + (int)Math.Round(dz * t);

                path.Add(new IntVec3(x, 0, z));
            }
        }
    }
}
