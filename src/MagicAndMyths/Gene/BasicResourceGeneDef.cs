using Verse;

namespace MagicAndMyths
{
    public class BasicResourceGeneDef : GeneDef
    {
        public PawnResourceDef resourceDef;

        public BasicResourceGeneDef()
        {
            geneClass = typeof(Gene_BasicResource);
            this.resourceGizmoType = typeof(GeneGizmo_BasicResource);
        }
    }
}
