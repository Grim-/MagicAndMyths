using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MagicAndMyths
{
    public class EnchantWorker : IExposable
    {
        public EnchantEffectDef def;
        public EnchantInstance parentInstance;
        public Pawn EquippingPawn;
        public ThingWithComps ParentEquipment;

        private Comp_EnchantProvider _MateriaComp;
        public Comp_EnchantProvider MateriaComp
        {
            get
            {
                if (_MateriaComp == null)
                {
                    _MateriaComp = ParentEquipment.TryGetComp<Comp_EnchantProvider>();
                }

                return _MateriaComp;
            }
        }

        private int CooldownTicks = 0;
        public int CooldownDurationTicks = 0;

        public EnchantWorker()
        {

        }

        public virtual bool HasCooldown()
        {
            return CooldownDurationTicks > 0;
        }

        public virtual bool IsOnCooldown()
        {
            if (!HasCooldown())
            {
                return false;
            }

            return CooldownTicks > 0;
        }


        public virtual void Notify_MateriaEquipped()
        {

        }
        public virtual void Notify_MateriaUnequipped()
        {

        }

        public virtual void Notify_Equipped(Pawn pawn)
        {
            EquippingPawn = pawn;
        }
        public virtual void Notify_Unequipped(Pawn pawn)
        {
            EquippingPawn = null;
        }
        public virtual void Notify_ParentDestroyed()
        {
 
        }
        public virtual void Notify_ParentDespawned()
        {

        }
        public virtual void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {

        }

        public virtual bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            return false;
        }

        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            return Damage;
        }

        public virtual void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {

        }

        public virtual void Notify_OwnerThoughtLost(Thought Thought)
        {

        }
        public virtual void PostDraw()
        {

        }

        public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            return Enumerable.Empty<Gizmo>();
        }


        public virtual void Notify_KilledPawn(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerKilled()
        {

        }

        public virtual void Notify_OwnerMood(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {

        }
        public virtual void Notify_OwnerHediffRemoved(Hediff Hediff)
        {

        }

        public virtual void OnTick(Pawn pawn)
        {

        }

        public virtual void Notify_MateriaLinked(EnchantDef sourceMateria, EnchantDef linkedMateria)
        {
        
        }
        public virtual void Notify_MateriaUnlinked(EnchantDef sourceMateria, EnchantDef linkedMateria)
        {
        
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            return DamageWorkerResult;
        }
        public virtual float GetStatFactor(StatDef stat)
        {
            return 1f;
        }

        public virtual float GetStatOffset(StatDef stat)
        {
            return 0f;
        }

        public virtual string GetExplanationString()
        {
            return string.Empty;
        }

        public virtual void ExposeData()
        {
           // Scribe_Deep.Look(ref def, "def");
            Scribe_Values.Look(ref CooldownTicks, "cooldownTicks");
            Scribe_References.Look(ref ParentEquipment, "parentEquipment");
            Scribe_References.Look(ref EquippingPawn, "EquippingPawn");
    }


        public virtual void TryRemoveEnchant()
        {
            if (this.MateriaComp != null)
            {
                if (this.EquippingPawn != null && this.EquippingPawn.Map != null)
                {
                    EffecterDefOf.Shield_Break.Spawn(this.EquippingPawn.Position, this.EquippingPawn.Map);
                }

                this.MateriaComp.RemoveEnchant(this.parentInstance, false);
            }
        }

    }


}
