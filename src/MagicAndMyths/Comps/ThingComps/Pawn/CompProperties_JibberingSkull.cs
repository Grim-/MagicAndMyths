using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class CompProperties_JibberingSkull : CompProperties
    {
        public CompProperties_JibberingSkull()
        {
            compClass = typeof(Comp_JibberingSkull);
        }
    }

    public class Comp_JibberingSkull : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            if (selPawn.Faction == Faction.OfPlayer)
            {
                if (Find.QuestManager.QuestsListForReading.Any(x => x.root == MagicAndMythDefOf.Quest_DeathKnightStartingPath))
                {
                    yield return new FloatMenuOption("Talk.. (Quest already started)", () =>
                    {

                    }, MenuOptionPriority.DisabledOption);
                }
                else
                {
                    yield return new FloatMenuOption("Talk..", () =>
                    {
                        Job job = JobMaker.MakeJob(MagicAndMythDefOf.GotoAndTalk, this.parent as Pawn);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                    });
                }

            }
        }
    }

}
