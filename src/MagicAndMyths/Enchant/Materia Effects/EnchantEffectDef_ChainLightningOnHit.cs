using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_ChainLightningOnHit : EnchantEffectDef
    {
        public int maxJumps = 14;
        public float targetRadius = 15f;
        public int damage = 34;
        public DamageDef damageType;

        public int lingerTicks = 50;
        public int ticksBetweenJumps = 25;


        public EnchantEffectDef_ChainLightningOnHit()
        {
            workerClass = typeof(EnchantEffect_ChainLightningOnHit);
        }

        public override string EffectDescription
        {
            get
            {
                string damageTypestring = damageType != null ? damageType.LabelCap : DamageDefOf.ElectricalBurn.LabelCap;
                return $"On Successful melee attack you deal {damage} ({damageTypestring}) damage to the target, which will then attempt to jump to upto {maxJumps} other targets in {targetRadius} radius around the target.";
            }
        }
    }

    public class EnchantEffect_ChainLightningOnHit : EnchantWorker
    {
        EnchantEffectDef_ChainLightningOnHit Def => (EnchantEffectDef_ChainLightningOnHit)def;

        private StaggeredChainLightning chainLightningInstance;

        private int LastTriggerTick = -1;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, ref DamageWorker.DamageResult damageResult)
        {
            if (Attacker != null && target.Pawn != null)
            {
                chainLightningInstance = new StaggeredChainLightning(Attacker.Map, Attacker, 1000, Def.maxJumps, Def.targetRadius, Def.damage, Def.damageType != null ? Def.damageType : DamageDefOf.ElectricalBurn, (Thing) =>
                {
                    return Thing != Attacker && Thing is Pawn targetPawn;
                }, Def.ticksBetweenJumps, Def.lingerTicks);

                chainLightningInstance.StartChain(target.Pawn);

                LastTriggerTick = Current.Game.tickManager.TicksGame;
            }
            return damageResult;
        }



        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);
            if (chainLightningInstance != null)
            {
                chainLightningInstance.Tick();
                if (chainLightningInstance.IsFinished)
                {
                    chainLightningInstance.Stop();
                }
            }
        }
    }
}