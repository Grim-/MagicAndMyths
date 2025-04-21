using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class Building_PortalGate : Building, IPortalProvider
    {
        public bool IsPortalOpen = false;
        public GateAddress OpenAddress = null;
        public Map OpenAddressMap = null;
        public List<GateAddress> savedAddresses = new List<GateAddress>();

        private int PositionRetries = 10;
        private Effecter portalEffect = null;
        protected Zone_Stockpile linkedZone = null;

        private WorldComp_GateManager _GateManager;
        private WorldComp_GateManager GateManager
        {
            get
            {
                if (_GateManager == null)
                {
                    _GateManager = Find.World.GetComponent<WorldComp_GateManager>();
                }

                return _GateManager;
            }
        }

        private WorldComp_SiteManager _SiteManager;
        private WorldComp_SiteManager SiteManager
        {
            get
            {
                if (_SiteManager == null)
                {
                    _SiteManager = Find.World.GetComponent<WorldComp_SiteManager>();
                }

                return _SiteManager;
            }
        }


        private GateAddress CurrentSiteAddress => GateAddress.CreateAddressForTile(this.Map.Tile);

        public bool IsPortalActive => IsPortalOpen;

        public override string GetInspectString()
        {
            return base.GetInspectString() + CurrentSiteAddress != null ? $"Address {CurrentSiteAddress}" : " ";
        }


        public override void Tick()
        {
            base.Tick();
            if (portalEffect != null && IsPortalOpen)
            {
                portalEffect.ticksLeft = 999;
                portalEffect.EffectTick(this, this);
            }
        }

        #region Zone Transfer
        private void TriggerAreaTransfer()
        {
            if (!IsPortalOpen || OpenAddress == null)
            {
                Log.Warning("Attempted area transfer with closed portal");
                return;
            }

            if (linkedZone == null)
            {
                Messages.Message("No zone linked to portal", MessageTypeDefOf.RejectInput);
                return;
            }

            if (OpenAddressMap == null)
            {
                Messages.Message("Invalid destination map", MessageTypeDefOf.RejectInput);
                return;
            }

            if (!OpenAddressMap.mapPawns.FreeColonistsSpawned.Any())
            {
                Messages.Message("No colonists at destination to receive items", MessageTypeDefOf.RejectInput);
                return;
            }

            var items = linkedZone.AllContainedThings
                .Where(t => !(t is Building) && !(t is Pawn))
                .ToList();

            if (!items.Any())
            {
                Messages.Message("No items to transfer in linked zone", MessageTypeDefOf.RejectInput);
                return;
            }


            List<Building_PortalGate> destinationStockPile = OpenAddressMap.listerBuildings.AllBuildingsColonistOfDef(MagicAndMythDefOf.Portal_GateLarge).Cast<Building_PortalGate>().ToList();
            List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();

            foreach (var item in destinationStockPile)
            {
                if (IsValidTransfer(item, items.Count))
                {
                    floatMenuOptions.Add(new FloatMenuOption($"Try Transfer to {item.Label} linked zone.", () =>
                    {
                        TryTransferItemsToZone(item, OpenAddressMap, items);
                    }));
                }
            }


            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        }

        private bool IsValidTransfer(Building_PortalGate DestinationGate, int itemCount)
        {
            if (DestinationGate == null || DestinationGate == this || DestinationGate.linkedZone == null)
                return false;

            var freeSpaces = DestinationGate.linkedZone.Cells
                .Count(cell => cell.Standable(DestinationGate.Map) &&
                               !cell.Filled(DestinationGate.Map) &&
                               !DestinationGate.Map.thingGrid.ThingsListAt(cell)
                                    .Any(thing => thing.def.category == ThingCategory.Item));

            return freeSpaces >= itemCount;
        }
        private void TryTransferItemsToZone(Building_PortalGate DestinationGate, Map destinationMap, List<Thing> TransferItems)
        {
            int successfulTransfers = 0;

            foreach (Thing item in TransferItems)
            {
                IntVec3 targetPos = FindItemDropLocation(destinationMap, DestinationGate.linkedZone);
                if (targetPos.IsValid)
                {
                    item.DeSpawn();
                    item.Position = targetPos;
                    GenSpawn.Spawn(item, targetPos, destinationMap);
                    successfulTransfers++;
                }
                else
                {
                    Messages.Message("Cannot find valid position for all items. Transfer stopped.", MessageTypeDefOf.RejectInput);
                    break;
                }
            }

            if (successfulTransfers > 0)
            {
                Messages.Message($"Transferred {successfulTransfers} items through portal", MessageTypeDefOf.PositiveEvent);
            }
        }
        private IntVec3 FindItemDropLocation(Map map, Zone_Stockpile zone)
        {
            var zoneCells = zone.Cells.ToList();

            zoneCells = zoneCells.InRandomOrder().ToList();

            foreach (IntVec3 cell in zoneCells)
            {
                if (cell.Standable(map) && !cell.Filled(map))
                {
                    bool cellOccupied = map.thingGrid.ThingsListAt(cell)
                        .Any(thing => thing.def.category == ThingCategory.Item);

                    if (!cellOccupied)
                    {
                        return cell;
                    }
                }
            }

            return IntVec3.Invalid;
        }
        public void LinkZone(Zone_Stockpile zone)
        {
            linkedZone = zone;
            Messages.Message($"Linked portal to zone: {zone.label}", MessageTypeDefOf.PositiveEvent);
        }
        #endregion


        public void OpenPortal(GateAddress Address)
        {
            if (IsPortalOpen || Address == null)
            {
                return;
            }

            if (portalEffect == null)
            {
                // portalEffect = PortalDefOf.Portal_Effect.SpawnMaintained(this, this.Map, 4f);
            }


            OpenAddress = Address;
            OpenAddressMap = SiteManager.GetOrCreateMapForTile(GateManager.ResolveTileFromAddress(OpenAddress), MagicAndMythDefOf.MapGenEmpty);


            if (OpenAddressMap == null)
            {

                return;
            }


            IsPortalOpen = true;

            if (!HasSavedAddress(Address))
            {
                SaveAddress(Address);
            }


            Messages.Message("Portal address accepted.", MessageTypeDefOf.PositiveEvent);
            OnPortalOpened();
        }

        public void ClosePortal()
        {
            if (!IsPortalOpen)
            {
                return;
            }

            if (portalEffect != null)
            {
                portalEffect.Cleanup();
                portalEffect = null;
            }


            SiteManager.RemoveStoredDataForTileID(GateManager.ResolveTileFromAddress(OpenAddress));
            //SiteManager.DestroyCustomDimension(OpenAddress);
            IsPortalOpen = false;
            OpenAddress = null;


            Messages.Message("Portal connection closed.", MessageTypeDefOf.NeutralEvent);
            OnPortalClosed();
        }

        protected virtual void OnPortalOpened()
        {

        }

        protected virtual void OnPortalClosed()
        {

        }

        private void SaveAddress(GateAddress Address)
        {
            savedAddresses.Add(Address);
        }

        private bool HasSavedAddress(GateAddress Address)
        {
            return savedAddresses.Any(a => a.ToString() == Address.ToString());
        }

        public bool TeleportPawn(Pawn pawn)
        {
            if (!IsPortalOpen || OpenAddress == null)
                return false;

            if (OpenAddressMap == null)
                return false;

            IntVec3 spawnLoc = FindTeleportLocation(pawn, OpenAddressMap);
            if (!spawnLoc.IsValid)
                return false;

            pawn.TransferToMap(spawnLoc, OpenAddressMap);
            TrySpawnAtLocation(spawnLoc, OpenAddressMap);
            return true;
        }



        private void TrySpawnAtLocation(IntVec3 spawnLoc, Map map)
        {
            //if (map != null)
            //{
            //    Building_OneWayPortal oneWayPortal = (Building_OneWayPortal)ThingMaker.MakeThing(PortalDefOf.Portal_Oneway, null);
            //    if (oneWayPortal != null)
            //    {
            //        oneWayPortal.SetDestination(GateAddress.CreateAddressForTile(this.Map.Tile));
            //    }

            //    GenSpawn.Spawn(oneWayPortal, spawnLoc, map);
            //}
        }

        public void AttemptDialAddress(List<GateSymbolDef> currentAddress)
        {
            if (IsPortalOpen || currentAddress == null || !currentAddress.Any())
            {
                Messages.Message("Cannot dial: Portal is already open or invalid address.", MessageTypeDefOf.RejectInput);
                return;
            }

            var gateAddress = new GateAddress(currentAddress);
            int tileId = GateManager.ResolveTileFromAddress(gateAddress);
            if (tileId != -1)
            {
                OpenPortal(gateAddress);
            }
            else
            {
                Messages.Message("Invalid portal address.", MessageTypeDefOf.RejectInput);
            }
        }
        private bool TeleportLocationValidator(Pawn Pawn, Map Map, IntVec3 Position)
        {
            return Position.Standable(Map) && !Position.Filled(Map) && Position.GetDangerFor(Pawn, Map) != Danger.Deadly;
        }

        private Map GetDestinationMap()
        {
            return OpenAddressMap;
        }

        private IntVec3 FindTeleportLocation(Pawn pawn, Map map)
        {
            for (int i = 0; i < PositionRetries; i++)
            {
                var testLoc = CellFinder.RandomEdgeCell(map);
                if (TeleportLocationValidator(pawn, map, testLoc))
                {
                    return testLoc;
                }
            }

            for (int i = 0; i < PositionRetries; i++)
            {
                var testLoc = CellFinder.StandableCellNear(map.Center, map, 3f);
                if (TeleportLocationValidator(pawn, map, testLoc))
                {
                    return testLoc;
                }
            }


            Log.Warning("Could not find valid spawn location for teleport");
            return IntVec3.Invalid;
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "Open Portal Interface",
                defaultDesc = "Configure and activate the portal gate.",
                icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/CaravanHitchingSpot", true),
                action = () =>
                {
                    Find.WindowStack.Add(new Window_PortalUI(this));
                },
                disabledReason = null
            };


            yield return new Command_Action
            {
                defaultLabel = "Link Zone",
                defaultDesc = "Link this portal to a stockpile zone for item transfer.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true),
                action = () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    // Get all stockpile zones on the map
                    foreach (Zone zone in this.Map.zoneManager.AllZones)
                    {
                        if (zone is Zone_Stockpile stockpile)
                        {
                            options.Add(new FloatMenuOption(
                                $"Link to {stockpile.label}",
                                () => LinkZone(stockpile)
                            ));
                        }
                    }

                    if (!options.Any())
                    {
                        Messages.Message("No stockpile zones available", MessageTypeDefOf.RejectInput);
                        return;
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            // Add transfer items command if zone is linked
            if (linkedZone != null && IsPortalOpen)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Transfer Zone Items",
                    defaultDesc = $"Transfer all items from {linkedZone.label} through the portal.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true),
                    action = () => TriggerAreaTransfer()
                };
            }


            if (IsPortalOpen)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Close Portal",
                    defaultDesc = "Deactivate the active portal connection.",
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/CaravanHitchingSpot", true),
                    action = ClosePortal
                };
            }
        }
        public bool TeleportToFactionBase(Settlement settlement)
        {
            if (settlement == null)
            {
                Messages.Message("Cannot teleport: Invalid settlement", MessageTypeDefOf.RejectInput);
                return false;
            }


            GateAddress address = settlement.GetGateAddress();
            if (address == null)
            {
                Messages.Message("Cannot create gate address for settlement", MessageTypeDefOf.RejectInput);
                return false;
            }


            OpenPortal(address);


            if (IsPortalOpen && !HasSavedAddress(address))
            {
                SaveAddress(address);
                Messages.Message($"Added {settlement.Label} to known addresses", MessageTypeDefOf.PositiveEvent);
            }

            return IsPortalOpen;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var g in base.GetFloatMenuOptions(selPawn))
            {
                yield return g;
            }

            if (IsPortalOpen)
            {
                if (selPawn.Faction == this.Faction && !selPawn.Downed && !selPawn.Dead)
                {
                    yield return new FloatMenuOption("Enter Portal", () =>
                    {
                        Job job = JobMaker.MakeJob(MagicAndMythDefOf.Portals_UsePortalJob, this);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                    });
                }

                if (selPawn.Faction == this.Faction)
                {
                    yield return new FloatMenuOption("Start Transport Transfer", () =>
                    {
                        TriggerAreaTransfer();
                    });
                }
            }
        }


        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (IsPortalOpen)
            {
                ClosePortal();
            }
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedZone, "linkedZone");
            Scribe_Deep.Look(ref OpenAddress, "openAddress");
            Scribe_References.Look(ref OpenAddressMap, "openAddressMap");
            Scribe_Values.Look(ref IsPortalOpen, "isPortalOpen");
            Scribe_Collections.Look(ref savedAddresses, "savedAddresses", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (OpenAddress != null)
                {
                    OpenPortal(OpenAddress);
                }
            }
        }
    }
}
