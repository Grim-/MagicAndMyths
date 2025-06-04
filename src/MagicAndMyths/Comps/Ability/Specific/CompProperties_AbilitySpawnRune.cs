using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilitySpawnRune: CompProperties_AbilityEffect
    {
        public ThingDef thingDef;
        public bool selfCast = false;

        public CompProperties_AbilitySpawnRune()
        {
            compClass = typeof(Comp_AbilitySpawnRune);
        }
    }

    public class Comp_AbilitySpawnRune : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnRune Props => (CompProperties_AbilitySpawnRune)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = parent.pawn.Map;
            if (map == null)
                return;

            if (Props.thingDef == null)
                return;

            IntVec3 position = Props.selfCast ? this.parent.pawn.Position.RandomAdjacentCell8Way() : target.Cell;
            GenSpawn.Spawn(Props.thingDef, position, map);
        }
    }
}
