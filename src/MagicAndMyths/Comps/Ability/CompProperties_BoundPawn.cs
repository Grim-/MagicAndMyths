using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class CompProperties_BoundPawn : CompProperties
    {
        public PawnKindDef summonedPawnKind;
        public float maxCharge = 100f;
        public float chargeUsagePerTick = 0.1f;
        public float chargeRegainPerTick = 0.05f;

        public CompProperties_BoundPawn()
        {
            compClass = typeof(Comp_BoundPawn);
        }
    }


    public class Comp_BoundPawn : ThingComp
    {
        private Pawn summonedPawn;
        private float currentCharge = 100f;
        private bool isSummoned;

        public CompProperties_BoundPawn Props => (CompProperties_BoundPawn)props;

        private int cooldownDurationTick = 6000;
        private int cooldownTick = 0;
        private bool isOnCooldown = false;

        private int chargeTick = 0;

        public float CurrentCharge => currentCharge;
        public float MaxCharge => Props.maxCharge;

        private Lord lord = null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentCharge = Props.maxCharge;
                lord = LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_DefendPoint(this.parent.Position, 1, 6, false, false), this.parent.Map);
            }
        }

        public override void CompTick()
        {
            base.CompTick();


            UpdateCooldown();


            if (isSummoned)
            {
                if (summonedPawn != null && !summonedPawn.Dead)
                {
                    UseCharge(Props.chargeUsagePerTick);
                }
            }
            else
            {
                GainCharge(Props.chargeRegainPerTick);

            }
        }

        public void GainCharge(float charge)
        {
            currentCharge += charge;

            if (currentCharge >= MaxCharge)
            {
                currentCharge = MaxCharge;
            }
        }

        public void UseCharge(float charge)
        {
            currentCharge -= charge;

            if (currentCharge <= 0)
            {
                OnChargeDepleted();
                currentCharge = 0;
            }
        }


        private void OnChargeDepleted()
        {
            if (summonedPawn != null && summonedPawn.Spawned)
            {
                UnsummonPawn();
            }
        }

        private void UpdateCooldown()
        {
            if (isOnCooldown)
            {
                cooldownTick--;

                if (cooldownTick <= 0)
                {
                    EndCooldown();
                }
            }
        }
        private void StartCooldown()
        {
            cooldownTick = cooldownDurationTick;
            isOnCooldown = true;
        }

        private void EndCooldown()
        {
            isOnCooldown = false;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (summonedPawn != null && summonedPawn.Spawned)
            {
                summonedPawn.Destroy();
            }
        }

        public override string CompInspectStringExtra()
        {
            string cooldownString = $"Recharging ticks remaining : {(cooldownTick).ToStringTicksToPeriod()}.";
            string chargeString = $"Charge {CurrentCharge} / {MaxCharge}.";

            return base.CompInspectStringExtra() + (isOnCooldown ? cooldownString + "\n\n" + chargeString : chargeString );
        }

        private void SummonPawn()
        {
            if (summonedPawn != null && !summonedPawn.Dead || Props.summonedPawnKind == null || isOnCooldown)
                return;

            StartCooldown();

            PawnGenerationRequest request = new PawnGenerationRequest(
                Props.summonedPawnKind,
                Faction.OfPlayer,
                PawnGenerationContext.NonPlayer,
                -1,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: true,
                colonistRelationChanceFactor: 0f,
                forceAddFreeWarmLayerIfNeeded: false);

            summonedPawn = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(summonedPawn, parent.Position, parent.Map);
            isSummoned = true;
            summonedPawn.guest.SetNoInteraction();
            lord.AddPawn(summonedPawn);
        }



        private void UnsummonPawn()
        {
            if (summonedPawn != null && !summonedPawn.Destroyed)
            {
                summonedPawn.Destroy();
                summonedPawn = null;
            }
            isSummoned = false;
        }

        private void ToggleSummon()
        {
            if (!isSummoned)
            {
                if (currentCharge > 0f && !isOnCooldown)
                {
                    SummonPawn();
                }               
            }
            else if (isSummoned)
            {
                UnsummonPawn();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Summon/Unsummon toggle button
            Command_Action summonGizmo = new Command_Action
            {
                defaultLabel = isSummoned ? "Unsummon" : "Summon",
                defaultDesc = isSummoned ? "Dismiss the summoned pawn." : "Summon a pawn using charge energy.",
                icon = TexButton.Banish,
                action = ToggleSummon
            };

            yield return summonGizmo;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentCharge, "currentCharge", 0f);
            Scribe_Values.Look(ref isSummoned, "isSummoned", false);
            Scribe_References.Look(ref summonedPawn, "summonedPawn");
            Scribe_References.Look(ref lord, "lord");
        }

    }
}
