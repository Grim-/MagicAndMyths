using EMF;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class CompProperties_SelectiveBiocodable : CompProperties_Biocodable
    {
        public PawnRequirementParms requirements;

        public EffecterDef onBondEffect;

        public CompProperties_SelectiveBiocodable()
        {
            compClass = typeof(CompSelectiveBiocodable);
        }
    }

    public class RequiredSkillLevel
    {
        public SkillDef skill;
        public int minLevel = 1;
    }

    public class CompSelectiveBiocodable : CompBiocodable
    {
        public CompProperties_SelectiveBiocodable SelectiveProps => (CompProperties_SelectiveBiocodable)props;

        public override bool Biocodable => !base.Biocoded;

        public override void CodeFor(Pawn p)
        {
            if (!CanBeBiocodedFor(p))
            {
                Messages.Message($"{p.Name.ToStringShort} cannot biocode {parent.Label}: requirements not met", MessageTypeDefOf.RejectInput, false);
                return;
            }

            base.CodeFor(p);
        }

        public bool CanBeBiocodedFor(Pawn p)
        {
            if (p == null)
                return false;

            if (SelectiveProps.requirements == null)
                return true;
            return SelectiveProps.requirements.MeetsRequirements(p);
        }


        public override string CompInspectStringExtra()
        {
            var info = new StringBuilder(base.CompInspectStringExtra());

            if (Biocoded) 
                return info.ToString();

            if (SelectiveProps.requirements != null)
            {
                return info.ToString().TrimEndNewlines() + SelectiveProps.requirements.RequirementsExplanation();
            }

            return info.ToString().TrimEndNewlines();
        }
    }
}
