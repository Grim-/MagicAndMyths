using RimWorld;
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
            // Main container rectangle with padding
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Pawn pawn = (Pawn)this.SelPawn;

            if (pawn != null && UndeadMaster != null)
            {
                // Calculate heights for different sections
                float controlButtonsHeight = BUTTON_HEIGHT + SPACING;
                float dividerHeight = 10f;
                float squadListAreaHeight = rect.height - controlButtonsHeight - dividerHeight;

                // Draw control buttons at the top
                Rect controlButtonsRect = new Rect(rect.x, rect.y, rect.width, controlButtonsHeight);
                DrawControlButtons(controlButtonsRect);

                // Add divider
                Rect dividerRect = new Rect(rect.x, controlButtonsRect.yMax, rect.width, dividerHeight);
                Widgets.DrawLineHorizontal(dividerRect.x, dividerRect.y + dividerRect.height / 2, dividerRect.width);

                // Squad list area below
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

            // Will capacity bar
            Rect willBarRect = new Rect(rect.x, rect.y, willBarWidth, buttonHeight);
            Widgets.FillableBar(willBarRect, this.UndeadMaster.WillCapacityAsPercent);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(willBarRect, $"Will: {this.UndeadMaster.WillRequiredForUndead} / {this.UndeadMaster.WillStat}");
            Text.Anchor = TextAnchor.UpperLeft;

            // Squad manager button
            Rect squadButtonRect = new Rect(willBarRect.xMax + buttonMargin, rect.y, squadButtonWidth, buttonHeight);
            if (Widgets.ButtonText(squadButtonRect, "Squad"))
            {
                Find.WindowStack.Add(new SquadManagerWindow(this.UndeadMaster));
            }
        }

        private void DrawSquadList(Rect rect)
        {
            if (UndeadMaster.ActiveSquads == null || UndeadMaster.ActiveSquads.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "No active squads. Create squads in the Squad Manager.");
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            // Use the reusable squad display utility
            squadDisplay.DrawSquadsList(rect, ref scrollPosition, UndeadMaster.ActiveSquads, UndeadMaster);
        }
    }

    //public class ITab_UndeadMaster : ITab
    //{
    //    private Vector2 scrollPosition = Vector2.zero;
    //    private const float ROW_HEIGHT = 40f;
    //    private const float ICON_SIZE = 30f;
    //    private const float LABEL_WIDTH = 130f;
    //    private const float HEALTH_WIDTH = 80f;
    //    private const float BUTTON_WIDTH = 70f;
    //    private const float COLUMN_SPACING = 5f;
    //    private const float SPACING = 5f;
    //    private int CurrentTabIndex = 0;


    //    private Hediff_UndeadMaster _UndeadMaster;
    //    private Hediff_UndeadMaster UndeadMaster
    //    {
    //        get
    //        {
    //            if (_UndeadMaster == null)
    //            {
    //                _UndeadMaster = this.SelPawn.health.hediffSet.GetFirstHediffOfDef(MagicAndMythDefOf.DeathKnight_UndeadMaster) as Hediff_UndeadMaster;
    //            }

    //            return _UndeadMaster;
    //        }
    //    }

    //    public override bool IsVisible => base.IsVisible && this.SelPawn != null && this.SelPawn.health.hediffSet.HasHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);


    //    public ITab_UndeadMaster()
    //    {
    //        this.labelKey = "Undead";
    //        this.tutorTag = "Undead";
    //        this.size = new Vector2(500f, 450f);
    //    }
    //    protected override void FillTab()
    //    {
    //        Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
    //        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, 5000f);
    //        Pawn pawn = (Pawn)this.SelPawn;
    //        Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
    //        Listing_Standard listingStandard = new Listing_Standard();
    //        listingStandard.Begin(viewRect);



    //        if (pawn != null && UndeadMaster != null)
    //        {

    //            SquadDisplayUtility squadDisplay = new SquadDisplayUtility();

    //            squadDisplay.DrawSquadsList(viewRect, scrollPosition, UndeadMaster.ActiveSquads, UndeadMaster);

    //            var absorbedCreatures = UndeadMaster.AllActive;
    //            DrawControlButtonsRow(listingStandard, absorbedCreatures);
    //            listingStandard.Gap(10f);
    //            listingStandard.GapLine();
    //        }
    //        else
    //        {
    //            listingStandard.Label("No data available");
    //        }

    //        listingStandard.End();
    //        Widgets.EndScrollView();
    //    }

    //    private void DrawControlButtonsRow(Listing_Standard listingStandard, List<Pawn> absorbedCreatures)
    //    {
    //        float buttonHeight = 24f;
    //        float buttonMargin = 10f;
    //        float availableWidth = listingStandard.ColumnWidth;
    //        float buttonWidth = (availableWidth - (4 * buttonMargin)) / 5; 

    //        Rect buttonsRow = listingStandard.GetRect(buttonHeight);
    //        Rect fillable = new Rect(buttonsRow.x, buttonsRow.y, buttonWidth, buttonHeight);

    //        Widgets.FillableBar(fillable, this.UndeadMaster.WillCapacityAsPercent);
    //        Widgets.Label(fillable, $"Will :  {this.UndeadMaster.WillRequiredForUndead} / {this.UndeadMaster.WillStat}");


    //        if (Widgets.ButtonText(listingStandard.GetRect(24), "Squad"))
    //        {
    //            Find.WindowStack.Add(new SquadManagerWindow(this.UndeadMaster));
    //        }
    //    }

    //    private void DrawRow(Pawn pawn, Pawn absorbedCreature, Listing_Standard listingStandard)
    //    {
    //        Rect rowRect = listingStandard.GetRect(ROW_HEIGHT);
    //        var layout = new RowLayoutManager(rowRect);
    //        Rect iconRect = layout.NextRect(ICON_SIZE, COLUMN_SPACING);
    //        Rect labelRect = layout.NextRect(LABEL_WIDTH, COLUMN_SPACING);
    //        Rect healthRect = layout.NextRect(layout.RemainingWidth - 70f);
    //        Rect removeButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);

    //        Hediff_Undead undead = absorbedCreature.health.hediffSet.GetFirstHediff<Hediff_Undead>();

    //        Widgets.HyperlinkWithIcon(iconRect, new Dialog_InfoCard.Hyperlink(absorbedCreature, -1, true));

    //        Text.Anchor = TextAnchor.MiddleLeft;
    //        Widgets.Label(labelRect, $"{absorbedCreature.Label}");
    //        Widgets.FillableBar(healthRect, absorbedCreature.health.summaryHealth.SummaryHealthPercent);
    //        Text.Anchor = TextAnchor.UpperLeft;
    //        if (UndeadMaster.IsPartOfSquad(absorbedCreature))
    //        {
    //            if (Widgets.ButtonText(removeButtonRect, "Remove"))
    //            {
    //                UndeadMaster.RemoveFromSquad(absorbedCreature);
    //            }
    //        }

    //        if (undead != null)
    //        {
    //            if (Widgets.ButtonInvisible(iconRect, true))
    //            {
    //                List<FloatMenuOption> gridOptions = new List<FloatMenuOption>();
    //                gridOptions.Add(new FloatMenuOption("Call To Arms", () =>
    //                {
    //                    undead.SetCurrentMemberState(SquadMemberState.CalledToArms);
    //                }));

    //                gridOptions.Add(new FloatMenuOption("At Ease", () =>
    //                {
    //                    undead.SetCurrentMemberState(SquadMemberState.AtEase);
    //                }));

    //                gridOptions.Add(new FloatMenuOption("Do Nothing", () =>
    //                {
    //                    undead.SetCurrentMemberState(SquadMemberState.DoNothing);
    //                }));
    //                Find.WindowStack.Add(new FloatMenu(gridOptions));
    //            }


    //            TooltipHandler.TipRegion(iconRect, $"Change State\n\nCurrent:{undead.CurrentState}");
    //            Widgets.DrawHighlightIfMouseover(iconRect);
    //        }


    //        if (Widgets.ButtonText(removeButtonRect, "Remove"))
    //        {
    //            UndeadMaster.RemoveFromSquad(absorbedCreature, true);
    //        }

    //        Widgets.DrawLineHorizontal(iconRect.x, iconRect.yMax, iconRect.width);
    //    }
    //}
}
