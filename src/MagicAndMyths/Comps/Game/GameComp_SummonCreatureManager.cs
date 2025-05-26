using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{


    public class GameComp_SummonCreatureManager : GameComponent
    {
        private List<SummonCreatureData> summonCreatureDatas = new List<SummonCreatureData>();

        public GameComp_SummonCreatureManager(Game game)
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

            SummonCreatureData data = summonCreatureDatas.FirstOrDefault(x => x.Master == master);

            if (data != null)
                return data;

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
                if (summonCreatureDatas == null)
                    summonCreatureDatas = new List<SummonCreatureData>();

                summonCreatureDatas.RemoveAll(data => data == null || data.Master == null);

                for (int i = 0; i < summonCreatureDatas.Count; i++)
                {
                    var data = summonCreatureDatas[i];
                    Pawn actualMaster = FindActualPawnInstance(data.Master);
                    if (actualMaster != null && actualMaster != data.Master)
                    {
                        data.Master = actualMaster;
                    }
                }
            }
        }
        private Pawn FindActualPawnInstance(Pawn originalRef)
        {
            if (originalRef == null) return null;

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

            return originalRef;
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
