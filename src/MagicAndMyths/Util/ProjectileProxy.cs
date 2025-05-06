using Verse;

namespace MagicAndMyths
{
    public class ProjectileProxy : Thing
    {
        public Pawn caster;
        public LocalTargetInfo target;
        public ThingDef projectileDef;
        public float weaponDamageMultiplier = 1f;
        public int ticksToDestroy = -1;
        public int ticksToFire = -1;
        public void Init(Pawn ProjectileLaunchingPawn, ThingDef ProjectileDef, int ticksToLive = -1)
        {
            caster = ProjectileLaunchingPawn;
            projectileDef = ProjectileDef;
            ticksToDestroy = ticksToLive;
        }
        public override void Tick()
        {
            if (ticksToFire > 0)
            {
                ticksToFire--;
                if (ticksToFire <= 0 && target.IsValid)
                {
                    LaunchProjectile(target);
                }
            }


            if (ticksToDestroy > 0)
            {
                ticksToDestroy--;
                if (ticksToDestroy <= 0)
                {
                    Destroy();
                    return;
                }
            }
        }
        public void LaunchProjectile(LocalTargetInfo target)
        {
            if (projectileDef == null || caster == null)
                return;
            Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, Position, Map);
            if (projectile != null)
            {
                projectile.Launch(
                    caster,
                    Position.ToVector3Shifted(),
                    target,
                    target,
                    ProjectileHitFlags.IntendedTarget);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref caster, "caster");
            Scribe_TargetInfo.Look(ref target, "target");
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_Values.Look(ref weaponDamageMultiplier, "weaponDamageMultiplier", 1f);
            Scribe_Values.Look(ref ticksToDestroy, "ticksToDestroy", -1);
            Scribe_Values.Look(ref ticksToFire, "ticksToFire", -1);
        }
    }
}
