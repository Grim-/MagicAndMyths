using Verse;

namespace MagicAndMyths
{
    public class SpecialThingFilterWorker_Materia : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return t.def != null && t.def is EnchantDef;
        }

    }
}
