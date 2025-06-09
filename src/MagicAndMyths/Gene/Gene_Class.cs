using Verse;

namespace MagicAndMyths
{
    public class ClassGeneDef : GeneDef
    {
        public AbilityResourceDef primaryResourceDef;

        public ClassGeneDef()
        {
            geneClass = typeof(Gene_Class);
        }
    }

    public class Gene_Class : Gene
    {
        ClassGeneDef Def => (ClassGeneDef)def;

        protected Gene_BasicResource _PrimaryResourceGene;
        protected Gene_BasicResource PrimaryResourceGene
        {
            get
            {
                if (_PrimaryResourceGene == null)
                {
                    _PrimaryResourceGene = this.pawn.GetGeneForResourceDef(Def.primaryResourceDef);
                }

                return _PrimaryResourceGene;
            }
        }


        public override bool Active => HasRequiredResourceFromGene() ? base.Active : false;

        public override string Label => !Active ? $"Requires {Def.primaryResourceDef.LabelCap}" : base.Label;

        public bool HasRequiredResourceFromGene()
        {
            return PrimaryResourceGene != null;
        }
    }
}