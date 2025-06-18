using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenerator
    {
        //private readonly DungeonGenerationPipeline pipeline;
        private readonly Dungeon dungeon;
        private readonly DungeonGenDef parentGenStep;
        private readonly Map map;

        private readonly List<IDungeonGenerationStep> steps;
        private readonly DungeonGenerationContext context;

        public Dungeon GeneratedDungeon => dungeon;

        public DungeonGenerator(Map map, DungeonGenDef def)
        {
            this.parentGenStep = def;
            this.map = map;
            this.dungeon = new Dungeon(map);
            this.dungeon.Def = def;
   
            if (map.Parent is DungeonMapParent dungeonMapParent)
            {
                dungeonMapParent.SetDungeon(this.dungeon);
            }

            context = new DungeonGenerationContext(dungeon, dungeon.Def, map);
            steps = CreateGenerationSteps();
        }

        public void Generate()
        {
            Log.Message($"<color=cyan>Beginning Dungeon generation...</color>");

            foreach (var step in steps)
            {
                step.Execute(context);
            }


            Log.Message($"<color=cyan>Dungeon generation complete</color>");
        }

        private List<IDungeonGenerationStep> CreateGenerationSteps()
        {
            return new List<IDungeonGenerationStep>
            {
                new MapInitializationStep(),
                new BspStructureGenerationStep(),
                new RoomAssignmentStep(),
                new EarlyAutomataStep(),
                new MinimumSpanningTreeStep(),
                new ConnectionGenerationStep(),
                new CriticalPathDesignationStep(),
                new SidePathProcessingStep(),
                new RoomTypeAssignmentStep(),
                new GenerateCorridoorsStep(),
                new GridApplicationStep(),
                new DoorPlacementStep(),
                new PostAutomataStep(),
                new RoomWorkerApplicationStep(),
                new HiddenRoomSealingStep(),
                new ApplyFogStep()
            };
        }
    }
}