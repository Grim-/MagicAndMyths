using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_ShowRadius : HediffCompProperties
    {
        public float radius = 5;
        public Color color = Color.yellow;
        public HediffCompProperties_ShowRadius()
        {
            compClass = typeof(HediffComp_ShowRadius);
        }
    }

    public class HediffComp_ShowRadius : HediffComp
    {
        new public HediffCompProperties_ShowRadius Props => (HediffCompProperties_ShowRadius)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.radius, true).ToList(), Props.color);
        }
    }
}