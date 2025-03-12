using RimWorld.Planet;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MagicAndMyths
{
    public class QuestNode_SpecificPawnKilled : QuestNode
	{
		public SlateRef<string> pawnThingID;
		public SlateRef<string> outSignal;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_SpecificPawnKilled reward = new QuestPart_SpecificPawnKilled
			{
				pawnThingID = pawnThingID.GetValue(slate),
				outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate))
			};
			QuestGen.quest.AddPart(reward);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return !pawnThingID.GetValue(slate).NullOrEmpty();
		}
	}


	public class QuestPart_SpecificPawnKilled : QuestPart
	{
		public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
		{
			base.Notify_PawnKilled(pawn, dinfo);
			if (pawn.ThingID == pawnThingID)
			{
				Find.SignalManager.SendSignal(new Signal(this.outSignal));
				Log.Message($"Sending {this.outSignal} signal");
			}
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.pawnThingID, "pawnThingID", null, false);
			Scribe_Values.Look<string>(ref this.outSignal, "outSignal", null, false);
		}

		public string pawnThingID;
		public MapParent mapParent;
		public string outSignal;
	}
}
