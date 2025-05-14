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


    public class CompProperties_AbilitySpawnTotem : CompProperties_AbilityEffect
    {
        public TotemDef totemDef;
        public int duration = -1;
        public float effectRadius = 3f;
        public SoundDef spawnSound;
        public bool selfCast = false;

        public bool overrideTotemRadius = false;

        public CompProperties_AbilitySpawnTotem()
        {
            compClass = typeof(Comp_AbilitySpawnTotem);
        }
    }

    public class Comp_AbilitySpawnTotem : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnTotem Props => (CompProperties_AbilitySpawnTotem)props;

        protected Building_Totem Totem;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = parent.pawn.Map;
            if (map == null)
                return;

            if (Totem != null)
            {
                Totem.Position = target.Cell;
                EffecterDefOf.ImpactSmallDustCloud.Spawn(target.Cell, parent.pawn.Map);
            }
            else
            {
                Comp_TotemManager totemManager = parent.pawn.TryGetComp<Comp_TotemManager>();
                if (totemManager == null)
                {
                    Log.Error($"Comp_AbilitySpawnTotem No Totem Manager comp found.");
                    return;
                }

                IntVec3 position = Props.selfCast ? this.parent.pawn.Position.RandomAdjacentCell8Way() : target.Cell;
                Totem = totemManager.SpawnTotem(this.parent.pawn, Props.totemDef, position, this.parent.pawn.Map);
                EffecterDefOf.ImpactSmallDustCloud.Spawn(target.Cell, parent.pawn.Map);
                if (Props.overrideTotemRadius && Props.effectRadius > 0)
                {
                    Totem.SetOverrideRadius(Props.effectRadius);
                }
            }
        }
    }
}
