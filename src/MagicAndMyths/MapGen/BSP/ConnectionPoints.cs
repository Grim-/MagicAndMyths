using Verse;

namespace MagicAndMyths
{
    public struct ConnectionPoints
    {
        public IntVec3 Start;
        public IntVec3 End;

        public ConnectionPoints(IntVec3 start, IntVec3 end)
        {
            Start = start;
            End = end;
        }
    }
}
