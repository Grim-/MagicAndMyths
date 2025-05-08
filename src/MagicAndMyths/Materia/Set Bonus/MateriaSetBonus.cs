//using System.Collections.Generic;
//using System.Linq;
//using Verse;

//namespace MagicAndMyths
//{
//    public class MateriaSetBonus : IExposable
//    {
//        public string label;
//        public string description;
//        public List<MateriaSetBonusRequirement> requirements = new List<MateriaSetBonusRequirement>();
//        public List<EnchantEffectDef> bonusEffects = new List<EnchantEffectDef>();
//        public bool isActive = false;

//        // Cached active effects
//        private List<EnchantWorker> activeEffects = new List<EnchantWorker>();

//        public List<EnchantWorker> ActiveEffects => activeEffects;

//        public MateriaSetBonus()
//        {
//        }

//        public void InitializeEffects(ThingWithComps parent, Comp_MateriaSetBonus comp)
//        {
//            activeEffects.Clear();

//            foreach (var effectDef in bonusEffects)
//            {
//                var worker = effectDef.CreateWorker(parent, null);
//                activeEffects.Add(worker);
//            }
//        }

//        public bool CheckRequirements(Pawn pawn)
//        {
//            if (pawn == null)
//                return false;

//            // Get all Comp_Materia components on the pawn's equipment and apparel
//            List<Comp_Enchant> materiaComps = new List<Comp_Enchant>();

//            if (pawn.equipment?.AllEquipmentListForReading != null)
//            {
//                foreach (var eq in pawn.equipment.AllEquipmentListForReading)
//                {
//                    var comp = eq.GetComp<Comp_Enchant>();
//                    if (comp != null)
//                        materiaComps.Add(comp);
//                }
//            }

//            if (pawn.apparel?.WornApparel != null)
//            {
//                foreach (var apparel in pawn.apparel.WornApparel)
//                {
//                    var comp = apparel.GetComp<Comp_Enchant>();
//                    if (comp != null)
//                        materiaComps.Add(comp);
//                }
//            }


//            return true;
//        }

//        public void ExposeData()
//        {
//            Scribe_Values.Look(ref label, "label");
//            Scribe_Values.Look(ref description, "description");
//            Scribe_Values.Look(ref isActive, "isActive", false);
//            Scribe_Collections.Look(ref requirements, "requirements", LookMode.Deep);
//            Scribe_Collections.Look(ref bonusEffects, "bonusEffects", LookMode.Def);
//            Scribe_Collections.Look(ref activeEffects, "activeEffects", LookMode.Deep);
//        }
//    }

//    public class MateriaSetBonusRequirement
//    {
//        public MateriaTypeDef materiaType;
//        public int requiredCount;
//        public List<EnchantDef> specificMateria; // Optional: for requiring specific materia instead of just types

//        public MateriaSetBonusRequirement()
//        {
//        }

//        public MateriaSetBonusRequirement(MateriaTypeDef type, int count)
//        {
//            materiaType = type;
//            requiredCount = count;
//        }
//    }
//}
