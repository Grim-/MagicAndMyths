using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenerator
    {
        //private readonly DungeonGenerationPipeline pipeline;
        private Dungeon dungeon;
        private readonly Map map;

        private readonly DungeonGenerationContext context;
        private List<IDungeonGenerationStep> generationSteps;
        private int currentStepIndex = 0;
        public bool GenerationComplete => currentStepIndex >= generationSteps.Count;
        public Dungeon GeneratedDungeon => dungeon;

        public DungeonGenerator(Map map, DungeonGenDef def)
        {
            this.map = map;
            this.dungeon = new Dungeon(map);
            this.dungeon.Def = def;

            context = new DungeonGenerationContext(dungeon, dungeon.Def, map);
            generationSteps = CreateGenerationSteps();
            if (map.Parent is DungeonMapParent dungeonMapParent)
            {
                dungeonMapParent.SetDungeon(this.dungeon);
                dungeonMapParent.DungeonGen = this;
            }
        }

        public void Generate()
        {
            // StepGeneration();
            Log.Message($"<color=cyan>Beginning Dungeon generation...</color>");


            int index = 0;
            foreach (var step in CreateGenerationSteps())
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

        public void Regenerate()
        {
            ResetGeneration();
            Generate();
        }

        public void StepGeneration()
        {
            if (GenerationComplete)
            {
                Log.Message("<color=green>Dungeon generation already complete.</color>");
                return;
            }

            var step = generationSteps[currentStepIndex];
            Log.Message($"<color=yellow>Executing Step {currentStepIndex + 1}: {step.GetType().Name}</color>");

            try
            {
                step.Execute(context);
            }
            catch (Exception e)
            {
                Log.Error($"<color=red>Dungeon Generation Error at Step {currentStepIndex + 1}</color>\n{e}");
            }

            if (dungeon != null)
            {
                GridApplicationStep.DrawGrid(context);
            }

            currentStepIndex++;

            if (GenerationComplete)
            {
                Log.Message("<color=cyan>Dungeon generation complete</color>");
            }
        }

        public void ResetGeneration()
        {
            currentStepIndex = 0;
            generationSteps = CreateGenerationSteps();
            dungeon = new Dungeon(map);
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