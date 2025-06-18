//using System.Collections.Generic;
//using Verse;

//namespace MagicAndMyths
//{
//    public class DungeonGenerationPipeline
//    {
//        private readonly List<IDungeonGenerationStep> steps;
//        private readonly DungeonGenerationContext context;

//        public DungeonGenerationPipeline(Dungeon dungeon, DungeonGenDef def, Map map)
//        {
//            context = new DungeonGenerationContext(dungeon, def, map);
//            steps = CreateGenerationSteps();
//        }

//        private List<IDungeonGenerationStep> CreateGenerationSteps()
//        {
//            return new List<IDungeonGenerationStep>
//            {
//                new MapInitializationStep(),
//                new BspStructureGenerationStep(),
//                new PlannedRoomProcessingStep(),
//                new RoomCreationStep(),
//                new EarlyAutomataStep(),
//                new MinimumSpanningTreeStep(),
//                new ConnectionGenerationStep(),
//                new CriticalPathDesignationStep(),
//                new SidePathProcessingStep(),
//                new RoomTypeAssignmentStep(),
//                new GridApplicationStep(),
//                new PostAutomataStep(),
//                new RoomWorkerApplicationStep()
//            };
//        }

//        public void Execute()
//        {
//            foreach (var step in steps)
//            {
//                step.Execute(context);
//            }
//        }
//    }
//}
