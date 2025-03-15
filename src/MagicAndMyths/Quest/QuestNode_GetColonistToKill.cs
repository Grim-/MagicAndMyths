using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MagicAndMyths
{
    public class QuestNode_GetColonistToKill : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeColonistToHuntAs;
		public SlateRef<string> storeColonistToHuntAsName;
		public SlateRef<string> colonistQuestSubjectName;
		protected override bool TestRunInt(Slate slate)
		{
			return this.DoWork(slate);
		}

		protected override void RunInt()
		{

			
			this.DoWork(QuestGen.slate);
		}

		private bool DoWork(Slate slate)
		{
			Map map = Find.CurrentMap;
			if (map == null)
			{
				return false;
			}

			float x2 = slate.Get<float>("points", 0f, false);

			string questSubject = slate.Get<string>("colonistQuestSubject");

			for (int i = 0; i < map.mapPawns.AllPawnsSpawned.Count; i++)
			{
				Pawn pawn = map.mapPawns.AllPawnsSpawned[i];
				if (!pawn.IsQuestLodger() && pawn.Faction == Faction.OfPlayer && pawn.ThingID != questSubject)
				{
					slate.Set<string>(storeColonistToHuntAsName.GetValue(slate), pawn.Label);
					slate.Set<string>(storeColonistToHuntAs.GetValue(slate), pawn.ThingID);
					break;
				}
			}
			return true;
		}
	}


}
