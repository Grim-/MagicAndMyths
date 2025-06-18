using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DoubleLCorridorPath : CorridorPathBase
    {
        public float midpointOffset = 0.5f;
        
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();
            
            float t = midpointOffset;
            int midX = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int midZ = Mathf.RoundToInt(Mathf.Lerp(start.z, end.z, t));
            
            IntVec3 corner1 = new IntVec3(midX, 0, start.z);
            IntVec3 corner2 = new IntVec3(midX, 0, end.z);
            
            AddPointsAlongLine(path, start, corner1);
            AddPointsAlongLine(path, corner1, corner2);
            AddPointsAlongLine(path, corner2, end);
            
            return path.Distinct().ToList();
        }
    }
}
