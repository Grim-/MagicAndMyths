using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ZigzagCorridorPath : CorridorPathBase
    {
        public int segments = 3;
        public float zigzagOffset = 2f;


        public override bool FitnessTest(IntVec3 start, IntVec3 end, Map map)
        {
            if (start.DistanceTo(end) < 5)
            {
                return false;
            }

            return base.FitnessTest(start, end, map);
        }

        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();
            
            Vector2 startV = new Vector2(start.x, start.z);
            Vector2 endV = new Vector2(end.x, end.z);
            Vector2 direction = (endV - startV).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            
            List<Vector2> waypoints = new List<Vector2> { startV };
            
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePoint = Vector2.Lerp(startV, endV, t);
                float offset = (i % 2 == 0 ? 1 : -1) * zigzagOffset;
                Vector2 zigzagPoint = basePoint + perpendicular * offset;
                waypoints.Add(zigzagPoint);
            }
            
            waypoints.Add(endV);
            
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                IntVec3 wpStart = new IntVec3(Mathf.RoundToInt(waypoints[i].x), 0, Mathf.RoundToInt(waypoints[i].y));
                IntVec3 wpEnd = new IntVec3(Mathf.RoundToInt(waypoints[i + 1].x), 0, Mathf.RoundToInt(waypoints[i + 1].y));
                AddPointsAlongLine(path, wpStart, wpEnd);
            }
            
            return path.Distinct().ToList();
        }
    }
}
