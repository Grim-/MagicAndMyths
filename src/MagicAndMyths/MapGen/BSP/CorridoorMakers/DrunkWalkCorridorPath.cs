using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DrunkWalkCorridorPath : CorridorPathBase
    {
        public float drunkeness = 0.3f;
        public int maxSteps = 1000;

        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();
            IntVec3 current = start;
            path.Add(current);

            int steps = 0;
            while (current != end && steps < maxSteps)
            {
                Vector2 toTarget = new Vector2(end.x - current.x, end.z - current.z).normalized;
                Vector2 randomDir = new Vector2(Rand.Range(-1f, 1f), Rand.Range(-1f, 1f)).normalized;

                Vector2 finalDir = Vector2.Lerp(toTarget, randomDir, drunkeness).normalized;

                IntVec3 next = new IntVec3(
                    current.x + Mathf.RoundToInt(finalDir.x),
                    0,
                    current.z + Mathf.RoundToInt(finalDir.y)
                );

                if (next != current)
                {
                    path.Add(next);
                    current = next;
                }

                steps++;
            }

            // Ensure we reach the end
            if (current != end)
            {
                AddPointsAlongLine(path, current, end);
            }

            return path.Distinct().ToList();
        }
    }
}
