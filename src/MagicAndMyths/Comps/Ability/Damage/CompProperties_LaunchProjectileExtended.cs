using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_LaunchProjectileExtended : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public IntRange launchAmount = new IntRange(1, 1);

        public CompProperties_LaunchProjectileExtended()
        {
            compClass = typeof(CompAbilityEffect_LaunchProjectileExtended);
        }
    }
    public class CompAbilityEffect_LaunchProjectileExtended : CompAbilityEffect
    {
        public new CompProperties_LaunchProjectileExtended Props => (CompProperties_LaunchProjectileExtended)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            this.LaunchProjectile(target);
        }

        private void LaunchProjectile(LocalTargetInfo target)
        {
            if (this.Props.projectileDef != null)
            {
                Pawn pawn = this.parent.pawn;

                for (int i = 0; i < Props.launchAmount.RandomInRange; i++)
                {
                    IntVec3 spawnCell = pawn.Position.RandomAdjacentCell8Way();

                    Projectile projectile = (Projectile)GenSpawn.Spawn(this.Props.projectileDef, spawnCell, pawn.Map, WipeMode.Vanish);

                    if (projectile is Projectile_Extended projectile_Extended && parent is ResourceAbility resourceAbility)
                    {
                        projectile_Extended.OverrideDamageAmount = (int)(projectile.DamageAmount * resourceAbility.GetDamageScalingMultiplier());
                    }

                    projectile.Launch(pawn, spawnCell.ToVector3Shifted(), target, target, ProjectileHitFlags.IntendedTarget, false, null, null);
                }
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }
    }
}
