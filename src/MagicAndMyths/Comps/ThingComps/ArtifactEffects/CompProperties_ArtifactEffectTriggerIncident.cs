using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectTriggerIncident : CompProperties
    {
        public IncidentDef incidentDef;

        public CompProperties_ArtifactEffectTriggerIncident()
        {
            compClass = typeof(Comp_ArtifactEffectTriggerIncident);
        }
    }


    public class Comp_ArtifactEffectTriggerIncident : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectTriggerIncident Props => (CompProperties_ArtifactEffectTriggerIncident)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (user.Map != null && Props.incidentDef != null)
            {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(Props.incidentDef.category, user.Map);
                parms.forced = true;
                Props.incidentDef.Worker.TryExecute(parms);
            }
        }
    }
}