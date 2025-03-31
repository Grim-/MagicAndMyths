using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenManager
    {
        private readonly Map map;
        private readonly List<DungeonGen> generators;

        public DungeonGenManager(Map map)
        {
            this.map = map;
            this.generators = new List<DungeonGen>();
        }

        public void AddGenerator(DungeonGen generator)
        {
            generators.Add(generator);
        }

        public void Generate()
        {
            Log.Message($"Dugneon Gen Manager :: Generate");

            // Sort by priority and run each generator
            foreach (var generator in generators.OrderBy(g => g.Priority))
            {
                Log.Message($"Dugneon Gen Manager :: Running {generator.GetType()}...");
                generator.Generate();
            }
        }
    }
}

