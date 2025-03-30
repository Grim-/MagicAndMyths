using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class Tree_Weirwood : Plant
    {
        private Queue<Pawn> pawnsStored = new Queue<Pawn>();
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            EventManager.OnThingKilled += EventManager_OnThingKilled;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            EventManager.OnThingKilled -= EventManager_OnThingKilled;
            base.DeSpawn(mode);
        }

        private void EventManager_OnThingKilled(Pawn arg1, DamageInfo arg2, Hediff arg3)
        {
            if (arg1.Position.InHorDistOf(this.Position, 5f))
            {
                Messages.Message("Pawn died", MessageTypeDefOf.PositiveEvent);
                pawnsStored.Enqueue(arg1);
            }
        }
    }
}
