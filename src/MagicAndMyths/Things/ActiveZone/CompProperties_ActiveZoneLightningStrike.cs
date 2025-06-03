using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ActiveZoneLightningStrike : CompProperties
    {
        public int ticksBetweenStrikes = 120;
        public int maxTargets = -1;

        public bool edgesOnly = true;

        public FloatRange damage = new FloatRange(1, 1);
        public DamageDef damageDef;

        public EffecterDef targetDamageEffecterDef = null;

        public CompProperties_ActiveZoneLightningStrike()
        {
            compClass = typeof(ActiveZoneComp_LightningStrike);
        }
    }

    public class ActiveZoneComp_LightningStrike : ActiveZoneComp
    {
        CompProperties_ActiveZoneLightningStrike Props => (CompProperties_ActiveZoneLightningStrike)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);
            if (ParentZone.IsHashIntervalTick(Props.ticksBetweenStrikes))
            {
                if (Props.edgesOnly)
                {
                    List<IntVec3> edgeCells = GetEdgeCells(cells);
                    if (edgeCells.Any())
                    {

                        foreach (var item in edgeCells)
                        {
                            if (Rand.Value > 0.6f)
                            {
                                LightningStrike.GenerateLightningStrike(ParentZone.Map, item, 3f, out IEnumerable<IntVec3> Cells, 3, 1);
                            }              
                        }
                       
                    }
                }
                else
                {
                    LightningStrike.GenerateLightningStrike(ParentZone.Map, cells.RandomElement(), 3f, out IEnumerable<IntVec3> Cells, 3, 1);
                }
            }
        }

        private List<IntVec3> GetEdgeCells(List<IntVec3> zoneCells)
        {
            List<IntVec3> edgeCells = new List<IntVec3>();
            HashSet<IntVec3> zoneCellsSet = new HashSet<IntVec3>(zoneCells);

            foreach (IntVec3 cell in zoneCells)
            {
                bool isEdge = false;
                foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell))
                {
                    if (!zoneCellsSet.Contains(neighbor))
                    {
                        isEdge = true;
                        break;
                    }
                }

                if (isEdge)
                {
                    edgeCells.Add(cell);
                }
            }

            return edgeCells;
        }
    }
}
