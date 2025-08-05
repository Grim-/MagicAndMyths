using EMF;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
   public class LevelAbilityDef
    {
        public int level;
        public AbilityDef abilityDef;
    }
    
    public class CompProperties_MagicalTome : CompProperties_Levelable
    {
        public float xpPerUse = 10f;
        public List<LevelAbilityDef> levelAbilities = new List<LevelAbilityDef>();
        
        public CompProperties_MagicalTome()
        {
            this.compClass = typeof(Comp_MagicalTome);
        }
    }
    
    public class Comp_MagicalTome : CompLevelable
    {
        protected List<Ability> grantedAbilities = new List<Ability>();


        protected Dictionary<AbilityDef, int> cooldownTicks = new Dictionary<AbilityDef, int>();

        protected List<AbilityDef> WorkingKeys = new List<AbilityDef>();
        protected List<int> WorkingValues = new List<int>();

        private CompProperties_MagicalTome Props => (CompProperties_MagicalTome)props;
        public Pawn EquipOwner = null;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            EquipOwner = pawn;
            EventManager.Instance.OnAbilityCompleted += Notify_WearerUsedAbility;

            RefreshGrantedAbilities(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            EventManager.Instance.OnAbilityCompleted -= Notify_WearerUsedAbility;
            RemoveGrantedAbilities(pawn);
            base.Notify_Unequipped(pawn);
            EquipOwner = null;
        }
        
        protected override void OnLevelUp(int oldLevel, int newLevel)
        {
            MoteMaker.ThrowText(parent.DrawPos, parent.Map, "TomeLevelUp".Translate(newLevel), Color.cyan);
            
            if (EquipOwner != null)
            {
                RefreshGrantedAbilities(EquipOwner);
            }
        }
        
        private void RefreshGrantedAbilities(Pawn pawn)
        {
            GrantAvailableAbilities(pawn);
        }

        private void GrantAvailableAbilities(Pawn pawn)
        {
            foreach (var levelAbility in Props.levelAbilities)
            {
                if (levelAbility.level <= currentLevel)
                {
                    pawn.abilities.GainAbility(levelAbility.abilityDef);
                    var ability = pawn.abilities.GetAbility(levelAbility.abilityDef);

                    if (cooldownTicks.ContainsKey(levelAbility.abilityDef) && cooldownTicks[levelAbility.abilityDef] > 0)
                    {
                        ability.StartCooldown(cooldownTicks[levelAbility.abilityDef]);
                    }
           
                    grantedAbilities.Add(ability);
                }
            }
        }



        private void RemoveGrantedAbilities(Pawn pawn)
        {
            if (pawn != null)
            {
                foreach (var ability in grantedAbilities)
                {
                    cooldownTicks[ability.def] = ability.CooldownTicksRemaining;
                    pawn.abilities.RemoveAbility(ability.def);
                }
            }

            grantedAbilities.Clear();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            EventManager.Instance.OnAbilityCompleted -= Notify_WearerUsedAbility;
            RemoveGrantedAbilities(EquipOwner);
            base.PostDestroy(mode, previousMap);
        }
        
        private void Notify_WearerUsedAbility(Pawn caster, Ability ability)
        {
            if (caster == EquipOwner && grantedAbilities.Contains(ability))
            {
                AddXp(Props.xpPerUse);
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref grantedAbilities, "grantedAbilities", LookMode.Reference);
            Scribe_Collections.Look(ref cooldownTicks, "cooldownTicks", LookMode.Def, LookMode.Value, ref WorkingKeys, ref WorkingValues);
        }
    }

    //public class CompProperties_MagicalTome : CompProperties
    //{
    //    public List<AbilityDef> abilityDefs;

    //    public CompProperties_MagicalTome()
    //    {
    //        this.compClass = typeof(Comp_MagicalTome);
    //    }
    //}

    //public class Comp_MagicalTome : ThingComp
    //{
    //    private List<Ability> abilities = new List<Ability>();

    //    private CompProperties_MagicalTome Props => (CompProperties_MagicalTome)props;

    //    public List<Ability> AbilitiesForReading
    //    {
    //        get
    //        {
    //            if (this.abilities.NullOrEmpty())
    //            {
    //                this.abilities = new List<Ability>();
    //                foreach (AbilityDef abilityDef in Props.abilityDefs)
    //                {
    //                    this.abilities.Add(AbilityUtility.MakeAbility(abilityDef, ));
    //                }
    //            }
    //            return this.abilities;
    //        }
    //    }

    //    public virtual void UsedOnce()
    //    {

    //    }

    //    public override void Notify_Equipped(Pawn pawn)
    //    {
    //        foreach (Ability ability in AbilitiesForReading)
    //        {
    //            ability.pawn = pawn;
    //            ability.verb.caster = pawn;
    //        }

    //        pawn.abilities.Notify_TemporaryAbilitiesChanged();
    //    }

    //    public override void Notify_Unequipped(Pawn pawn)
    //    {
    //        foreach (Ability ability in AbilitiesForReading)
    //        {
    //            ability.pawn = pawn;
    //            ability.verb.caster = pawn;
    //        }

    //        pawn.abilities.Notify_TemporaryAbilitiesChanged();
    //    }

    //    public override void PostExposeData()
    //    {
    //        base.PostExposeData();
    //        Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);

    //        if (Scribe.mode == LoadSaveMode.PostLoadInit && Holder != null)
    //        {
    //            foreach (Ability ability in AbilitiesForReading)
    //            {
    //                ability.pawn = Holder;
    //                ability.verb.caster = Holder;
    //            }
    //        }
    //    }
    //}


}
