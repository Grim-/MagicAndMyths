using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    // Jagged Corridor 
    public class JaggedCorridorPath : CorridorPathBase
    {
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();

            int dx = end.x - start.x;
            int dz = end.z - start.z;

            int steps = Math.Max(Math.Abs(dx), Math.Abs(dz));

            // Noise factor is just a percentage of the total distance
            int maxDeviation = Math.Max(1, steps / 5);

            path.Add(start);

            for (int i = 1; i < steps; i++)
            {
                float t = (float)i / steps;
                int baseX = start.x + (int)Math.Round(dx * t);
                int baseZ = start.z + (int)Math.Round(dz * t);

                // 70% chance to add noise at each point
                if (Rand.Value < 0.7f)
                {
                    baseX += Rand.RangeInclusive(-maxDeviation, maxDeviation);
                    baseZ += Rand.RangeInclusive(-maxDeviation, maxDeviation);
                }

                path.Add(new IntVec3(baseX, 0, baseZ));
            }

            path.Add(end);

            return path;
        }
    }
}
