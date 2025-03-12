using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class SummonCreatureManager : GameComponent
    {
        private List<SummonCreatureData> summonCreatureDatas = new List<SummonCreatureData>();

        public SummonCreatureManager(Game game)
        {
           
        }

        public void StoreSummon(Pawn master, Pawn summon)
        {
            if (master == null || summon == null)
            {
                Log.Error("Attempted to store a null master or summon in SummonCreatureManager");
                return;
            }

            if (!HasDataForMaster(master))
            {
                SummonCreatureData newData = new SummonCreatureData
                {
                    Master = master,
                    storedPawns = new List<Pawn> { summon }
                };
                summonCreatureDatas.Add(newData);
                Log.Message($"Created new summon data for {master.Label} with {summon.Label}");
            }
            else
            {
                SummonCreatureData summonData = GetDataForMaster(master);
                if (summonData != null)
                {
                    if (!summonData.storedPawns.Contains(summon))
                    {
                        summonData.storedPawns.Add(summon);
                        Log.Message($"Added {summon.Label} to {master.Label}'s summon list");
                    }
                }
            }
        }

        public List<Pawn> RetrieveSummonsFor(Pawn master)
        {
            if (master == null)
            {
                Log.Error("Attempted to retrieve summons for null master in SummonCreatureManager");
                return new List<Pawn>();
            }

            if (HasDataForMaster(master))
            {

                SummonCreatureData creatureData = GetDataForMaster(master);

                if (creatureData == null)
                {
                    Log.Error("creatureData null");
                }
                if (creatureData.storedPawns == null)
                {
                    Log.Error("creatureData storedpawns null");
                }
                return new List<Pawn>(GetDataForMaster(master).storedPawns);
            }
            return new List<Pawn>();
        }

        public void RemoveSummon(Pawn master, Pawn summon)
        {
            if (master == null || summon == null)
            {
                Log.Error("Attempted to remove a null master or summon in SummonCreatureManager");
                return;
            }

            if (HasDataForMaster(master))
            {
                SummonCreatureData data = GetDataForMaster(master);
                if (data != null && data.storedPawns != null && data.storedPawns.Contains(summon))
                {
                    data.storedPawns.Remove(summon);
                    Log.Message($"Removed {summon.Label} from {master.Label}'s summon list");
                }
            }
        }

        public SummonCreatureData GetDataForMaster(Pawn master)
        {
            if (summonCreatureDatas == null || master == null)
                return null;

            // Try direct reference check first (faster and more reliable when it works)
            SummonCreatureData data = summonCreatureDatas.FirstOrDefault(x => x.Master == master);

            if (data != null)
                return data;

            // Fallback to ThingID check
            return summonCreatureDatas.FirstOrDefault(x =>
                x.Master != null && master != null && x.Master.ThingID == master.ThingID);
        }

        public bool HasDataForMaster(Pawn master)
        {
            return GetDataForMaster(master) != null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref summonCreatureDatas, "summonCreatureDatas", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Initialize if null
                if (summonCreatureDatas == null)
                    summonCreatureDatas = new List<SummonCreatureData>();

                // Clean up the collection
                summonCreatureDatas.RemoveAll(data => data == null || data.Master == null);

                // THIS is likely the key fix - when loading in-game, the Master references might
                // point to different instances than the ones being used by the game
                // We need to sync them with the actual in-game instances
                for (int i = 0; i < summonCreatureDatas.Count; i++)
                {
                    var data = summonCreatureDatas[i];

                    // Find the actual in-game pawn instance that corresponds to this Master
                    // This corrects the reference to point to the actual in-game object
                    Pawn actualMaster = FindActualPawnInstance(data.Master);
                    if (actualMaster != null && actualMaster != data.Master)
                    {
                        // Replace with the correct in-game reference
                        data.Master = actualMaster;
                    }
                }
            }
        }

        // Helper method to find the actual in-game pawn instance
        private Pawn FindActualPawnInstance(Pawn originalRef)
        {
            if (originalRef == null) return null;

            // Look for pawns with the same ThingID in all maps and in world pawns
            foreach (Map map in Find.Maps)
            {
                foreach (Pawn p in map.mapPawns.AllPawnsSpawned)
                {
                    if (p.ThingID == originalRef.ThingID)
                        return p;
                }
            }

            // Check world pawns too
            foreach (Pawn p in Find.WorldPawns.AllPawnsAliveOrDead)
            {
                if (p.ThingID == originalRef.ThingID)
                    return p;
            }

            return originalRef; // Fall back to original if not found
        }
    }


    public class SummonCreatureData : IExposable
    {
        public Pawn Master;
        public List<Pawn> storedPawns = new List<Pawn>();

        public SummonCreatureData()
        {
           
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Master, "Master");
            Scribe_Collections.Look(ref storedPawns, "storedPawns", LookMode.Deep);
        }
    }
}
