using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class CompProperties_GraphicUniqueColor : CompProperties_GraphicColorable
    {

        public CompProperties_GraphicUniqueColor()
        {
            compClass = typeof(Comp_GraphicUniqueColor);
        }
    }


    public class Comp_GraphicUniqueColor : Comp_GraphicColorable
    {
        private Color startColor;
        private Color currentColor;
        private int lerpStartTick = -1;

        public override Color ColorTwo => currentColor;

        private CompProperties_GraphicUniqueColor Props => (CompProperties_GraphicUniqueColor)props;

        protected Color UniqueColorOne = default(Color);
        protected Color UniqueColorTwo = default(Color);


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UniqueColorOne = new Color(Rand.Value, Rand.Value, Rand.Value, 1);
            UniqueColorTwo = new Color(Rand.Value, Rand.Value, Rand.Value, 1);
            if (!respawningAfterLoad)
            {

            }
        }

        public override void ModifyPropBlock(ref MaterialPropertyBlock materialPropertyBlock)
        {
            base.ModifyPropBlock(ref materialPropertyBlock);
            if (materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor("_AfterColor1", UniqueColorOne);
                materialPropertyBlock.SetColor("_AfterColor2", UniqueColorTwo);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref UniqueColorOne, "UniqueColorOne", Color.white);
            Scribe_Values.Look(ref UniqueColorTwo, "UniqueColorTwo", Color.white);
        }
    }



    public class CompProperties_GraphicColorableSesonal : CompProperties_GraphicColorable
    {
        public int lerpDurationTicks = 120;
        public int colorChangeInterval = 4000;

        public CompProperties_GraphicColorableSesonal()
        {
            compClass = typeof(Comp_GraphicColorableSesonal);
        }
    }


    public class Comp_GraphicColorableSesonal : Comp_GraphicColorable
    {
        private Color startColor;
        private Color currentColor;
        private int lerpStartTick = -1;

        public override Color ColorTwo => currentColor;

        private CompProperties_GraphicColorableSesonal Props => (CompProperties_GraphicColorableSesonal)props;

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


            switch (GenLocalDate.Season(this.parent.Map))
            {
                case Season.Undefined:
                    TargetColorTwo = Color.white;
                    break;
                case Season.Spring:
                    TargetColorTwo = Color.green;
                    break;
                case Season.Summer:
                    TargetColorTwo = Color.yellow;
                    break;
                case Season.Fall:
                    TargetColorTwo = Color.cyan;
                    break;
                case Season.Winter:
                    TargetColorTwo = Color.red;
                    break;
                case Season.PermanentSummer:
                    TargetColorTwo = Color.red;
                    break;
                case Season.PermanentWinter:
                    TargetColorTwo = Color.magenta;
                    break;
            }

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
