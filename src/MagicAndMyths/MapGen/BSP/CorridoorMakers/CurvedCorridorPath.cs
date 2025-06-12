using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CurvedCorridorPath : CorridorPathBase
    {
        public float curvature = 0.3f;
        
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();
            
            Vector2 startV = new Vector2(start.x, start.z);
            Vector2 endV = new Vector2(end.x, end.z);
            Vector2 direction = (endV - startV).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            
            float distance = Vector2.Distance(startV, endV);
            Vector2 controlPoint = Vector2.Lerp(startV, endV, 0.5f) + perpendicular * distance * curvature;
            
            int steps = Mathf.RoundToInt(distance * 1.5f);
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 point = QuadraticBezier(startV, controlPoint, endV, t);
                path.Add(new IntVec3(Mathf.RoundToInt(point.x), 0, Mathf.RoundToInt(point.y)));
            }
            
            return path.Distinct().ToList();
        }
        
        private Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }
    }
}
