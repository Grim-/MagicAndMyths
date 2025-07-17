using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_GraphicColorablePulse : CompProperties_GraphicColorable
    {
        public float pulseSpeed = 1.0f;
        public int colorChangeInterval = 4000;

        public CompProperties_GraphicColorablePulse()
        {
            compClass = typeof(Comp_GraphicColorablePulse);
        }
    }

    public class Comp_GraphicColorablePulse : Comp_GraphicColorable
    {
        private Color colorA;
        private Color colorB;
        private CompProperties_GraphicColorablePulse Props => (CompProperties_GraphicColorablePulse)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                colorA = new Color(Rand.Value, Rand.Value, Rand.Value);
                colorB = new Color(Rand.Value, Rand.Value, Rand.Value);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(Props.colorChangeInterval))
            {
                colorA = colorB;
                colorB = new Color(Rand.Value, Rand.Value, Rand.Value);
            }
        }

        public override Color ColorTwo
        {
            get
            {
                float tickTime = (Find.TickManager.TicksGame + parent.thingIDNumber) / 60f;
                float sineWave = (Mathf.Sin(tickTime * Props.pulseSpeed) + 1f) / 2f;
                return Color.Lerp(colorA, colorB, sineWave);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref colorA, "colorA", Color.black);
            Scribe_Values.Look(ref colorB, "colorB", Color.white);
        }
    }
}
