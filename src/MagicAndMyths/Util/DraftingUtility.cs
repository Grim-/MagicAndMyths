using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public static class DraftingUtility
    {
        public static WorldComponent_DraftableCreatures DraftManager
        {
            get
            {
                if (Current.Game != null && Current.Game.World != null)
                {
                    return Current.Game.World.GetComponent<WorldComponent_DraftableCreatures>();
                }

                return null;
            }
        }

        public static void DropAndEquip(this Pawn_EquipmentTracker equipment, ThingWithComps newEquipment)
        {
            if (equipment?.pawn == null || newEquipment == null) return;

            // Check for existing weapon in the same equipment slot
            var existingEquipment = equipment.AllEquipmentListForReading
                .FirstOrDefault(x => x.def.equipmentType == newEquipment.def.equipmentType);

            if (existingEquipment != null)
            {
                // Drop the existing equipment near the pawn
                equipment.Remove(existingEquipment);
                GenPlace.TryPlaceThing(existingEquipment, equipment.pawn.Position, equipment.pawn.Map, ThingPlaceMode.Near);
            }

            // Equip the new weapon
            equipment.AddEquipment(newEquipment);
        }
        public static void RegisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.RegisterDraftableCreature(pawn);
            }
        }

        public static void UnregisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.UnregisterDraftableCreature(pawn);
            }
        }

        public static bool IsDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                return DraftManager.IsDraftableCreature(pawn);
            }

            return false;
        }

        public static void MakeDraftable(this Pawn pawn)
        {
            RegisterDraftableCreature(pawn);
        }
    }
}
