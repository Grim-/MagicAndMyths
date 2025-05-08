using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{

    public class TotemDef : ThingDef
    {
        public int lifetimeTicks = -1;
        public float effectRadius = 3f;

        public EffecterDef sustainedEffecter;

        public TotemDef()
        {
            thingClass = typeof(Building_Totem);
        }
    }


    public class Building_Totem : Building
    {
        public Pawn owner;
        private int lifeTimeTicks = 0;
        private TotemDef Def => (TotemDef)def;
        protected Comp_TotemManager TotemManager => owner.TryGetComp<Comp_TotemManager>();

        protected Effecter sustainedEffecter = null;


        private float overriddenRadius = -1;
        public float EffectRadius => overriddenRadius > 0 ? overriddenRadius : Def.effectRadius;


        public virtual void InitTotem(Pawn totemOwner)
        {
            owner = totemOwner;
            EventManager.Instance.OnThingKilled += Instance_OnThingKilled;
        }

        public void SetOverrideRadius(float radius)
        {
            overriddenRadius = radius;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            EventManager.Instance.OnThingKilled -= Instance_OnThingKilled;
            if (TotemManager != null)
            {
                TotemManager.RemoveTotem(this);
            }

            if (sustainedEffecter != null)
            {
                sustainedEffecter.Cleanup();
                sustainedEffecter = null;
            }
            base.Destroy(mode);
        }

        private void Instance_OnThingKilled(Pawn arg1, DamageInfo arg2, Hediff arg3)
        {
            if (owner != null && arg1 == owner)
            {
                this.Destroy();
            }
        }


        public override void Tick()
        {
            base.Tick();


            if (this.Spawned)
            {
                if (Def.sustainedEffecter != null)
                {
                    if (sustainedEffecter == null)
                    {
                        sustainedEffecter = Def.sustainedEffecter.SpawnMaintained(this.Position, this.Map, 1);
                    }
                    sustainedEffecter.EffectTick(new TargetInfo(this.Position, this.Map), new TargetInfo(this.Position, this.Map));
                }
            }

            if (Def.lifetimeTicks > 0)
            {
                lifeTimeTicks++;
                if (lifeTimeTicks >= Def.lifetimeTicks)
                {
                    Destroy();
                    return;
                }
            }
        }



        public override string GetInspectString()
        {
            string baseString = base.GetInspectString();

            if (Def.lifetimeTicks > 0)
            {
                baseString += $"Duration remaining {(Def.lifetimeTicks - lifeTimeTicks).ToStringSecondsFromTicks()}";
            }


            return baseString;
        }


        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (Find.Selector.IsSelected(this))
            {
                GenDraw.DrawRadiusRing(Position, Def.effectRadius);
            }
        }


        protected IEnumerable<Pawn> PawnsInRadius()
        {
            return GenRadial.RadialDistinctThingsAround(Position, Map, Def.effectRadius, true)
                .OfType<Pawn>()
                .Where(p => p.Spawned && !p.Dead);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref owner, "owner");
            Scribe_Values.Look(ref lifeTimeTicks, "lifeTimeTicks", -1);
            Scribe_Values.Look(ref overriddenRadius, "overriddenRadius", -1);
        }
    }
}
