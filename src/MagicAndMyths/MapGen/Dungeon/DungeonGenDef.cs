using System;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenDef : Def
    {
        public Type generatorWorkerClass;

        public DungeonGen CreateGenerator(Map map)
        {
            var generator = (DungeonGen)Activator.CreateInstance(generatorWorkerClass);
            generator.map = map;
            return generator;
        }
    }
}
