﻿using RimWorld;
using SquadBehaviour;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ITab_UndeadMaster : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private const float BUTTON_HEIGHT = 24f;
        private const float SPACING = 10f;
        private int CurrentTabIndex = 0;
        private SquadDisplayUtility squadDisplay;

        private Hediff_UndeadMaster _UndeadMaster;
        private Hediff_UndeadMaster UndeadMaster
        {
            get
            {
                if (_UndeadMaster == null)
                {
                    _UndeadMaster = this.SelPawn.health.hediffSet.GetFirstHediffOfDef(MagicAndMythDefOf.DeathKnight_UndeadMaster) as Hediff_UndeadMaster;
                }

                return _UndeadMaster;
            }
        }

        public override bool IsVisible => base.IsVisible && this.SelPawn != null && this.SelPawn.health.hediffSet.HasHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);

        public ITab_UndeadMaster()
        {
            this.labelKey = "Undead";
            this.tutorTag = "Undead";
            this.size = new Vector2(500f, 450f);
            this.squadDisplay = new SquadDisplayUtility();
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Pawn pawn = (Pawn)this.SelPawn;

            if (pawn != null && UndeadMaster != null)
            {
                float controlButtonsHeight = BUTTON_HEIGHT + SPACING;
                float dividerHeight = 10f;
                float squadListAreaHeight = rect.height - controlButtonsHeight - dividerHeight;


                Rect controlButtonsRect = new Rect(rect.x, rect.y, rect.width, controlButtonsHeight);
                DrawControlButtons(controlButtonsRect);


                Rect dividerRect = new Rect(rect.x, controlButtonsRect.yMax, rect.width, dividerHeight);
                Widgets.DrawLineHorizontal(dividerRect.x, dividerRect.y + dividerRect.height / 2, dividerRect.width);

                Rect squadListRect = new Rect(rect.x, dividerRect.yMax, rect.width, squadListAreaHeight);
                DrawSquadList(squadListRect);
            }
            else
            {
                Widgets.Label(rect, "No data available");
            }
        }

        private void DrawControlButtons(Rect rect)
        {
            float buttonHeight = BUTTON_HEIGHT;
            float buttonMargin = 10f;
            float availableWidth = rect.width;
            float willBarWidth = availableWidth * 0.4f;
            float squadButtonWidth = availableWidth * 0.2f;


            Rect willBarRect = new Rect(rect.x, rect.y, willBarWidth, buttonHeight);
            Widgets.FillableBar(willBarRect, this.UndeadMaster.WillCapacityAsPercent);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(willBarRect, $"Will: {this.UndeadMaster.WillRequiredForUndead} / {this.UndeadMaster.WillStat}");
            Text.Anchor = TextAnchor.UpperLeft;

            Rect squadButtonRect = new Rect(willBarRect.xMax + buttonMargin, rect.y, squadButtonWidth, buttonHeight);
            if (Widgets.ButtonText(squadButtonRect, "Squad"))
            {
                Find.WindowStack.Add(new SquadManagerWindow(this.UndeadMaster.SquadLeaderComp));
            }
        }

        private void DrawSquadList(Rect rect)
        {
            if (UndeadMaster.SquadLeaderComp.ActiveSquads == null || UndeadMaster.SquadLeaderComp.ActiveSquads.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "No active squads. Create squads in the Squad Manager.");
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            squadDisplay.DrawSquadsList(rect, ref scrollPosition, UndeadMaster.SquadLeaderComp.ActiveSquads, this.UndeadMaster.SquadLeaderComp);
        }
    }
}
