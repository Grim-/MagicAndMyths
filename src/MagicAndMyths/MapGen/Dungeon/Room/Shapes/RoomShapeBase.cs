using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{

    public class RoomLayoutDef : Def
    {
        public Type workerClass;
        public RoomLayoutProps layoutProps;

        public RoomShapeBase GetWorker()
        {
            RoomShapeBase roomShapeBase = (RoomShapeBase)Activator.CreateInstance(workerClass);
            roomShapeBase.Def = this;
            return roomShapeBase;
        }
    }


    public class RoomLayoutProps
    {
        public float layoutSizeMultiplier = 1f;
    }

    public abstract class RoomShapeBase
    {
        public RoomLayoutDef Def;

        public abstract List<IntVec3> GenerateRoomCells(DungeonGenerationContext context, CellRect bounds, float sizeMultiplier);
    }
}
