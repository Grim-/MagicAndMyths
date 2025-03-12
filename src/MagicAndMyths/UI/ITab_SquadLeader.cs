using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ITab_SquadLeader : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private const float ROW_HEIGHT = 40f;
        private const float ICON_SIZE = 30f;
        private const float LABEL_WIDTH = 100f;
        private const float HEALTH_WIDTH = 80f;
        private const float BUTTON_WIDTH = 70f;
        private const float COLUMN_SPACING = 5f;
        private const float SPACING = 5f;
        private int CurrentTabIndex = 0;


        private ISquadLeader _UndeadMaster;
        private ISquadLeader UndeadMaster
        {
            get
            {
                if (_UndeadMaster == null)
                {

                    if (this.SelPawn.TryGetSquadLeader(out ISquadLeader squadLeader))
                    {
                        _UndeadMaster = squadLeader;
                    }
                }

                return _UndeadMaster;
            }
        }

        public override bool IsVisible => base.IsVisible && this.SelPawn != null && this.SelPawn.TryGetSquadLeader(out ISquadLeader squadLeader);


        public ITab_SquadLeader()
        {
            this.labelKey = "Undead";
            this.tutorTag = "Undead";
            this.size = new Vector2(500f, 450f);
        }
        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, 5000f);
            Pawn pawn = (Pawn)this.SelPawn;
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);

            if (pawn != null && UndeadMaster != null)
            {
                DrawControlButtonsRow(listingStandard);
                listingStandard.Gap(10f);
                listingStandard.GapLine();
                DrawAbsorbedCreaturesList(pawn, listingStandard);
            }
            else
            {
                listingStandard.Label("No data available");
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DrawControlButtonsRow(Listing_Standard listingStandard)
        {
            float buttonHeight = 24f;
            float buttonMargin = 10f;
            float availableWidth = listingStandard.ColumnWidth;
            float buttonWidth = (availableWidth - (4 * buttonMargin)) / 5;

            Rect buttonsRow = listingStandard.GetRect(buttonHeight);
            DrawDebugGenerateButton(new Rect(buttonsRow.x + (buttonWidth + buttonMargin) * 2, buttonsRow.y, buttonWidth, buttonHeight));
            DrawToggleCallToArmsButton(new Rect(buttonsRow.x + (buttonWidth + buttonMargin) * 3, buttonsRow.y, buttonWidth, buttonHeight));
            DrawToggleColonistBehaviourButton(new Rect(buttonsRow.x + (buttonWidth + buttonMargin) * 4, buttonsRow.y, buttonWidth, buttonHeight));
        }

        private void DrawDebugGenerateButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Debug: Gen10"))
            {
                //for (int i = 0; i < 10; i++)
                //{
                //    Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                //        kind: PawnKindDefOf.Colonist,
                //        mustBeCapableOfViolence: true,
                //        allowDead: false,
                //        faction: Faction.OfPlayer,
                //        context: PawnGenerationContext.NonPlayer
                //    ));

                //    if (newPawn != null)
                //    {
                //        UndeadMaster.StoreCreature(newPawn);
                //    }
                //}
            }
            TooltipHandler.TipRegion(rect, "Debug Generate 10");
        }

        private void DrawToggleCallToArmsButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Toggle CTA"))
            {
                UndeadMaster.SetAllState(SquadMemberState.CalledToArms);
            }
            TooltipHandler.TipRegion(rect, "Toggle Call To Arms");
        }

        private void DrawToggleColonistBehaviourButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Toggle Colonist"))
            {
                UndeadMaster.SetAllState(SquadMemberState.AtEase);
            }
            TooltipHandler.TipRegion(rect, "Toggle Allow Colonist Behaviour");
        }


        private void DrawAbsorbedCreaturesList(Pawn pawn, Listing_Standard listingStandard)
        {
            foreach (var group in UndeadMaster.SquadMembersPawns.ToArray())
            {
                DrawRow(pawn, group, listingStandard);
            }
        }

        private void DrawRow(Pawn pawn, Pawn absorbedCreature, Listing_Standard listingStandard)
        {
            Rect rowRect = listingStandard.GetRect(ROW_HEIGHT);
            var layout = new RowLayoutManager(rowRect);
            Rect iconRect = layout.NextRect(ICON_SIZE, COLUMN_SPACING);
            Rect labelRect = layout.NextRect(LABEL_WIDTH, COLUMN_SPACING);
            Rect healthRect = layout.NextRect(HEALTH_WIDTH, COLUMN_SPACING);
            Rect removeButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);

            Hediff_Undead undead = absorbedCreature.health.hediffSet.GetFirstHediff<Hediff_Undead>();

            Widgets.HyperlinkWithIcon(iconRect, new Dialog_InfoCard.Hyperlink(absorbedCreature, -1, true));

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, $"{absorbedCreature.Label}");
            Widgets.FillableBar(healthRect, absorbedCreature.health.summaryHealth.SummaryHealthPercent);
            Text.Anchor = TextAnchor.UpperLeft;
            if (UndeadMaster.IsPartOfSquad(absorbedCreature))
            {
                if (Widgets.ButtonText(removeButtonRect, "Remove"))
                {
                    UndeadMaster.RemoveFromSquad(absorbedCreature);
                }
            }

            if (undead != null)
            {
                if (Widgets.ButtonInvisible(iconRect, true))
                {
                    List<FloatMenuOption> gridOptions = new List<FloatMenuOption>();
                    gridOptions.Add(new FloatMenuOption("Call To Arms", () =>
                    {
                        undead.SetCurrentMemberState(SquadMemberState.CalledToArms);
                    }));

                    gridOptions.Add(new FloatMenuOption("At Ease", () =>
                    {
                        undead.SetCurrentMemberState(SquadMemberState.AtEase);
                    }));

                    gridOptions.Add(new FloatMenuOption("Do Nothing", () =>
                    {
                        undead.SetCurrentMemberState(SquadMemberState.DoNothing);
                    }));
                    Find.WindowStack.Add(new FloatMenu(gridOptions));
                }


                TooltipHandler.TipRegion(iconRect, $"{undead.CurrentState}");
                Widgets.DrawHighlightIfMouseover(iconRect);
            }


            if (Widgets.ButtonText(removeButtonRect, "Remove"))
            {
                UndeadMaster.RemoveFromSquad(absorbedCreature);
            }

            Widgets.DrawLineHorizontal(iconRect.x, iconRect.yMax, iconRect.width);
        }

    }
}
