using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class PawnRenderNodeWorker_GraphicColorable : PawnRenderNodeWorker
    {


        protected bool Applied = false;

        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            base.AppendDrawRequests(node, parms, requests);
        }


        public override MaterialPropertyBlock GetMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms)
        {
            MaterialPropertyBlock matPropBlock = base.GetMaterialPropertyBlock(node, material, parms);

            if (!Applied)
            {
                if (node.Graphics != null)
                {
                    foreach (var item in node.Graphics)
                    {
                        if (item?.data?.shaderParameters != null)
                        {
                            foreach (var p in item.data.shaderParameters)
                            {
                                p.Apply(material);
                            }
                        }
                    }
                }
                Applied = true;
            }

            if (matPropBlock != null && parms.pawn != null && parms.pawn.TryGetComp(out Comp_GraphicColorable comp))
            {
                comp.ModifyPropBlock(ref matPropBlock);
            }

            if (parms.pawn != null)
            {
                int tick = (Find.TickManager.TicksGame - parms.pawn.TickSpawned);
                matPropBlock.SetFloat(ShaderPropertyIDs.AgeSecs, tick);
            }

            return matPropBlock;
        }
    }
}
