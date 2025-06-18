using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class OrganicCorridorPath : CorridorPathBase
    {
        public float noise = 0.2f;
        public int smoothingPasses = 2;

        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            List<IntVec3> path = new List<IntVec3>();

            int distance = Mathf.RoundToInt((start - end).LengthHorizontal);
            int steps = distance * 2;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;

                Vector2 basePoint = Vector2.Lerp(new Vector2(start.x, start.z), new Vector2(end.x, end.z), t);

                Vector2 noiseOffset = new Vector2(
                    Mathf.PerlinNoise(t * 10f, 0) - 0.5f,
                    Mathf.PerlinNoise(0, t * 10f) - 0.5f
                ) * noise * distance * Mathf.Sin(t * Mathf.PI);

                Vector2 noisyPoint = basePoint + noiseOffset;
                path.Add(new IntVec3(Mathf.RoundToInt(noisyPoint.x), 0, Mathf.RoundToInt(noisyPoint.y)));
            }

            return SmoothPath(path.Distinct().ToList(), smoothingPasses);
        }

        private List<IntVec3> SmoothPath(List<IntVec3> path, int passes)
        {
            for (int pass = 0; pass < passes; pass++)
            {
                List<IntVec3> smoothed = new List<IntVec3> { path[0] };

                for (int i = 1; i < path.Count - 1; i++)
                {
                    Vector3 prev = path[i - 1].ToVector3();
                    Vector3 curr = path[i].ToVector3();
                    Vector3 next = path[i + 1].ToVector3();

                    Vector3 smoothPoint = (prev + curr + next) / 3f;
                    smoothed.Add(new IntVec3(Mathf.RoundToInt(smoothPoint.x), 0, Mathf.RoundToInt(smoothPoint.z)));
                }

                smoothed.Add(path[path.Count - 1]);
                path = smoothed;
            }

            return path;
        }
    }

}
