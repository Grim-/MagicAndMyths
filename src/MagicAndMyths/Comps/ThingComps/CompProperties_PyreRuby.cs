using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PyreRuby : CompProperties
    {
        public float absorptionRadius = 30f;
        public ThingDef projectileDef;


        public GraphicData overlayGraphic;

        public CompProperties_PyreRuby()
        {
            compClass = typeof(Comp_PyreRuby);
        }
    }

    public class Comp_PyreRuby : ThingComp
    {
        private int absorbedFireCount = 0;
        private bool HasAbsorbed => absorbedFireCount > 0;

        public CompProperties_PyreRuby Props => (CompProperties_PyreRuby)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            EventManager.OnThingDamageTaken += EventManager_OnDamageTaken;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            EventManager.OnThingDamageTaken -= EventManager_OnDamageTaken;
        }

        private void EventManager_OnDamageTaken(Thing arg1, DamageInfo arg2)
        {
            if (arg1 == this.parent)
            {
                if (HasAbsorbed && absorbedFireCount > 0)
                {
                    ReleaseStoredFires();
                }
            }
        }

        public void AbsorbAllFires()
        {
            if (HasAbsorbed || parent.Map == null) 
                return;

            IntVec3 position = parent.Position;
            float radius = Props.absorptionRadius;

            List<Thing> fires = new List<Thing>();
            foreach (Thing thing in parent.Map.listerThings.ThingsOfDef(ThingDefOf.Fire))
            {
                if ((thing.Position - position).LengthHorizontal <= radius)
                {
                    fires.Add(thing);
                }
            }

            absorbedFireCount = fires.Count;
            foreach (Thing fire in fires)
            {
                fire.Destroy();
            }

            FleckMaker.Static(parent.Position, parent.Map, FleckDefOf.PsycastAreaEffect, 10f);
            Find.CameraDriver.shaker.DoShake(4f);
            Messages.Message("Absorbed " + absorbedFireCount + " fires into the pyre ruby.", parent, MessageTypeDefOf.PositiveEvent);
        }

        public void ReleaseStoredFires(bool destroyAlso = true)
        {
            if (parent.Map == null || absorbedFireCount <= 0)
                return;

            IntVec3 position = parent.Position;
            int firesPerCell = Mathf.Max(1, absorbedFireCount / 100);

            for (int i = 0; i < Mathf.Min(absorbedFireCount, 100); i++)
            {
                IntVec3 firePos = position + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(5f))];
                if (firePos.InBounds(parent.Map))
                {
                    Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
                    fire.fireSize = Rand.Range(0.5f, 1.0f) * firesPerCell;
                    GenSpawn.Spawn(fire, firePos, parent.Map);
                }
            }

            GenExplosion.DoExplosion(
                position,
                parent.Map,
                Props.absorptionRadius / 3f,
                DamageDefOf.Flame,
                parent,
                Mathf.Min(absorbedFireCount, 25),
                0f,
                DamageDefOf.Flame.soundExplosion
            );

            if (destroyAlso)
            {
                this.parent.Destroy();
            }
          
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            if (!HasAbsorbed)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Absorb Fire",
                    defaultDesc = "Absorb all fires within a large radius. Can only be used once.",
                    icon = this.parent.def.uiIcon,
                    action = delegate
                    {
                        AbsorbAllFires();
                    }
                };
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!HasAbsorbed)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Absorb Fire",
                    defaultDesc = "Absorb all fires within a large radius. Can only be used once.",
                    icon = this.parent.def.uiIcon,
                    action = delegate
                    {
                        AbsorbAllFires();
                    }
                };
            }
        }


        public override string CompInspectStringExtra()
        {
            if (HasAbsorbed && absorbedFireCount > 0)
            {
                return "Stored fires: " + absorbedFireCount;
            }
            return base.CompInspectStringExtra();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref absorbedFireCount, "absorbedFireCount", 0);
        }
    }

    public class CompProperties_Overlay : CompProperties
    {
        public GraphicData overlayGraphic;
        public bool useMaskTexture = true;
        public bool enablePulse = false;
        public float pulseSpeed = 4f;
        public float pulseAmount = 0.1f;
        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;
        public CompProperties_Overlay()
        {
            compClass = typeof(Comp_Overlay);
        }
    }

    public class Comp_Overlay : ThingComp
    {
        private Texture2D _maskTex;
        private bool showOverlay = true;

        public CompProperties_Overlay Props => (CompProperties_Overlay)props;

        private Texture2D MaskTex
        {
            get
            {
                if (_maskTex == null)
                {
                    _maskTex = ContentFinder<Texture2D>.Get(this.parent.Graphic.data.texPath);
                }
                return _maskTex;
            }
        }
        public void ShowOverlay() => showOverlay = true;
        public void HideOverlay() => showOverlay = false;
        public void ToggleOverlay() => showOverlay = !showOverlay;


        public override void PostDraw()
        {
            base.PostDraw();

            if (Props.overlayGraphic != null && showOverlay)
            {
                Vector3 drawPos = parent.DrawPos;
                drawPos.y = Props.altitudeLayer.AltitudeFor();

                Graphic graphic = Props.overlayGraphic.Graphic;

                if (Props.useMaskTexture)
                {
                    graphic.MatSingle.SetTexture("_MaskTex", MaskTex);
                }

                if (Props.enablePulse)
                {
                    float pulseSize = 1f + (Mathf.Sin(Time.realtimeSinceStartup * Props.pulseSpeed) * Props.pulseAmount);
                    Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(0f, Vector3.up), new Vector3(pulseSize, 1f, pulseSize));
                    Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
                }
                else
                {
                    graphic.Draw(drawPos, parent.Rotation, parent);
                }
            }
        }
    }

}