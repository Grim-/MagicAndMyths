using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_OnHitSplashThing : EnchantEffectDef_OnHitBase
    {
        public FloatRange chanceToSplash = new FloatRange(1, 1);
        public int thingSpawnMax = 10;
        public float splashRadius = 2f;
        public ThingDef thing = null;
        public ThingDef thingStuff = null;

        public EnchantEffectDef_OnHitSplashThing()
        {
            workerClass = typeof(EnchantEffect_OnHitSplashThing);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Has a {chanceToSplash.min * 100} - {chanceToSplash.max * 100} % chance to splash {thing.LabelCap} in a {splashRadius} radius around the hit target";
            }
        }


    }

    public class EnchantEffect_OnHitSplashThing : EnchantWorker
    {
        EnchantEffectDef_OnHitSplashThing Def => (EnchantEffectDef_OnHitSplashThing)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, ref DamageWorker.DamageResult damageResult)
        {
            if (Def.hitMode == OnHitMode.Melee && Def.chanceToSplash.RandomInRange >= Rand.Value)
            {
                List<IntVec3> Cells = GenRadial.RadialCellsAround(target.Thing.Position, Def.splashRadius, true).ToList();

                if (Def.thing != null)
                {
                    for (int i = 0; i < Def.thingSpawnMax; i++)
                    {
                        IntVec3 position = Cells.RandomElement();

                        if (position.InBounds(target.Thing.Map))
                        {
                            Thing thing = ThingMaker.MakeThing(Def.thing, Def.thingStuff);
                            GenSpawn.Spawn(thing, position, target.Thing.Map);
                        }
                    }
                }
            }

            return damageResult;
        }
    }
}