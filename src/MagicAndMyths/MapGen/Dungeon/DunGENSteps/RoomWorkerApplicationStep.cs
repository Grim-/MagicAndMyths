using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class RoomWorkerApplicationStep : IDungeonGenerationStep
    {

        private Queue<DungeonRoom> retries = new Queue<DungeonRoom>();

        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Applying Room workers to rooms");
            ApplyRoomWorkers(context);
        }

        public void ApplyRoomWorkers(DungeonGenerationContext context)
        {
            foreach (var room in context.Dungeon.GetAllRooms())
            {
                if (room.def != null)
                {
                    if (room.def.CanApply(context, room))
                    {
                        room.def.DoWorker(context, room);
                    }
                    else
                    {
                        retries.Enqueue(room);
                    }
                   
                }
            }

            ////remove def so the type is retried in assignment
            //foreach (var item in retries)
            //{
            //    item.def = null;
            //}


            //new RoomTypeAssignmentStep().Execute(context);
        }
    }
}
