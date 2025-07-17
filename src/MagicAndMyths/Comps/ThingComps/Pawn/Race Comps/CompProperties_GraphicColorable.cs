using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public interface IColorProvider
    {
        Color ColorOne { get; }
        Color ColorTwo { get; }
    }

    public class CompProperties_GraphicColorable : CompProperties
    {
        public CompProperties_GraphicColorable()
        {
            compClass = typeof(Comp_GraphicColorable);
        }
    }

    public abstract class Comp_GraphicColorable : ThingComp, IColorProvider
    {
        public virtual Color ColorTwo => TargetColorTwo;
        public virtual Color ColorOne => this.parent.def.graphic.Color;


        protected Color TargetColorOne = default(Color);
        protected Color TargetColorTwo = Color.white;

        public virtual void SetNewTargetColorOne(Color newColor)
        {
            TargetColorOne = newColor;
        }

        public virtual void SetNewTargetColorTwo(Color newColor)
        {
            TargetColorTwo = newColor;
        }


        public virtual void ModifyPropBlock(ref MaterialPropertyBlock materialPropertyBlock)
        {

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref TargetColorTwo, "targetColor", Color.white);
        }
    }
}
