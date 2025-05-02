using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Building_PhilosophersCircle : Building, IGraphicColorLerpable
    {
        private float storedEnergy = 0;
        private float maxEnergy = 200f;
        private float energyPerKill = 10f;

        public float EnergyAsPercent
        {
            get
            {
                return Mathf.Clamp01(storedEnergy / maxEnergy);
            }
        }

        public Color ColorOne => Color.white;

        public Color ColorTwo => Color.red;

        public float ColorLerpT => EnergyAsPercent;

        public virtual ThingDef ThingToGenerate => MagicAndMythDefOf.MagicAndMyths_PhilsophersStone;
        public virtual bool DestroyOnFullyCharged => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                EventManager.Instance.OnThingKilled += EventManager_OnThingKilled;
            }
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);

            EventManager.Instance.OnThingKilled -= EventManager_OnThingKilled;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(this.Position, 5f);
        }

        private void EventManager_OnThingKilled(Pawn arg1, DamageInfo arg2, Hediff arg3)
        {
            if (arg1.Position.InHorDistOf(this.Position, 5))
            {
        
                AddEnergy(energyPerKill);
                if (arg1.Corpse != null)
                {
                    FleckMaker.ThrowMicroSparks(arg1.Corpse.Position.ToVector3(), this.Map);

                    if (arg1.Corpse.Spawned)
                    {
                        arg1.Corpse.Destroy();
                    }
                }
            }
        }


        private void AddEnergy(float amount)
        {
            storedEnergy += amount;
            if (storedEnergy >= maxEnergy)
            {
                storedEnergy = maxEnergy;
                OnEnergyFull();
            }
        }

        private void RemoveEnergy(float amount)
        {
            storedEnergy -= amount;

            if (storedEnergy <= 0)
            {
                storedEnergy = 0;
                OnEnergyEmpty();
            }
        }

        private void OnEnergyEmpty()
        {

        }

        private void OnEnergyFull()
        {
            if (ThingToGenerate != null)
            {
                Thing thing = ThingMaker.MakeThing(ThingToGenerate);
                thing.stackCount = 1;
                GenSpawn.Spawn(thing, this.Position, this.Map);
                FleckMaker.ThrowExplosionCell(this.Position, this.Map, FleckDefOf.FlashHollow, ColorTwo);

                if (DestroyOnFullyCharged)
                {
                    this.Destroy(DestroyMode.Vanish);
                }

            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Add Energy 10",
                    action = () =>
                    {
                        AddEnergy(10);
                    }
                };

                yield return new Command_Action()
                {
                    defaultLabel = "Remove Energy 10",
                    action = () =>
                    {
                        RemoveEnergy(10);
                    }
                };

                yield return new Command_Action()
                {
                    defaultLabel = "Max Energy",
                    action = () =>
                    {
                        AddEnergy(maxEnergy);
                    }
                };

                yield return new Command_Action()
                {
                    defaultLabel = "Energy Empty",
                    action = () =>
                    {
                        RemoveEnergy(maxEnergy);
                    }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref storedEnergy, "storedEnergy");
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + $"Charge {storedEnergy}";
        }
    }
}
