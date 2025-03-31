using Verse;

namespace MagicAndMyths
{
    public class DungeonGen_Buildings : DungeonGen
    {
        public override int Priority => 20;
        public DungeonGen_Buildings(Map map) : base(map) { }
        public override void Generate()
        { 
            
        }
    }
}

