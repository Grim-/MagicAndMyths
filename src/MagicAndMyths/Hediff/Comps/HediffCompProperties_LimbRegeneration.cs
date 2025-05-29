using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_LimbRegeneration : HediffCompProperties
    {
        public int timeToRegen = 2400;
        public HediffCompProperties_LimbRegeneration()
        {
            compClass = typeof(HediffComp_LimbRegeneration);
        }
    }
    public class HediffComp_LimbRegeneration : HediffComp
    {
        private HediffCompProperties_LimbRegeneration Props => (HediffCompProperties_LimbRegeneration)props;
        private int ticksElapsed = 0;
        private bool CanRemoveNow = false;
        private List<BodyPartRecord> PartsToRegenerate = new List<BodyPartRecord>();
        private int ticksPerPart = 0;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            PartsToRegenerate = new List<BodyPartRecord>();
            BodyPartRecord mainPart = HealthUtility.FindBiggestMissingBodyPart(Pawn);
            if (mainPart != null)
            {
                List<BodyPartRecord> partsToRegenerate = new List<BodyPartRecord>();
                foreach (var part in mainPart.GetPartAndAllChildParts())
                {
                    if (Pawn.health.hediffSet.GetMissingPartFor(part) != null)
                    {
                        partsToRegenerate.Add(part);
                    }
                }
                partsToRegenerate = partsToRegenerate.OrderBy(p => p.depth).ToList();
                PartsToRegenerate = partsToRegenerate;

                if (PartsToRegenerate.Count > 0)
                {
                    ticksPerPart = Props.timeToRegen / PartsToRegenerate.Count;
                }
            }

            if (PartsToRegenerate.Count == 0)
            {
                CanRemoveNow = true;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticksElapsed++;

            if (PartsToRegenerate.Count > 0)
            {
                int currentPartIndex = ticksElapsed / ticksPerPart;
                if (ticksElapsed >= Props.timeToRegen)
                {
                    CanRemoveNow = true;
                    return;
                }

                for (int i = 0; i < PartsToRegenerate.Count; i++)
                {
                    int partStartTick = i * ticksPerPart;
                    int partEndTick = (i + 1) * ticksPerPart;

                    if (ticksElapsed >= partStartTick && ticksElapsed < partEndTick)
                    {
                        BodyPartRecord currentPart = PartsToRegenerate[i];
                        Hediff missingPartHediff = Pawn.health.hediffSet.GetMissingPartFor(currentPart);

                        if (missingPartHediff != null && missingPartHediff is Hediff_MissingPart missingPart)
                        {
                            int ticksIntoCurrentPart = ticksElapsed - partStartTick;
                            float healingProgress = (float)ticksIntoCurrentPart / ticksPerPart;
                            float targetSeverity = 1f - healingProgress;
                            missingPart.Severity = targetSeverity;
                        }
                        break;
                    }
                    else if (ticksElapsed == partEndTick)
                    {
                        BodyPartRecord partToRestore = PartsToRegenerate[i];
                        Hediff missingPartHediff = Pawn.health.hediffSet.GetMissingPartFor(partToRestore);
                        HealthUtility.Cure(missingPartHediff);
                        break;
                    }
                }
            }
            else
            {
                CanRemoveNow = true;
            }
        }
        public override bool CompShouldRemove => CanRemoveNow;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksElapsed, "ticksElapsed", 0);
            Scribe_Values.Look(ref CanRemoveNow, "canRemoveNow", false);
            Scribe_Values.Look(ref ticksPerPart, "ticksPerPart", 0);
            Scribe_Collections.Look(ref PartsToRegenerate, "partsToRegenerate", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && PartsToRegenerate == null)
            {
                PartsToRegenerate = new List<BodyPartRecord>();
            }
        }
    }
}