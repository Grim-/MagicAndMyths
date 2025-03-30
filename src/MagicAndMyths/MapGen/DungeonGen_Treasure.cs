using Verse;

namespace MagicAndMyths
{
    public class DungeonGen_Treasure : DungeonGen
    {
        public override int Priority => 40;
        public DungeonGen_Treasure(Map map) : base(map) { }
        public override void Generate() 
        {

        }
    }
}

