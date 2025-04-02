using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class Corridoor
    {
        public IntVec3 Start;
        public IntVec3 End;
        public IntVec3 RoomAEntryPoint;
        public IntVec3 RoomBEntryPoint;
        public List<IntVec3> path;

        public Corridoor(IntVec3 start, IntVec3 end)
        {
            Start = start;
            End = end;
            path = GeneratePath();
            RoomAEntryPoint = Start;
            RoomBEntryPoint = End;
        }

        private List<IntVec3> GeneratePath()
        {
            List<IntVec3> result = new List<IntVec3>();

            int x = Start.x;
            int z = Start.z;
            int dx = System.Math.Abs(End.x - Start.x);
            int dz = System.Math.Abs(End.z - Start.z);
            int sx = Start.x < End.x ? 1 : -1;
            int sz = Start.z < End.z ? 1 : -1;
            int err = dx - dz;

            result.Add(new IntVec3(x, Start.y, z));

            while (x != End.x || z != End.z)
            {
                int e2 = 2 * err;
                if (e2 > -dz)
                {
                    err -= dz;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    z += sz;
                }
                result.Add(new IntVec3(x, Start.y, z));
            }

            return result;
        }
    }
}
