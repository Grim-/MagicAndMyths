using HarmonyLib;
using System;
using System.Collections.Generic;
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
}
