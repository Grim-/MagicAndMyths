using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TriggerEffectTransmuteArea : CompProperties_TriggerBase
    {
        public HediffDef hediff;
        public FloatRange applicationChance = new FloatRange(100, 100);
        public FloatRange severityAmount = new FloatRange(1, 1);
        public FloatRange radius = new FloatRange(4, 4);
        public CompProperties_TriggerEffectTransmuteArea()
        {
            compClass = typeof(CompTrap_TriggerEffectTransmuteArea);
        }
    }


    public class CompTrap_TriggerEffectTransmuteArea : Comp_TriggerBase
    {
        private CompProperties_TriggerEffectTransmuteArea Props => (CompProperties_TriggerEffectTransmuteArea)props;

        public override void Trigger(Pawn pawn)
        {
            base.Trigger(pawn);


            TerrainDef chosen = DefDatabase<TerrainDef>.AllDefs.RandomElement();

            foreach (var item in GenRadial.RadialCellsAround(this.parent.Position, Props.radius.RandomInRange, true))
            {
                this.parent.Map.terrainGrid.SetTerrain(item, chosen);
            }
        }
    }



}