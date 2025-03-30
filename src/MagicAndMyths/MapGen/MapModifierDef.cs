using System;
using Verse;

namespace MagicAndMyths
{
    public class MapModifierDef : Def
    {
        public Type modifierWorkerClass;

        public MapModifier CreateModifier(Map map)
        {
            var modifier = (MapModifier)Activator.CreateInstance(modifierWorkerClass);
            modifier.map = map;
            return modifier;
        }
    }
}
