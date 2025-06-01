using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class BasicResourceGeneDef : GeneDef
    {
        public string resourceName = "unnamed resource";
        public StatDef maxStat;
        public StatDef regenTicks;
        public StatDef regenStat;
        public StatDef regenSpeedStat;
        public StatDef costMult;
        public Color barColor = Color.cyan;

        public BasicResourceGeneDef()
        {
            geneClass = typeof(Gene_BasicResource);
            this.resourceGizmoType = typeof(GeneGizmo_BasicResource);
        }
    }
}
