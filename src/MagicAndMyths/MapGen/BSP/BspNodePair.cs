using Verse;

namespace MagicAndMyths
{
    public class BspNodePair : IExposable
    {
        public BspNode NodeOne;
        public BspNode NodeTwo;

        public BspNodePair()
        {

        }

        public BspNodePair(BspNode nodeOne, BspNode nodeTwo)
        {
            NodeOne = nodeOne;
            NodeTwo = nodeTwo;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref NodeOne, "NodeOne");
            Scribe_Deep.Look(ref NodeTwo, "NodeTwo");
        }
    }


}