using Verse;

namespace MagicAndMyths
{
    public abstract class DungeonGen
    {
        public Map map;

        public virtual int Priority => 100; 

        public DungeonGen(Map map)
        {
            this.map = map;
        }

        public abstract void Generate();
    }
}

