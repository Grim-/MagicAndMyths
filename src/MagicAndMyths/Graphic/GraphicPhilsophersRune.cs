using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class GraphicPhilsophersRune : Graphic_WithPropertyBlock
    {
        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            Material mat = base.MatAt(rot, thing);

            if (thing is IGraphicColorLerpable graphicColorLerpable)
            {
                mat.SetColor(ShaderPropertyIDs.Color, Color.Lerp(graphicColorLerpable.ColorOne, graphicColorLerpable.ColorTwo, graphicColorLerpable.ColorLerpT));
            }

            return mat;
        }
    }

    //public class GraphicEquipment : Graphic_WithPropertyBlock
    //{
    //    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    //    {
    //        base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    //    }
    //}

    //public class GraphicDataEquipment : GraphicData
    //{
    //    public Vector3 offset;
    //}
}
