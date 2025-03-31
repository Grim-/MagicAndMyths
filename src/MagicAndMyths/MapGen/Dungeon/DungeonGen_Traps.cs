using Verse;

namespace MagicAndMyths
{
    public class DungeonGen_Traps : DungeonGen
    {
        public override int Priority => 30;
        public DungeonGen_Traps(Map map) : base(map) { }
        public override void Generate()     
        {

        }
    }
}

