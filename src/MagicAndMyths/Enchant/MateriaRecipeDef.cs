using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class MateriaRecipeDef : RecipeDef
    {
        public IntRange generatedMateriaLevel = new IntRange(1, 4);
        public List<MateriaTypeDef> allowedTypes;
        public List<MateriaTypeDef> disallowedTypes;
    }
}
