using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilitySpawnTotem : CompProperties_AbilityEffect
    {
        public ThingDef totemDef;
        public int duration = -1;
        public float effectRadius = 3f;
        public SoundDef spawnSound;

        public CompProperties_AbilitySpawnTotem()
        {
            compClass = typeof(Comp_AbilitySpawnTotem);
        }
    }

    public class Comp_AbilitySpawnTotem : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnTotem Props => (CompProperties_AbilitySpawnTotem)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Map map = parent.pawn.Map;
            if (map == null) 
                return;

            Comp_TotemManager totemManager = parent.pawn.TryGetComp<Comp_TotemManager>();
            if (totemManager == null)
            {
                return;
            }


            Building_Totem totem =  totemManager.SpawnTotem(this.parent.pawn, Props.totemDef, this.parent.pawn.Position, this.parent.pawn.Map);
            totem.effectRadius = Props.effectRadius;
        }

        //public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        //{
        //    return target.Cell.Standable(parent.pawn.Map) && !target.Cell.Fogged(parent.pawn.Map);
        //}

        //public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        //{
        //    return base.Valid(target, throwMessages) && CanApplyOn(target, LocalTargetInfo.Invalid);
        //}
    }
}
