using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class LShapedCorridorPath : CorridorPathBase
    {
        public bool preferHorizontalFirst = false;
        public float cornerRounding = 0f;

        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();

            bool horizontalFirst = preferHorizontalFirst ? true : Rand.Value > 0.5f;
            IntVec3 corner;

            if (horizontalFirst)
                corner = new IntVec3(end.x, 0, start.z);
            else
                corner = new IntVec3(start.x, 0, end.z);

            if (cornerRounding > 0)
            {
                path.AddRange(GenerateRoundedLShape(start, corner, end, cornerRounding));
            }
            else
            {
                AddPointsAlongLine(path, start, corner);
                AddPointsAlongLine(path, corner, end);
            }

            return path.Distinct().ToList();
        }

        private List<IntVec3> GenerateRoundedLShape(IntVec3 start, IntVec3 corner, IntVec3 end, float radius)
        {
            List<IntVec3> path = new List<IntVec3>();

            Vector2 dir1 = new Vector2(corner.x - start.x, corner.z - start.z).normalized;
            Vector2 dir2 = new Vector2(end.x - corner.x, end.z - corner.z).normalized;

            int roundRadius = Mathf.RoundToInt(radius);
            IntVec3 roundStart = new IntVec3(
                corner.x - Mathf.RoundToInt(dir1.x * roundRadius),
                0,
                corner.z - Mathf.RoundToInt(dir1.y * roundRadius)
            );
            IntVec3 roundEnd = new IntVec3(
                corner.x + Mathf.RoundToInt(dir2.x * roundRadius),
                0,
                corner.z + Mathf.RoundToInt(dir2.y * roundRadius)
            );

            AddPointsAlongLine(path, start, roundStart);

            // Add rounded corner
            for (int i = 0; i <= roundRadius; i++)
            {
                float t = (float)i / roundRadius;
                Vector2 roundPoint = Vector2.Lerp(
                    new Vector2(roundStart.x, roundStart.z),
                    new Vector2(roundEnd.x, roundEnd.z),
                    t
                );
                path.Add(new IntVec3(Mathf.RoundToInt(roundPoint.x), 0, Mathf.RoundToInt(roundPoint.y)));
            }

            AddPointsAlongLine(path, roundEnd, end);

            return path;
        }
    }
}
