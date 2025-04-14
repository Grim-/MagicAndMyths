using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonMapParent : ExtendedMapParent
    {
        private List<MapModifier> activeModifiers = new List<MapModifier>();
        public bool ShouldDestroy = false;


        private WorldComp_DungeonManager _dungeonManager;
        private WorldComp_DungeonManager DungeonManager
        {
            get
            {
                if (_dungeonManager == null)
                {
                    _dungeonManager = Find.World.GetComponent<WorldComp_DungeonManager>();
                }
                return _dungeonManager;
            }
        }
        protected int dungeonID = -1;
        public int DungeonID => dungeonID;
        protected Dungeon GeneratedDungeon = null;

        public bool HasColonistLimit = false;
        public int ColonistLimit = 10;

        public void SetDungeonID(int ID)
        {
            dungeonID = ID;
        }

        public void SetOriginMap(Map map)
        {
            OriginMap = map;
        }

        public void SetDungeon(Dungeon dungeon)
        {
            GeneratedDungeon = dungeon;
        }

        public override void Tick()
        {
            base.Tick();

            if (this.Map != null)
            {
                foreach (var modifier in activeModifiers)
                {
                    modifier.Tick();
                }
            }
        }

        public override void MoveToMap(Pawn pawn)
        {
            if (pawn.IsColonist && !CanEnter(pawn, out string denialReason))
            {
                Messages.Message($"Cannot teleport {pawn.LabelShort}: {denialReason}",
                                MessageTypeDefOf.RejectInput);
                return;
            }


            IntVec3 entryPoint;

            if (GeneratedDungeon != null && GeneratedDungeon.StartNode != null)
            {
                DungeonRoom startRoom = GeneratedDungeon.GetRoom(GeneratedDungeon.StartNode);
                if (startRoom != null)
                {
                    entryPoint = startRoom.roomCellRect.RandomCell;
                }
                else
                {
                    entryPoint = CellFinder.RandomSpawnCellForPawnNear(Map.Center, Map, 10);
                }
            }
            else
            {
                entryPoint = CellFinder.RandomSpawnCellForPawnNear(Map.Center, Map, 10);
            }

            if (entryPoint.IsValid)
            {
                if (pawn.Spawned)
                    pawn.DeSpawn(DestroyMode.Vanish);

                GenSpawn.Spawn(pawn, entryPoint, this.Map);
            }
        }

        public void AddModifier(MapModifier modifier)
        {
            activeModifiers.Add(modifier);
        }

        public void RemoveModifier(MapModifier modifier)
        {
            activeModifiers.Remove(modifier);
        }

        public int GetCurrentColonistCount()
        {
            if (!HasMap)
                return 0;

            return Map.mapPawns.AllPawns.Where(x=> x.Faction == Faction.OfPlayer).Count();
        }

        public override bool CanEnter(Thing thing, out string denialReason)
        {
            denialReason = String.Empty;

            if (thing is Pawn pawn)
            {
                if (HasColonistLimit && GetCurrentColonistCount() >= ColonistLimit)
                {
                    denialReason = "at maximum capacity";
                    return false;
                }
            }
            return base.CanEnter(thing, out denialReason);
        }


        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = true;
            return ShouldDestroy && !this.Map.mapPawns.AnyPawnBlockingMapRemoval;
        }



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref HasColonistLimit, "hasColonistLimit", false);
            Scribe_Values.Look(ref ColonistLimit, "colonistLimit", 10);
            Scribe_Values.Look(ref ShouldDestroy, "shouldDestroy", false);
            Scribe_Values.Look(ref dungeonID, "DungeonID");
            Scribe_Collections.Look(ref activeModifiers, "activeModifiers", LookMode.Deep);
        }

    }
}
