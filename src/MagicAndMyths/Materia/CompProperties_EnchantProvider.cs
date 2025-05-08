using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_EnchantProvider : CompProperties
    {
        public int maxEnchantsAllowed = 3;
        public List<EnchantmentSlotConfig> enchantmentSlots;

        public CompProperties_EnchantProvider()
        {
            compClass = typeof(Comp_EnchantProvider);
        }
    }


    public class Comp_EnchantProvider : ThingComp
    {
        private List<EnchantInstance> enchants = new List<EnchantInstance>();
        public List<EnchantInstance> Enchants => enchants.ToList();

        private bool initialized = false;
        private CompProperties_EnchantProvider Props => (CompProperties_EnchantProvider)props;

        protected Pawn _EquippedPawn = null;
        public Pawn EquippedPawn => _EquippedPawn;
        public bool HasEquipOwner => EquippedPawn != null;

        public bool HasMaximumEnchantsAllowed => enchants.Count >= Props.maxEnchantsAllowed;
        public int EnchantCount => this.enchants.Count;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                if (!initialized)
                {
                    if (Props.enchantmentSlots != null && Props.enchantmentSlots.Count > 0)
                    {
                        InitializeEnchants();
                    }
                    initialized = true;
                }
            }
        }

        private void InitializeEnchants()
        {
            if (enchants == null)
            {
                enchants = new List<EnchantInstance>();
            }

            if (Props.enchantmentSlots != null)
            {
                foreach (var slotConfig in Props.enchantmentSlots)
                {
                    if (slotConfig.enchantToAutoSlot != null)
                    {
                        AddEnchant(slotConfig.enchantToAutoSlot);
                    }
                }
            }

            for (int i = 0; i < Props.maxEnchantsAllowed; i++)
            {
                if (HasMaximumEnchantsAllowed)
                {
                    break;
                }

                AddEnchant(DefDatabase<EnchantDef>.AllDefs.Where(x => x.IsValidEquipmentType(this.parent)).RandomElement());
            }
        }

        public bool HasActive(EnchantDef enchantDef)
        {
            return enchants != null && enchants.Any(x => x.def == enchantDef);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (enchants != null)
            {
                foreach (var enchant in enchants)
                {
                    enchant.Notify_ParentDestroyed();
                }
            }
            base.PostDestroy(mode, previousMap);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (enchants != null)
            {
                foreach (var enchant in enchants)
                {
                    enchant.Notify_ParentDestroyed();
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (enchants != null)
            {
                foreach (var enchant in enchants)
                {
                    enchant.PostDraw();
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            _EquippedPawn = pawn;

            var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null && enchants != null)
            {
                foreach (var enchant in enchants)
                {
                    pawnEnchantComp.AddEnchant(enchant, this.parent);
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null && enchants != null)
            {
                foreach (var enchant in enchants)
                {
                    pawnEnchantComp.RemoveEnchant(enchant);
                }
            }

            _EquippedPawn = null;
        }

        #region enchant management
        public bool CanAddEnchant(EnchantDef enchantDef)
        {
            if (HasMaximumEnchantsAllowed)
                return false;

            return enchantDef.IsValidEquipmentType(this.parent);
        }

        public EnchantInstance AddEnchant(EnchantDef enchantDef, bool force = false)
        {
            if (!CanAddEnchant(enchantDef) && !force)
                return null;

            var enchantInstance = new EnchantInstance(enchantDef, this);
            enchants.Add(enchantInstance);

            enchantInstance.Notify_MateriaEquipped();

            if (HasEquipOwner)
            {
                var pawnEnchantComp = EquippedPawn.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    pawnEnchantComp.AddEnchant(enchantInstance, this.parent);
                }
            }

            return enchantInstance;
        }

        public void RemoveEnchant(EnchantDef enchant, bool createMateriaItem = true)
        {
            if (enchant == null || !enchants.Any(x => x.def == enchant))
                return;
            EnchantInstance enchantInstance = enchants.First(x => x.def == enchant);
            RemoveEnchant(enchantInstance, createMateriaItem);
        }

        public void RemoveEnchant(EnchantInstance enchantInstance, bool createMateriaItem = true)
        {
            if (enchantInstance == null || !enchants.Contains(enchantInstance))
                return;

            enchantInstance.Notify_MateriaUnequipped();

            if (HasEquipOwner)
            {
                var pawnEnchantComp = EquippedPawn.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    pawnEnchantComp.RemoveEnchant(enchantInstance);
                }
            }

            enchants.Remove(enchantInstance);
        }

        public int GetEnchantCount()
        {
            return enchants?.Count ?? 0;
        }
        #endregion

        private string GetEquippedEnchantsInfo(bool includeDescription = false)
        {
            if (enchants.NullOrEmpty())
                return "No enchantments equipped";

            var equippedEnchants = enchants
                .Select(enchant => includeDescription
                    ? $"  [{enchant.def.GetColouredLabel()}] \n     {GetEffectDescription(enchant)}"
                    : $"  [{enchant.def.GetColouredLabel()}]")
                .ToList();

            StringBuilder output = new StringBuilder();
            output.AppendLine(string.Join(Environment.NewLine, equippedEnchants));
            return output.ToString().TrimEnd();
        }

        private string GetEffectDescription(EnchantInstance enchant)
        {
            return string.Join("\n", enchant.ActiveEffects.Select(x => x.def.EffectDescription));
        }

        public override string CompInspectStringExtra()
        {
            return GetEquippedEnchantsInfo();
        }

        public override string GetDescriptionPart()
        {
            string equippedEnchantsString = GetEquippedEnchantsInfo(true);
            return equippedEnchantsString;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref initialized, "initialized", false);
            Scribe_References.Look(ref _EquippedPawn, "equippedPawn");
            Scribe_Collections.Look(ref enchants, "enchants", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (enchants == null)
                {
                    enchants = new List<EnchantInstance>();
                    InitializeEnchants();
                }
            }
        }
    }
}
