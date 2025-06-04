using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_LaunchProjectileExtended : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;

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


                Projectile projectile = (Projectile)GenSpawn.Spawn(this.Props.projectileDef, pawn.Position, pawn.Map, WipeMode.Vanish);

                if (projectile is Projectile_Extended projectile_Extended && parent is ResourceAbility resourceAbility)
                {
                    projectile_Extended.OverrideDamageAmount = (int)(projectile.DamageAmount * resourceAbility.GetDamageScalingMultiplier());
                }

                projectile.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget, false, null, null);

            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }
    }
}
