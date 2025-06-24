using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class PlantExtended : Plant
    {
        public override bool FireBulwark => base.FireBulwark;
        public override void CropBlighted()
        {
            base.CropBlighted();
        }

        public override bool CanYieldNow()
        {
            return base.CanYieldNow();
        }

        public override float GetBeauty(bool outside)
        {
            return base.GetBeauty(outside);
        }

        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            base.PlantCollected(by, plantDestructionMode);
        }

        public override int YieldNow()
        {
            return 0;
        }

        public override void TickLong()
        {
            if (this.LifeStage == PlantLifeStage.Mature)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel), this.Position, this.Map);

                if (!this.Destroyed)
                {
                    this.Destroy();
                }

            
                return;
            }

            base.TickLong();
        }
    }

}