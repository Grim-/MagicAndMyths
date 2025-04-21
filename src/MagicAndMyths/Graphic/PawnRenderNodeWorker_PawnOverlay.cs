using LudeonTK;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class PawnRenderNodeWorker_PawnOverlay : PawnRenderNodeWorker
    {
        [TweakValue("EMO")]
        public static float TWEAK_OFFSET = 2.921543f;

        private static readonly Color BaseRottenColor = new Color(0.29f, 0.25f, 0.22f);
        public static readonly Color DessicatedColorInsect = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Vector3 BaseCarriedOffset = new Vector3(0f, 0f, -0.1f);

        // Dictionary to cache property blocks per overlay
        private static readonly Dictionary<HediffComp_Overlay, MaterialPropertyBlock> propertyBlockCache =
            new Dictionary<HediffComp_Overlay, MaterialPropertyBlock>();

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms) || parms.pawn.Dead || !parms.pawn.Spawned)
                return false;

            Pawn pawn = parms.pawn;
            if (pawn == null)
            {
                return false;
            }

            List<HediffComp_Overlay> overlays = GetValidOverlayHediffs(pawn);
            if (overlays.Count == 0)
            {
                return false;
            }

            return true;
        }


        public static List<HediffComp_Overlay> GetValidOverlayHediffs(Pawn pawn)
        {
            List<HediffComp_Overlay> result = new List<HediffComp_Overlay>();
            List<HediffComp_Overlay> allOverlays = pawn.health.hediffSet.GetHediffComps<HediffComp_Overlay>().ToList();

            if (allOverlays != null && allOverlays.Count > 0)
            {
                foreach (HediffComp_Overlay overlay in allOverlays)
                {
                    if (overlay != null && overlay.Props.overlayGraphic != null && overlay.showOverlay)
                    {
                        result.Add(overlay);
                    }
                }
            }

            return result;
        }

        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            requests.Add(new PawnGraphicDrawRequest(node, null, null));
        }

        public override void PostDraw(PawnRenderNode node, PawnDrawParms parms, Mesh mesh, Matrix4x4 matrix)
        {
            if (parms.pawn == null)
            {
                return;
            }

            Pawn pawn = parms.pawn;
            Vector3 drawPos = parms.matrix.Position();

            List<HediffComp_Overlay> overlays = GetValidOverlayHediffs(pawn);
            if (overlays.Count == 0)
            {
                return;
            }

            Mesh pawnMesh = mesh;
            if (pawnMesh == null)
            {
                return;
            }

            Vector3 baseDrawPos = drawPos;


            foreach (HediffComp_Overlay overlay in overlays)
            {
                if (overlay.Props.overlayGraphic == null || overlay.Props.overlayGraphic.Graphic == null)
                    continue;

                Vector3 currentDrawPos = baseDrawPos;

                currentDrawPos.y = overlay.Props.altitudeLayer.AltitudeFor();

                Graphic graphic = overlay.Props.overlayGraphic.Graphic;

                Material mat = graphic.MatAt(pawn.Rotation);
                Matrix4x4 overlayMatrix = Matrix4x4.TRS(currentDrawPos, Quaternion.identity, Vector3.one);

                if (overlay.MaskTex != null)
                {
                    MaterialPropertyBlock propertyBlock;
                    if (!propertyBlockCache.TryGetValue(overlay, out propertyBlock))
                    {
                        propertyBlock = new MaterialPropertyBlock();
                        propertyBlockCache[overlay] = propertyBlock;
                    }

                    propertyBlock.SetTexture("_MaskTex", overlay.MaskTex);

                    Graphics.DrawMesh(pawnMesh, overlayMatrix, mat, 0, null, 0, propertyBlock);
                }
                else
                {
                    Graphics.DrawMesh(pawnMesh, overlayMatrix, mat, 0);
                }
            }
        }
    }
}