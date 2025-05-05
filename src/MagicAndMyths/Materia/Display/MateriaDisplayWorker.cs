using System.Collections.Generic;
using UnityEngine;

namespace MagicAndMyths
{
    public abstract class MateriaDisplayWorker
    {
        public abstract Dictionary<EnchantSlot, Rect> PositionSlots(Window_MateriaSelection MateriaWindow,
            List<EnchantSlot> slots,
            Rect availableSpace,
            Vector2 slotSize,
            Vector2 spacing);

        public virtual void DrawToolBar(Rect toolbarRect) { }

        public virtual void DrawSlot(Rect slotRect, EnchantSlot slot) { }

        public virtual float GetLayoutSuitability(List<EnchantSlot> slots)
        {
            return 0.5f; 
        }

        public virtual List<MateriaSlotLinkConfig> GetLinkDefinitions(List<EnchantSlot> slots, Dictionary<EnchantSlot, Rect> positions)
        {
            return new List<MateriaSlotLinkConfig>();
        }

    }

    public class MateriaSlotLinkConfig
    {
        public EnchantSlot FirstSlot;
        public EnchantSlot SecondSlot;
        public bool IsVisual;
    }
}
