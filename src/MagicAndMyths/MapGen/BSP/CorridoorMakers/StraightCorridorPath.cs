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
            List<IntVec3> path = new List<IntVec3>();
            int dx = end.x - start.x;
            int dz = end.z - start.z;
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dz));

            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0 : (float)i / steps;

                int x = start.x + (int)Math.Round(dx * t);
                int z = start.z + (int)Math.Round(dz * t);

                // Add jitter if enabled
                if (jitter > 0 && i > 0 && i < steps)
                {
                    x += Rand.Range(-(int)jitter, (int)jitter + 1);
                    z += Rand.Range(-(int)jitter, (int)jitter + 1);
                }

                path.Add(new IntVec3(x, 0, z));
            }

            return path;
        }
    }
}
