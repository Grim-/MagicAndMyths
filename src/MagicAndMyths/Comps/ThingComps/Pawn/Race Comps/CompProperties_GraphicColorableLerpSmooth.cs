using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_GraphicColorableLerpSmooth : CompProperties_GraphicColorable
    {
        public int lerpDurationTicks = 120;
        public int colorChangeInterval = 4000;

        public CompProperties_GraphicColorableLerpSmooth()
        {
            compClass = typeof(Comp_GraphicColorableLerpSmooth);
        }
    }
    public class Comp_GraphicColorableLerpSmooth : Comp_GraphicColorable
    {
        private Color startColor;
        private Color currentColor;
        private int lerpStartTick = -1;

        public override Color ColorTwo => currentColor;

        private CompProperties_GraphicColorableLerpSmooth Props => (CompProperties_GraphicColorableLerpSmooth)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentColor = new Color(Rand.Value, Rand.Value, Rand.Value);
                SetNewTargetColorTwo(currentColor);
            }
        }


        public override void ModifyPropBlock(ref MaterialPropertyBlock materialPropertyBlock)
        {
            base.ModifyPropBlock(ref materialPropertyBlock);
            if (materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor(ShaderPropertyIDs.Color, ColorTwo);
            }
        }

        public override void SetNewTargetColorTwo(Color newColor)
        {
            base.SetNewTargetColorTwo(newColor);
            startColor = currentColor;
            lerpStartTick = Find.TickManager.TicksGame;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (lerpStartTick >= 0)
            {
                int elapsedTicks = Find.TickManager.TicksGame - lerpStartTick;
                float t = (float)elapsedTicks / Props.lerpDurationTicks;
                currentColor = Color.Lerp(startColor, TargetColorTwo, Mathf.Clamp01(t));

                if (t >= 1.0f)
                {
                    lerpStartTick = -1;
                }
            }

            if (parent.IsHashIntervalTick(Props.colorChangeInterval))
            {
                SetNewTargetColorTwo(new Color(Rand.Value, Rand.Value, Rand.Value));
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentColor, "currentColor", Color.white);
            Scribe_Values.Look(ref startColor, "startColor", Color.white);
            Scribe_Values.Look(ref lerpStartTick, "lerpStartTick", -1);
        }
    }


}
