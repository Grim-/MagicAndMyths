using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class MaskNodeProperties : PawnRenderNodeProperties
    {
        public Color maskColor = Color.white;
        public float alpha = 1f;
        public Vector3 offset = Vector3.zero;
        public float layerOffset = 0.3f;
        public float northFacingLayerOffset = -70f;
        public Vector3 eastOffset = new Vector3(-0.05f, 0f, -0.05f);
        public Vector3 southOffset = new Vector3(0f, 0f, -0.1f);
        public Vector3 southRotation = new Vector3(0f, 0f, 0f);
        public GraphicData graphicData;

        public MaskNodeProperties()
        {
            this.nodeClass = typeof(MaskRenderNode);
            this.workerClass = typeof(MaskNodeWorker);
            this.drawSize = Vector2.one;
            this.pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly;
        }
    }

    public class MaskRenderNode : PawnRenderNode
    {
        public new MaskNodeProperties Props;

        public MaskRenderNode(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
            Props = (MaskNodeProperties)props;
        }

        public override Color ColorFor(Pawn pawn)
        {
            return Props.maskColor;
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            return GraphicDatabase.Get<Graphic_Multi>(
                Props.texPath,
                ShaderDatabase.CutoutComplex,
                Props.drawSize,
                Props.maskColor
            );
        }
    }

    public class MaskNodeWorker : PawnRenderNodeWorker
    {
        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            MaskRenderNode maskNode = node as MaskRenderNode;
            if (maskNode == null || maskNode.Graphic == null) return;

            Mesh mesh = node.GetMesh(parms);
            if (mesh == null) return;

            Material material = maskNode.GraphicFor(parms.pawn).MatAt(parms.facing);
            if (material == null) return;

            Vector3 drawLoc;
            Vector3 pivot;
            Quaternion quat;
            Vector3 scale;

            Vector3 offset = this.OffsetFor(node, parms, out pivot);
            node.GetTransform(parms, out drawLoc, out _, out quat, out scale);
            drawLoc += offset;

            PawnGraphicDrawRequest request = new PawnGraphicDrawRequest(node, mesh, material);
            request.preDrawnComputedMatrix = Matrix4x4.TRS(drawLoc, quat, scale);
            requests.Add(request);
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 baseOffset = base.OffsetFor(node, parms, out pivot);

            if (node is MaskRenderNode maskNode)
            {
                if (parms.facing == Rot4.East)
                {
                    return baseOffset + maskNode.Props.offset + maskNode.Props.eastOffset;
                }
                else if (parms.facing == Rot4.West)
                {
                    return baseOffset + maskNode.Props.offset + new Vector3(-maskNode.Props.eastOffset.x, maskNode.Props.eastOffset.y, maskNode.Props.eastOffset.z);
                }
                else if (parms.facing == Rot4.South)
                {
                    return baseOffset + maskNode.Props.offset + maskNode.Props.southOffset;
                }
                else
                {
                    return baseOffset + maskNode.Props.offset;
                }
            }

            return baseOffset;
        }

        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is MaskRenderNode maskNode)
            {
                float baseLayer = base.LayerFor(node, parms);
                float northFacingAdjustment = (parms.facing == Rot4.North ? maskNode.Props.northFacingLayerOffset : 0);

                return baseLayer + maskNode.Props.layerOffset + northFacingAdjustment;
            }
            return base.LayerFor(node, parms);
        }

        public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is MaskRenderNode maskNode)
            {
                if (parms.facing == Rot4.South)
                {
                    return Quaternion.Euler(maskNode.Props.southRotation) * base.RotationFor(node, parms);
                }
            }

            return base.RotationFor(node, parms);
        }
    }
}
