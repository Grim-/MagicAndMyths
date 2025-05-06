using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TotemManager : CompProperties
    {
        public CompProperties_TotemManager()
        {
            compClass = typeof(Comp_TotemManager);
        }
    }

    public class Comp_TotemManager : ThingComp
    {
        public List<Building_Totem> activeTotems;

        public Building_Totem SpawnTotem(Pawn owner, ThingDef totemDef, IntVec3 position, Map map)
        {
            Building_Totem newTotem = (Building_Totem)ThingMaker.MakeThing(totemDef);
            newTotem.InitTotem(owner);
            activeTotems.Add(newTotem);
            GenSpawn.Spawn(newTotem, position, map);
            return newTotem;
        }

        public void RemoveTotem(Building_Totem totem)
        {
            if (activeTotems.Contains(totem))
            {
                activeTotems.Remove(totem);
            }
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref activeTotems, "activeTotems", LookMode.Reference);
        }
    }


}
