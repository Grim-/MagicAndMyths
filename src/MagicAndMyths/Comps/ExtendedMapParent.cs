using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class ExtendedMapParent : MapParent
    {
        protected Map OriginMap = null;

        public virtual void CloseMap()
        {
            if (OriginMap == null)
            {
                EjectAllColonistPawn(WorldComp_DungeonManager.StartingColonyMap, CellFinder.RandomSpawnCellForPawnNear(Map.Center, Map, 10));
            }
            else
            {
                EjectAllColonistPawn(OriginMap, CellFinder.RandomSpawnCellForPawnNear(Map.Center, Map, 10));
            }
        }

        public virtual bool CanMoveToParentMap(Pawn Pawn)
        {
            return true;
        }


        public virtual bool CanEnter(Thing thing, out string denialReason)
        {
            denialReason = String.Empty;
            return true;
        }

        public virtual void MoveToMap(Pawn pawn)
        {
            if (pawn.IsColonist && !CanEnter(pawn, out string denialReason))
            {
                Messages.Message($"Cannot teleport {pawn.LabelShort}: {denialReason}",
                                MessageTypeDefOf.RejectInput);
                return;
            }

            IntVec3 entryPoint = CellFinder.RandomSpawnCellForPawnNear(Map.Center, Map, 10);
            if (entryPoint.IsValid)
            {
                if (pawn.Spawned)
                    pawn.DeSpawn(DestroyMode.Vanish);

                GenSpawn.Spawn(pawn, entryPoint, this.Map);
            }
        }

        public virtual void EjectPawn(Pawn Pawn, Map TargetMap, IntVec3 TargetPosition)
        {
            if (TargetMap != null)
            {
                if (Pawn.Spawned)
                {
                    Pawn.DeSpawn(DestroyMode.Vanish);
                }
                GenSpawn.Spawn(Pawn, TargetPosition, TargetMap);
            }
        }

        public virtual void EjectAllColonistPawn(Map TargetMap, IntVec3 TargetPosition)
        {
            List<Pawn> allColonists = this.Map.mapPawns.AllPawns
                .Where(p => p.Faction == Faction.OfPlayer)
                .ToList();

            if (allColonists.Count > 0)
            {
                Log.Message($"Ejecting {allColonists.Count} colonists from dungeon map {this.Map} to {TargetMap}");

                foreach (var pawn in allColonists)
                {
                    try
                    {
                        EjectPawn(pawn, TargetMap, TargetPosition);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"Failed to eject pawn {pawn}: {ex}");
                    }
                }
            }
            else
            {
                Log.Warning("No colonists found to eject in dungeon map");
            }
        }


        public virtual void EjectAllItems(Map TargetMap, IntVec3 TargetPosition)
        {
            List<Thing> allColonists = this.Map.spawnedThings.Where(x => x.Faction == Faction.OfPlayer && x.def.building == null).ToList();

            if (allColonists.Count > 0)
            {
                foreach (var item in allColonists)
                {
                    EjectThing(item, TargetMap, TargetPosition);
                }

            }
        }

        public virtual void EjectThing(Thing thing, Map TargetMap, IntVec3 TargetPosition)
        {
            if (TargetMap != null)
            {
                if (thing.Spawned)
                {
                    thing.DeSpawn(DestroyMode.Vanish);
                }
                GenSpawn.Spawn(thing, TargetPosition, TargetMap);
            }
        }
        public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions()
        {
            yield break;
        }
    }
}
