using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_DamageReflection : EnchantEffectDef
    {
        public float reflectPercentage = 0.5f;
        public DamageDef damageType = null; 
        public DamageDef reflectedDamageType = null;
        public bool useOriginalDamageType = true;
        public EffecterDef reflectionEffecter;
        public SoundDef reflectionSound;

        public EnchantEffectDef_DamageReflection()
        {
            workerClass = typeof(EnchantEffect_DamageReflection);
        }

        public override string EffectDescription
        {
            get
            {
                string reflectionText = $"Reflects {reflectPercentage * 100}% of damage back to the attacker";

                if (reflectedDamageType != null && !useOriginalDamageType)
                {
                    reflectionText += $" as {reflectedDamageType.LabelCap} damage";
                }

                if (damageType != null)
                {
                    return $"When hit with {damageType.LabelCap} damage, {reflectionText}.";
                }

                return $"When taking damage, {reflectionText}.";
            }
        }
    }

    public class EnchantEffect_DamageReflection : EnchantWorker
    {
        EnchantEffectDef_DamageReflection Def => (EnchantEffectDef_DamageReflection)def;

        public override bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            if (dinfo.Amount <= 0 || dinfo.Instigator == null || EquippingPawn == null)
                return false;

            if (Def.damageType != null && dinfo.Def != Def.damageType)
                return false;

            float damageToReflect = dinfo.Amount * Def.reflectPercentage;

            if (damageToReflect > 0.1f && dinfo.Instigator.Position.DistanceTo(EquippingPawn.Position) < 2)
            {
                DamageDef reflectDamageType = DamageDefOf.Stab;

                if (Def.useOriginalDamageType)
                {
                    reflectDamageType = dinfo.Def;
                }
                else if (Def.reflectedDamageType != null)
                {
                    reflectDamageType = Def.reflectedDamageType;
                }


                DamageInfo reflectDamage = new DamageInfo(
                    reflectDamageType,
                    damageToReflect,
                    armorPenetration: dinfo.ArmorPenetrationInt,
                    angle: dinfo.Angle + 180f,
                    instigator: EquippingPawn,
                    hitPart: null
                );

                if (Def.reflectionEffecter != null && EquippingPawn.Map != null)
                {
                    Effecter effect = Def.reflectionEffecter.Spawn();
                    effect.Trigger(new TargetInfo(EquippingPawn.Position, EquippingPawn.Map),
                                 new TargetInfo(dinfo.Instigator.Position, dinfo.Instigator.Map));
                    effect.Cleanup();
                }


                //if (Def.reflectionSound != null)
                //{
                //    Def.reflectionSound.PlayOneShot(new TargetInfo(EquippingPawn.Position, EquippingPawn.Map));
                //}

                dinfo.Instigator.TakeDamage(reflectDamage);

                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map,
                                  $"{damageToReflect:F1} reflected!", Color.red);

                return false;
            }

            return false;
        }
    }

}