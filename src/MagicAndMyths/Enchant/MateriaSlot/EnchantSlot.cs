//using LudeonTK;
//using RimWorld;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;

//namespace MagicAndMyths
//{
//    public class EnchantSlot : IExposable
//    {
//        protected EnchantInstance _EnchantInstance;
//        public EnchantInstance EnchantInstance
//        {
//            get
//            {
//                return _EnchantInstance;
//            }
//        }

//        protected ThingWithComps _ParentThing;
//        public ThingWithComps ParentThing
//        {
//            get
//            {
//                return _ParentThing;
//            }
//        }

//        protected Comp_Enchant _EnchantComp;
//        public Comp_Enchant EnchantComp => _EnchantComp;
//        public bool IsOccupied => _EnchantInstance != null;


//        public string EffectDescription => _EnchantInstance != null ? string.Join("\n", _EnchantInstance.ActiveEffects.Select(x => x.def.EffectDescription)) : null;


//        protected int _SlotLevel = 1;
//        public int SlotLevel
//        {
//            get
//            {
//                return _SlotLevel;
//            }
//        }

//        //protected int _SlotIndex = -1;
//        //public int SlotIndex
//        //{
//        //    get
//        //    {
//        //        if (EnchantComp == null)
//        //        {
//        //            return -1;
//        //        }

//        //        return EnchantComp.ActiveEnchants.IndexOf(this);
//        //    }
//        //}

//        protected bool _IsSlotLocked = false;
//        public bool IsSlotLocked
//        {
//            get
//            {
//                return _IsSlotLocked;
//            }
//        }

//        public EnchantSlot()
//        {
//        }

//        public EnchantSlot(ThingWithComps parentThing, Comp_Enchant materiaComp, int slotLevel = 1)
//        {
//            _EnchantComp = materiaComp;
//            _ParentThing = parentThing;
//            _SlotLevel = slotLevel;
//        }



//        public void EquipMateria(EnchantDef materiaDef, bool force = false)
//        {
//            if (!CanEquipMateria(materiaDef) && !force)
//                return;

//            _EnchantInstance = new EnchantInstance(materiaDef, this.EnchantComp);

//            if (EnchantComp.HasEquipOwner)
//            {
//                _EnchantInstance.Notify_Equipped(EnchantComp.EquippedPawn);
//            }

//            _EnchantInstance.Notify_MateriaEquipped();
//        }

//        public void UnequipMateria()
//        {
//            if (_EnchantInstance == null)
//                return;
//            _EnchantInstance.Notify_MateriaUnequipped();

//            if (EnchantComp.HasEquipOwner)
//            {
//                _EnchantInstance.Notify_Unequipped(EnchantComp.EquippedPawn);
//            }

//            _EnchantInstance = null;
//        }

//        public bool CanAccept(EnchantDef Materia)
//        {
//            if (Materia == null)
//                return false;

//            return AcceptsType(Materia.SlotType) && IsValidLevel(Materia) && IsValidEquipmentType(Materia, this.ParentThing) && !IsSlotLocked;
//        }


//        public bool AcceptsType(MateriaTypeDef materiaTypeDef)
//        {
//            return true;
//        }

//        public bool IsValidLevel(EnchantDef Materia)
//        {
//            return this.SlotLevel >= Materia.slotLevel;
//        }

//        public bool IsValidEquipmentType(EnchantDef Materia, Thing Thing)
//        {
//            return Materia.materiaCategories == null ||
//            (Thing.def.IsMeleeWeapon && Materia.materiaCategories.Contains(MagicAndMythDefOf.MateriaCategory_Melee)) ||
//            (Thing.def.IsRangedWeapon && Materia.materiaCategories.Contains(MagicAndMythDefOf.MateriaCategory_Ranged)) ||
//            (Thing.def.IsApparel && Materia.materiaCategories.Contains(MagicAndMythDefOf.MateriaCategory_Armor));
//        }

//        public void AddLevel(int levelsToAdd)
//        {
//            this._SlotLevel += levelsToAdd;
//        }

//        public void SetLevel(int level)
//        {
//            this._SlotLevel = level;
//        }

//        public void RemoveLevel(int levelsToRemove)
//        {
//            this._SlotLevel -= levelsToRemove;
//            this._SlotLevel = Mathf.Clamp(_SlotLevel, 1, 1000);
//        }

//        public void Reset()
//        {
//            this._SlotLevel = 1;
//        }

//        public void SetLockStatus(bool IsLocked)
//        {
//            this._IsSlotLocked = IsLocked;
//        }

//        public bool CanEquipMateria(EnchantDef materia)
//        {
//            return !IsOccupied && CanAccept(materia);
//        }

//        public void ExposeData()
//        {
//            Scribe_Deep.Look(ref _EnchantInstance, "SlottedMateria");
//            Scribe_References.Look(ref _ParentThing, "ParentThing");
//            //Scribe_Values.Look(ref _SlotLevel, "SlotLevel");
//            //Scribe_Values.Look(ref _SlotIndex, "SlotIndex");
//            Scribe_Values.Look(ref _IsSlotLocked, "SlotLocked");
//        }
//    }
//}
