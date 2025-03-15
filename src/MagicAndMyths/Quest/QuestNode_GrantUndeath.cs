using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MagicAndMyths
{
    public class QuestNode_GrantUndeath : QuestNode
	{
		public SlateRef<string> inSignal;
		public SlateRef<string> colonistQuestSubject;
		protected override bool TestRunInt(Slate slate)
		{
			string questSubject = colonistQuestSubject.GetValue(slate);
			if (string.IsNullOrEmpty(questSubject))
            {
				return false;
            }
			return Find.CurrentMap.mapPawns.AllHumanlikeSpawned.Any(x => x.ThingID == questSubject);
		}

		protected override void RunInt()
        {
			QuestPart_GrantUndead grantUndead = new QuestPart_GrantUndead()
			{
				inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(QuestGen.slate)),
				colonistQuestSubject = colonistQuestSubject.GetValue(QuestGen.slate)
			};
			QuestGen.quest.AddPart(grantUndead);
		}

    }


	public class QuestPart_GrantUndead : QuestPart
	{
		public string inSignal;
		public string colonistQuestSubject;
		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);

			if (signal.tag == inSignal)
			{
				Pawn pawn = Current.Game.CurrentMap.mapPawns.AllHumanlikeSpawned.Find(x => x.ThingID == colonistQuestSubject);
				if (pawn != null)
				{
					pawn.health.AddHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);
				}
			}

		}
	}
}
