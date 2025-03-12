using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class VerbShootMelee : Verb_MeleeAttackDamage
    {
        protected override bool TryCastShot()
        {
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			ThingDef projectile = this.verbProps.defaultProjectile;
			if (projectile == null)
			{
				return false;
			}
			ShootLine shootLine;
			bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine, false);
			if (this.verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (base.EquipmentSource != null)
			{
				CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
				if (comp != null)
				{
					comp.Notify_ProjectileLaunched();
				}
				CompApparelVerbOwner_Charged comp2 = base.EquipmentSource.GetComp<CompApparelVerbOwner_Charged>();
				if (comp2 != null)
				{
					comp2.UsedOnce();
				}
			}
			this.lastShotTick = Find.TickManager.TicksGame;
			Thing thing = this.caster;
			Thing equipment = base.EquipmentSource;
			CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
			if (((compMannable != null) ? compMannable.ManningPawn : null) != null)
			{
				thing = compMannable.ManningPawn;
				equipment = this.caster;
			}
			Vector3 drawPos = this.caster.DrawPos;
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map, WipeMode.Vanish);
			if (this.verbProps.ForcedMissRadius > 0.5f)
			{
				float num = this.verbProps.ForcedMissRadius;
				Pawn caster;
				if ((caster = (thing as Pawn)) != null)
				{
					num *= this.verbProps.GetForceMissFactorFor(equipment, caster);
				}
				float num2 = VerbUtility.CalculateAdjustedForcedMiss(num, this.currentTarget.Cell - this.caster.Position);
				if (num2 > 0.5f)
				{
					IntVec3 forcedMissTarget = this.GetForcedMissTarget(num2);
					if (forcedMissTarget != this.currentTarget.Cell)
					{
						ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
						if (Rand.Chance(0.5f))
						{
							projectileHitFlags = ProjectileHitFlags.All;
						}
						if (!this.canHitNonTargetPawnsNow)
						{
							projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
						}
						projectile2.Launch(thing, drawPos, forcedMissTarget, this.currentTarget, projectileHitFlags, this.preventFriendlyFire, equipment, null);
						return true;
					}
				}
			}
			ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
			Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
			ThingDef targetCoverDef = (randomCoverToMissInto != null) ? randomCoverToMissInto.def : null;
			if (this.verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
			{
				bool flag2;
				if (projectile2 == null)
				{
					flag2 = (null != null);
				}
				else
				{
					ThingDef def = projectile2.def;
					flag2 = (((def != null) ? def.projectile : null) != null);
				}
				bool flyOverhead = flag2 && projectile2.def.projectile.flyOverhead;
				shootLine.ChangeDestToMissWild_NewTemp(shotReport.AimOnTargetChance_StandardTarget, flyOverhead, this.caster.Map);
				ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
				if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
				{
					projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
				}
				projectile2.Launch(thing, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags2, this.preventFriendlyFire, equipment, targetCoverDef);
				return true;
			}
			if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.CanBenefitFromCover && !Rand.Chance(shotReport.PassCoverChance))
			{
				ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
				if (this.canHitNonTargetPawnsNow)
				{
					projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
				}
				projectile2.Launch(thing, drawPos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, this.preventFriendlyFire, equipment, targetCoverDef);
				return true;
			}
			ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
			if (this.canHitNonTargetPawnsNow)
			{
				projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
			}
			if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
			{
				projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
			}
			if (this.currentTarget.Thing != null)
			{
				projectile2.Launch(thing, drawPos, this.currentTarget, this.currentTarget, projectileHitFlags4, this.preventFriendlyFire, equipment, targetCoverDef);
			}
			else
			{
				projectile2.Launch(thing, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags4, this.preventFriendlyFire, equipment, targetCoverDef);
			}
			return true;


		}

		// Token: 0x060033CC RID: 13260 RVA: 0x001440D0 File Offset: 0x001422D0
		protected IntVec3 GetForcedMissTarget(float forcedMissRadius)
		{
			int maxExclusive = GenRadial.NumCellsInRadius(forcedMissRadius);
			int num = Rand.Range(0, maxExclusive);
			return this.currentTarget.Cell + GenRadial.RadialPattern[num];
		}
	}
}
