using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MateriaSlotTypeDef : Def
    {
        public Color materiaTextColor;
        public List<MateriaTypeDef> acceptableMateriaTypes;
        public List<SlotLimit> slotLimits;
        public float baseWeight = 1f;

        public List<MateriaWeight> weaponMateriaWeights;
        public List<MateriaWeight> armourMateriaWeights;
    }

    public class MateriaWeight
    {
        public MateriaTypeDef materiaType;
        public float weight = 1;
    }
}
