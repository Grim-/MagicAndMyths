using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MagicTattooRecipeDef : RecipeDef
    {
        public TattooDef tattooDef;

        public MagicTattooRecipeDef()
        {
            workerClass = typeof(RecipeWorker_BaseAddMagicTattoo);
        }
    }


    public class RecipeWorker_BaseAddMagicTattoo : Recipe_Surgery
    {
        MagicTattooRecipeDef Def => (MagicTattooRecipeDef)this.recipe;

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);

            TattooDef faceTattoo = Def.tattooDef;
            pawn.style.FaceTattoo = faceTattoo;
        }
    }
    public class UndeadHediffDef : HediffDef
    {
        public int regenTicks = 2400;
        public float baseHealAmount = 2f;


        public UndeadHediffDef()
        {
            hediffClass = typeof(Hediff_Undead);
        }
    }


    public class Hediff_Undead : HediffWithComps
    {
        UndeadHediffDef Def => (UndeadHediffDef)def;

        private Pawn referencedPawn;
        public Pawn Master => referencedPawn;
        public override string Label => base.Label;

        public bool CalledToArms = false;

        public void SetMaster(Pawn pawn)
        {
            referencedPawn = pawn;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            this.pawn.story.skinColorOverride = Color.white;
            pawn.story.HairColor = new Color(0.85f, 0.85f, 0.85f);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.pawn != null && !this.pawn.Dead && !this.pawn.Destroyed && this.Def != null && this.pawn.IsHashIntervalTick(Def.regenTicks))
            {
                this.pawn.QuickHeal(Def.baseHealAmount);
                HandleNeeds();
            }
        }

        private void HandleNeeds()
        {
            foreach (var item in this.pawn.needs.AllNeeds)
            {
                item.CurLevel = item.MaxLevel;
            }

            if (this.pawn.IsSlave)
            {
                Need_Suppression suppression = this.pawn.needs.TryGetNeed<Need_Suppression>();
                if (suppression != null)
                {
                    suppression.CurLevel = suppression.MaxLevel;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
            Scribe_Values.Look(ref CalledToArms, "calledToArms");
            DraftingUtility.MakeDraftable(pawn);
        }
    }


    public class HediffCompProperties_MagicTattooBase : HediffCompProperties
    {
        public TattooDef tattooDef;

        public HediffCompProperties_MagicTattooBase()
        {
            compClass = typeof(HediffComp_MagicTattooBase);
        }
    }

    public abstract class HediffComp_MagicTattooBase : HediffComp
    {
        public HediffCompProperties_MagicTattooBase Props => (HediffCompProperties_MagicTattooBase)props;


        protected TattooDef previousTattoo;
        protected TattooDef currentTattoo;
        protected int appliedTick = -1;
        protected bool hasApplied = false;


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ApplyTattoo(Props.tattooDef);
        }

        public virtual bool ApplyTattoo(TattooDef tattooDef)
        {
            if (tattooDef == null)
                return false;

            if (this.Pawn.style.FaceTattoo != null)
            {
                previousTattoo = this.Pawn.style.FaceTattoo;
            }

            this.Pawn.style.FaceTattoo = tattooDef;
            currentTattoo = tattooDef;
            appliedTick = Find.TickManager.TicksGame;
            hasApplied = true;
            return true;
        }

        public virtual bool RemoveTattoo()
        {
            this.Pawn.style.FaceTattoo = null;
            currentTattoo = null;
            hasApplied = false;
            return true;
        }


        public virtual void OnTattooApplied()
        {

        }

        public virtual void OnTattooRemoved()
        {

        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Defs.Look(ref currentTattoo, "currentTattoo");
            Scribe_Values.Look(ref appliedTick, "appliedTick");
            Scribe_Values.Look(ref hasApplied, "hasApplied");
        }

    }


    public class HediffCompProperties_MagicTattooGrantAbility : HediffCompProperties_MagicTattooBase
    {
        public AbilityDef ability;

        public HediffCompProperties_MagicTattooGrantAbility()
        {
            compClass = typeof(HediffComp_MagicTattooGrantAbility);
        }
    }

    public class HediffComp_MagicTattooGrantAbility : HediffComp_MagicTattooBase
    {
        new public HediffCompProperties_MagicTattooGrantAbility Props => (HediffCompProperties_MagicTattooGrantAbility)props;


        private Ability abilityRef = null;

        public override void OnTattooApplied()
        {
            base.OnTattooApplied();

            if (this.Pawn.abilities != null)
            {
                if (this.Pawn.abilities.GetAbility(Props.ability) == null)
                {
                    this.Pawn.abilities.GainAbility(Props.ability);

                    abilityRef = this.Pawn.abilities.GetAbility(Props.ability);
                }
            }
        }

        public override void OnTattooRemoved()
        {
            base.OnTattooRemoved();

            if (abilityRef != null)
            {
                if (this.Pawn.abilities != null)
                {
                    if (this.Pawn.abilities.GetAbility(abilityRef.def) != null)
                    {
                        this.Pawn.abilities.RemoveAbility(Props.ability);
                        abilityRef = null;
                    }

                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref abilityRef, "abilityGranted");
        }
    }
}
