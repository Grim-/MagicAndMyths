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

            if (selPawn.Faction == Faction.OfPlayer && !Find.QuestManager.QuestsListForReading.Any(x=> x.root == MagicAndMythDefOf.Quest_DeathKnightStartingPath))
            {
                yield return new FloatMenuOption("Talk..", () =>
                {
                    // Messages.Message($"SPOKe", MessageTypeDefOf.PositiveEvent);

                        Job job = JobMaker.MakeJob(MagicAndMythDefOf.GotoAndTalk, this.parent as Pawn);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced);

     //               Slate newSlate = new Slate();
					//newSlate.Set<string>("colonistQuestSubject", selPawn.ThingID);
					//newSlate.Set<string>("colonistQuestSubjectName", selPawn.Label);

	
					//Find.QuestManager.Add(QuestUtility.GenerateQuestAndMakeAvailable(MagicAndMythDefOf.Quest_DeathKnightStartingPath, newSlate));
                });
            }
        }
    }

}
