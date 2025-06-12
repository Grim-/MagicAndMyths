using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class BranchingCorridorPath : CorridorPathBase
    {
        public int branchCount = 2;
        public float branchLength = 5f;
        
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();
            
            // Main spine
            AddPointsAlongLine(path, start, end);
            
            // Add branches
            for (int i = 0; i < branchCount; i++)
            {
                float t = (float)(i + 1) / (branchCount + 1);
                IntVec3 branchPoint = new IntVec3(
                    Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t)),
                    0,
                    Mathf.RoundToInt(Mathf.Lerp(start.z, end.z, t))
                );
                
                Vector2 direction = new Vector2(end.x - start.x, end.z - start.z).normalized;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                
                float side = (i % 2 == 0) ? 1f : -1f;
                IntVec3 branchEnd = new IntVec3(
                    Mathf.RoundToInt(branchPoint.x + perpendicular.x * branchLength * side),
                    0,
                    Mathf.RoundToInt(branchPoint.z + perpendicular.y * branchLength * side)
                );
                
                AddPointsAlongLine(path, branchPoint, branchEnd);
            }
            
            return path.Distinct().ToList();
        }
    }
}
