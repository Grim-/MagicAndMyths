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
        private readonly Map map;

        private readonly List<IDungeonGenerationStep> steps;
        private readonly DungeonGenerationContext context;

        public Dungeon GeneratedDungeon => dungeon;

        public DungeonGenerator(Map map, DungeonGenDef def)
        {
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


            int index = 0;
            foreach (var step in steps)
            {
                Log.Message($"<color=cyan>Step {index + 1} {step.GetType()}</color>");
                try
                {
                    step.Execute(context);
                    index++;
                }
                catch (Exception e)
                {
                    Log.Message($"<color=red>Dungeon Generation Error at Step {index + 1}</color>");
                    Log.Error(e.Message);
                    continue;
                }
            }


            Log.Message($"<color=cyan>Dungeon generation complete</color>");
        }

        private List<IDungeonGenerationStep> CreateGenerationSteps()
        {
            return new List<IDungeonGenerationStep>
            {
                new MapInitializationStep(),
                new BspStructureGenerationStep(),
                new RoomCreationStep(),
                new EarlyAutomataStep(),
                new MinimumSpanningTreeStep(),
                new ConnectionGenerationStep(),
                new CriticalPathDesignationStep(),
                new SidePathProcessingStep(),
                new RoomTypeAssignmentStep(),
                new SideRoomTypeUpdateStep(),
                new GenerateCorridoorsStep(),
                new GridApplicationStep(),
                new DoorPlacementStep(),
                new PostAutomataStep(),
                new RoomWorkerApplicationStep(),
                new HiddenRoomSealingStep()
                //new ApplyFogStep()
            };
        }
    }
}