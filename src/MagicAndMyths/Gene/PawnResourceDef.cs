using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class PawnResourceDef : Def
    {
        public string resourceName = "unnamed resource";
        public StatDef maxStat;
        public StatDef regenTicks;
        public StatDef regenStat;
        public StatDef regenSpeedStat;
        public StatDef costMult;
        public Color barColor = Color.cyan;
    }
}
