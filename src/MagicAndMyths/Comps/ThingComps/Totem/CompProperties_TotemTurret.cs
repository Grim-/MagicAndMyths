using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TotemTurret : CompProperties_BaseTotem
    {
        public ThingDef projectileDef;
        public bool requiresLineOfSight = true;
        public EffecterDef turretFireEffect = null;

        public CompProperties_TotemTurret()
        {
            compClass = typeof(Comp_TotemTurret);
        }
    }
    public class Comp_TotemTurret : Comp_BaseTotem
    {
        protected CompProperties_TotemTurret Props => (CompProperties_TotemTurret)props;

        public override void OnTotemTick()
        {
            base.OnTotemTick();
            if (Props.projectileDef == null)
            {
                return;
            }
            List<Pawn> pawnsInRange = GetPawnsInRange();
            Log.Message($"Targets found {pawnsInRange.Count}");
            if (pawnsInRange.Count == 0)
            {
                return;
            }


            if (Props.requiresLineOfSight)
            {
               pawnsInRange.RemoveWhere(x => !GenSight.LineOfSight(this.parent.Position, x.Position, this.parent.Map));

                if (pawnsInRange.Count == 0)
                {
                    return;
                }
            }

            bool anyAttackingOwner = pawnsInRange.Any(x => x.mindState.lastAttackedTarget == Parent.owner);
            Pawn chosenTarget = null;
            if (anyAttackingOwner)
            {
                chosenTarget = pawnsInRange.Where(x => x.mindState.lastAttackedTarget == Parent.owner).FirstOrDefault();
            }
            else
            {
                chosenTarget = pawnsInRange.OrderBy(x=> x.Position.DistanceTo(Parent.Position)).First();
            }
            if (chosenTarget != null)
            {
                FireProjectile(this.parent.Position, this.parent.Map, chosenTarget, chosenTarget);
            }
        }

        private void FireProjectile(IntVec3 origin, Map map, LocalTargetInfo target, LocalTargetInfo intendedTarget)
        {
            Projectile projectile = (Projectile)ThingMaker.MakeThing(Props.projectileDef);
            GenSpawn.Spawn(projectile, origin, map);

            if (Props.turretFireEffect != null)
            {
                Props.turretFireEffect.Spawn(origin, map);
            }

            projectile.Launch(this.parent, target, intendedTarget, ProjectileHitFlags.IntendedTarget);
        }
    }
}
