using UnityEngine;
using Verse;
namespace MagicAndMyths
{
    public class ProjectileProxy : Thing
    {
        public Pawn caster;
        public LocalTargetInfo target;
        public ThingDef projectileDef;
        public ThingDef visualOverrideDef = null;
        public float weaponDamageMultiplier = 1f;
        public ProjectileHitFlags hitflags = ProjectileHitFlags.IntendedTarget;
        public int amountPerBurst = 1;
        public int amountOfShots = 1;
        public int ticksToDestroy = -1;
        public int ticksBetweenShots = 10;
        public bool followCaster = true;
        private IntVec3 offset;

        private int shotTimer = 0;
        private int roundsFired = 0;


        public bool HasFiredAllShots => roundsFired >= amountOfShots;

        public void Init(Pawn ProjectileLaunchingPawn, ThingDef ProjectileDef)
        {
            caster = ProjectileLaunchingPawn;
            projectileDef = ProjectileDef;
            offset = this.Position - caster.Position;
        }

        public override void Tick()
        {
            if (!target.IsValid)
            {
                return;
            }


            if (!HasFiredAllShots)
            {
                shotTimer++;

                if (shotTimer >= ticksBetweenShots)
                {
                    FireRound();
                    shotTimer = 0;
                }
            }
            else
            {
                Destroy();
                return;
            }
        }


        protected void FireRound()
        {
            for (int i = 0; i < amountPerBurst; i++)
            {
                LaunchProjectile(target);
            }

            roundsFired++;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 adjustedLoc = drawLoc;

            if (caster != null && caster.Spawned && followCaster)
            {
                adjustedLoc = caster.DrawPos + offset.ToVector3();
            }

            if (visualOverrideDef != null)
            {
                visualOverrideDef.graphic.DrawFromDef(adjustedLoc, caster != null ? caster.Rotation : Rot4.South, visualOverrideDef, 0);
            }
            else
            {
                base.DrawAt(adjustedLoc, flip);
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
                    hitflags);
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
            Scribe_Values.Look(ref shotTimer, "shotTimer", 0);
            Scribe_Values.Look(ref offset, "offset");
            Scribe_Values.Look(ref roundsFired, "shotsFired");
            Scribe_Values.Look(ref amountPerBurst, "amountPerBust");
            Scribe_Values.Look(ref amountOfShots, "amountOfShots");
        }
    }
}