using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ITab_UndeadMaster : ITab
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
                var absorbedCreatures = UndeadMaster.GetStored();
                listingStandard.Label($"Total Absorbed Creatures: {absorbedCreatures.Count}");

                DrawControlButtonsRow(listingStandard, absorbedCreatures);
                listingStandard.Gap(10f);
                listingStandard.GapLine();
                DrawAbsorbedCreaturesList(pawn, absorbedCreatures, listingStandard);
            }
            else
            {
                listingStandard.Label("No data available");
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DrawControlButtonsRow(Listing_Standard listingStandard, List<Pawn> absorbedCreatures)
        {
            float buttonHeight = 24f;
            float buttonMargin = 10f;
            float availableWidth = listingStandard.ColumnWidth;
            float buttonWidth = (availableWidth - (4 * buttonMargin)) / 5; // 5 buttons with margins

            Rect buttonsRow = listingStandard.GetRect(buttonHeight);

            DrawSummonAllButton(new Rect(buttonsRow.x, buttonsRow.y, buttonWidth, buttonHeight), absorbedCreatures);
            DrawDismissAllButton(new Rect(buttonsRow.x + buttonWidth + buttonMargin, buttonsRow.y, buttonWidth, buttonHeight));
            DrawDebugGenerateButton(new Rect(buttonsRow.x + (buttonWidth + buttonMargin) * 2, buttonsRow.y, buttonWidth, buttonHeight));
            DrawToggleCallToArmsButton(new Rect(buttonsRow.x + (buttonWidth + buttonMargin) * 3, buttonsRow.y, buttonWidth, buttonHeight));
            DrawToggleColonistBehaviourButton(new Rect(buttonsRow.x + (buttonWidth + buttonMargin) * 4, buttonsRow.y, buttonWidth, buttonHeight));
        }

        private void DrawSummonAllButton(Rect rect, List<Pawn> absorbedCreatures)
        {
            if (Widgets.ButtonText(rect, "Summon all"))
            {
                for (int i = 0; i < absorbedCreatures.Count; i++)
                {
                    UndeadMaster.SummonCreatureInFormation(absorbedCreatures[i]);
                }
            }
            TooltipHandler.TipRegion(rect, "Summon All");
        }

        private void DrawDismissAllButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Dismiss all"))
            {
                UndeadMaster.UnSummonAll();
            }
            TooltipHandler.TipRegion(rect, "Dismiss All");
        }

        private void DrawDebugGenerateButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Debug: Gen10"))
            {
                for (int i = 0; i < 10; i++)
                {
                    Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                        kind: PawnKindDefOf.Colonist,
                        mustBeCapableOfViolence: true,
                        allowDead: false
                    ));

                    if (newPawn != null)
                    {
                        UndeadMaster.AbsorbCreature(newPawn);
                    }
                }
            }
            TooltipHandler.TipRegion(rect, "Debug Generate 10");
        }
   
        private void DrawToggleCallToArmsButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Toggle CTA"))
            {
                UndeadMaster.ToggleALLCallToArms();
            }
            TooltipHandler.TipRegion(rect, "Toggle Call To Arms");
        }

        private void DrawToggleColonistBehaviourButton(Rect rect)
        {
            if (Widgets.ButtonText(rect, "Toggle Colonist"))
            {
                UndeadMaster.ToggleALLAllowColonistBehaviour();
            }
            TooltipHandler.TipRegion(rect, "Toggle Allow Colonist Behaviour");
        }


        private void DrawAbsorbedCreaturesList(Pawn pawn, List<Pawn> absorbedCreatures, Listing_Standard listingStandard)
        {
            var groupedCreatures = absorbedCreatures
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count());

            foreach (var group in groupedCreatures)
            {
                DrawRow(pawn, group.Key, listingStandard);
            }
        }

        private void DrawRow(Pawn pawn, Pawn absorbedCreature, Listing_Standard listingStandard)
        {
            Rect rowRect = listingStandard.GetRect(ROW_HEIGHT);
            var layout = new RowLayoutManager(rowRect);
            Rect iconRect = layout.NextRect(ICON_SIZE, COLUMN_SPACING);
            Rect labelRect = layout.NextRect(LABEL_WIDTH, COLUMN_SPACING);
            Rect healthRect = layout.NextRect(HEALTH_WIDTH, COLUMN_SPACING);
            Rect summonButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);
            Rect removeButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);
            Rect calledToArmsRect = layout.NextRect(30f);
            Rect allowColonistBehaviourRect = layout.NextRect(30f);


            Hediff_Undead undead = absorbedCreature.health.hediffSet.GetFirstHediff<Hediff_Undead>();

            Widgets.DrawTextureFitted(iconRect, absorbedCreature.kindDef.race.uiIcon, 1f);
            Widgets.HyperlinkWithIcon(iconRect, new Dialog_InfoCard.Hyperlink(absorbedCreature.def));
            Widgets.Label(labelRect, $"{absorbedCreature.Label}");
            Widgets.FillableBar(healthRect, absorbedCreature.health.summaryHealth.SummaryHealthPercent);

            if (UndeadMaster.IsCreatureActive(absorbedCreature))
            {
                if (Widgets.ButtonText(summonButtonRect, "UnSummon"))
                {
                    UndeadMaster.UnsummonCreature(absorbedCreature);
                }
            }
            else
            {
                if (Widgets.ButtonText(summonButtonRect, "Summon"))
                {
                    UndeadMaster.SummonCreature(absorbedCreature, pawn.Position);
                }
            }

            if (undead != null)
            {

                TooltipHandler.TipRegion(calledToArmsRect, $"Call To Arms:\nWhen called to Arms, the unit follows in formation and defends their master, nothing else is considered.");
                TooltipHandler.TipRegion(allowColonistBehaviourRect, $"Allow NaturalBehaviour:\nAllow a Pawn to behave as it would as a regular pawn.");

                bool newInFormation = undead.CalledToArms;
                Widgets.CheckboxLabeled(calledToArmsRect, "", ref newInFormation);
                if (newInFormation != undead.CalledToArms)
                {
                    undead.CalledToArms = newInFormation;
                }


                bool newALlow = undead.AllowColonistBehaviour;
                Widgets.CheckboxLabeled(allowColonistBehaviourRect, "", ref newALlow);
                if (newInFormation != undead.AllowColonistBehaviour)
                {
                    undead.AllowColonistBehaviour = newInFormation;
                }
            }


            if (Widgets.ButtonText(removeButtonRect, "Remove"))
            {
                UndeadMaster.DeleteAbsorbedCreature(absorbedCreature);
            }
        }

    }
}
