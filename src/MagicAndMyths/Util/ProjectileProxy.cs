using UnityEngine;
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
        public bool followCaster = true;
        private IntVec3 offset;

        public void Init(Pawn ProjectileLaunchingPawn, ThingDef ProjectileDef, int ticksToLive = -1)
        {
            caster = ProjectileLaunchingPawn;
            projectileDef = ProjectileDef;
            ticksToDestroy = ticksToLive;
            offset = this.Position - caster.Position;
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (caster != null && caster.Spawned && followCaster)
            {
                Vector3 adjustedLoc = caster.DrawPos + offset.ToVector3();
                base.DrawAt(adjustedLoc, flip);
            }
            else
            {
                base.DrawAt(drawLoc, flip);
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
                    this.DrawPos,
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
            Scribe_Values.Look(ref offset, "offset");
        }
    }
}