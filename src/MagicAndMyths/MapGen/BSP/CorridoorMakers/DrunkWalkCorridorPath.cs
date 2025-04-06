using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    // Drunk Walk Corridor
    public class DrunkWalkCorridorPath : CorridorPathBase
    {
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3> { start };
            IntVec3 current = start;

            // Cardinal directions
            IntVec3[] directions = new IntVec3[] {
                new IntVec3(0, 0, 1),   // North
                new IntVec3(1, 0, 0),   // East
                new IntVec3(0, 0, -1),  // South
                new IntVec3(-1, 0, 0)   // West
            };

            int maxSteps = (int)(start.DistanceTo(end) * 3);
            int steps = 0;

            while (current != end && steps < maxSteps)
            {
                steps++;

                // 70% chance of random move, 30% chance of directed move
                if (Rand.Value < 0.7f)
                {
                    // Random move in cardinal direction
                    IntVec3 dir = directions[Rand.RangeInclusive(0, 3)];
                    current += dir;
                }
                else
                {
                    // Move toward end
                    IntVec3 toEnd = end - current;

                    if (Math.Abs(toEnd.x) > Math.Abs(toEnd.z))
                        current += new IntVec3(Math.Sign(toEnd.x), 0, 0);
                    else
                        current += new IntVec3(0, 0, Math.Sign(toEnd.z));
                }

                path.Add(current);

                // If we're close to the end, make a direct line
                if (current.DistanceTo(end) <= 3)
                {
                    // Add direct line to end
                    int endDx = end.x - current.x;
                    int endDz = end.z - current.z;
                    int endSteps = Math.Max(Math.Abs(endDx), Math.Abs(endDz));

                    for (int i = 1; i <= endSteps; i++)
                    {
                        float t = (float)i / endSteps;
                        int x = current.x + (int)Math.Round(endDx * t);
                        int z = current.z + (int)Math.Round(endDz * t);

                        path.Add(new IntVec3(x, 0, z));
                    }

                    break;
                }
            }

            return path;
        }
    }
}
