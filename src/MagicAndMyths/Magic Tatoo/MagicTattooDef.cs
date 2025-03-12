using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MagicTattooDef : TattooDef
    {
       // public HediffDef hediff;
		public float tattooScale = 1f;

		public override Graphic GraphicFor(Pawn pawn, Color color)
		{
			if (this.noGraphic)
			{
				return null;
			}
			string maskPath = (this.tattooType == TattooType.Body) ? pawn.story.bodyType.bodyNakedGraphicPath : pawn.story.headType.graphicPath;
			string texPath = this.texPath;
			ShaderTypeDef overrideShaderTypeDef = this.overrideShaderTypeDef;
			return GraphicDatabase.Get<Graphic_Multi>(texPath, ((overrideShaderTypeDef != null) ? overrideShaderTypeDef.Shader : null) ?? ShaderDatabase.CutoutSkinOverlay, new Vector2(tattooScale, tattooScale), color, Color.white, null, maskPath);
		}
	}

	public class Graphic_TattooBase : Graphic_Multi
    {
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
    }
}
