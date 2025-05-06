using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class Building_Totem : Building
    {
        public Pawn owner;
        public int ticksToDestroy = -1;
        public float effectRadius = 3f;


        protected Comp_TotemManager TotemManager => owner.TryGetComp<Comp_TotemManager>();

        public virtual void InitTotem(Pawn totemOwner, int duration = -1)
        {
            owner = totemOwner;
            ticksToDestroy = duration;
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksToDestroy > 0)
            {
                ticksToDestroy--;
                if (ticksToDestroy <= 0)
                {
                    Destroy();
                    return;
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (TotemManager != null)
            {
                TotemManager.RemoveTotem(this);
            }
            base.DeSpawn(mode);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (Find.Selector.IsSelected(this))
            {
                GenDraw.DrawRadiusRing(Position, effectRadius);
            }
        }


        protected IEnumerable<Pawn> PawnsInRadius()
        {
            return GenRadial.RadialDistinctThingsAround(Position, Map, effectRadius, true)
                .OfType<Pawn>()
                .Where(p => p.Spawned && !p.Dead);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref owner, "owner");
            Scribe_Values.Look(ref ticksToDestroy, "ticksToDestroy", -1);
            Scribe_Values.Look(ref effectRadius, "effectRadius", 3f);
        }
    }
}
