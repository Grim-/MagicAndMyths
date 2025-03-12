using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class Gizmo_FormationControl : Gizmo
    {
        private ISquadLeader master;
        private static readonly Vector2 BaseSize = new Vector2(140f, 80f);
        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private const float ButtonGridWidth = 140f;

        private Vector2 scrollPosition = new Vector2(0, 0);


        public Gizmo_FormationControl(ISquadLeader master)
        {
            this.master = master;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return BaseSize.x + (master.ShowExtraOrders ? ButtonGridWidth : 0);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect baseRect = new Rect(topLeft.x, topLeft.y, BaseSize.x, BaseSize.y);
            GUI.DrawTexture(baseRect, Command.BGTex);


            Rect formationRect = new Rect(baseRect.x + 5f, baseRect.y + 5f, baseRect.width - 40f, 22f);
            if (Widgets.ButtonText(formationRect, "Type: " + master.FormationType.ToString()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (FormationUtils.FormationType formation in Enum.GetValues(typeof(FormationUtils.FormationType)))
                {
                    options.Add(new FloatMenuOption(
                        formation.ToString(),
                        delegate { master.SetFormation(formation); }
                    ));
                }

                options.Add(new FloatMenuOption("do chant", () =>
                {
                    master.SquadLord = new Lord();
                    master.SquadLord.loadID = Find.UniqueIDsManager.GetNextLordID();
                    master.SquadLord.faction = this.master.SquadLeader.Faction;
                    master.SquadLord.SetJob(new LordJob_HateChant());
                    master.SquadLord.AddPawns(master.SquadMembersPawns);

                }));
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (Mouse.IsOver(formationRect))
            {
                TooltipHandler.TipRegion(formationRect, "Change formation");
            }


            //at side of formation button
            Rect toggleRect = new Rect(formationRect.xMax + 4f, formationRect.y, 22f, 22f);
            if (Widgets.ButtonImage(toggleRect, master.ShowExtraOrders ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                master.ShowExtraOrders = !master.ShowExtraOrders;
            }

            if (Mouse.IsOver(toggleRect))
            {
                TooltipHandler.TipRegion(toggleRect, "Show Extra");
            }


            //Widgets.DrawLineHorizontal(baseRect.x, formationRect.yMax + 4, GetWidth(maxWidth) - 8f);

            //next row
            Rect standardOrderGrid = new Rect(baseRect.x, formationRect.yMax + 10f, ButtonGridWidth, 40f);
            DrawStandardOrderGrid(standardOrderGrid);


            if (master.ShowExtraOrders)
            {
                //grid to the far right
                Rect RightButtonGrid = new Rect(baseRect.max.x, topLeft.y, ButtonGridWidth, BaseSize.y);
                DrawGridButtons(RightButtonGrid);
            }


            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawStandardOrderGrid(Rect rect)
        {
            GridLayout gridLayout = new GridLayout(rect, 4, 1);
            Rect cellRect = gridLayout.GetCellRect(0, 0);

            GUI.DrawTexture(cellRect, Command.BGTex);
            if (Mouse.IsOver(cellRect))
            {
                TooltipHandler.TipRegion(cellRect, "Toggle In Formation");
            }
            if (Widgets.ButtonImage(cellRect, master.InFormation ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                master.SetInFormation(!master.InFormation);
            }
 
            GUI.DrawTexture(gridLayout.GetCellRect(1, 0), Command.BGTex);
            //Widgets.DrawBoxSolidWithOutline(gridLayout.GetCellRect(1, 0), Color.clear, Color.white);
            if (Widgets.ButtonImage(gridLayout.GetCellRect(1, 0), TexCommand.Draft, true, "Call To Arms"))
            {
                master.SetAllState(SquadMemberState.CalledToArms);
            }

            GUI.DrawTexture(gridLayout.GetCellRect(2, 0), Command.BGTex);
            // Widgets.DrawBoxSolidWithOutline(gridLayout.GetCellRect(2, 0), Color.clear, Color.white);
            if (Widgets.ButtonImage(gridLayout.GetCellRect(2, 0), TexCommand.HoldOpen, true, "At Ease"))
            {
                master.SetAllState(SquadMemberState.AtEase);
            }

            GUI.DrawTexture(gridLayout.GetCellRect(3, 0), Command.BGTex);
            //Widgets.DrawBoxSolidWithOutline(gridLayout.GetCellRect(3, 0), Color.clear, Color.white);
            if (Widgets.ButtonImage(gridLayout.GetCellRect(3, 0), TexCommand.ForbidOn, true, "Do Nothing"))
            {
                master.SetAllState(SquadMemberState.DoNothing);
            }

        }

        private void DrawGridButtons(Rect GridButtonRect)
        {
            GUI.DrawTexture(GridButtonRect, Command.BGTex);

            GridLayout gridLayout = new GridLayout(GridButtonRect, 3, 2);

            //Widgets.DrawBoxSolidWithOutline(gridLayout.GetCellRect(0, 0), Color.clear, Color.white);
            //if (Widgets.ButtonImage(gridLayout.GetCellRect(0, 0), TexCommand.Draft, true, "Call To Arms"))
            //{
            //    master.SetAllState(SquadMemberState.CalledToArms);
            //}

            //Widgets.DrawBoxSolidWithOutline(gridLayout.GetCellRect(1, 0), Color.clear, Color.white);
            //if (Widgets.ButtonImage(gridLayout.GetCellRect(1, 0), TexCommand.HoldOpen, true, "At Ease"))
            //{
            //    master.SetAllState(SquadMemberState.AtEase);
            //}

            //Widgets.DrawBoxSolidWithOutline(gridLayout.GetCellRect(2, 0), Color.clear, Color.white);
            //if (Widgets.ButtonImage(gridLayout.GetCellRect(2, 0), TexCommand.ForbidOn, true, "Do Nothing"))
            //{
            //    master.SetAllState(SquadMemberState.DoNothing);
            //}

            DrawSquadOrders(gridLayout);
        }

        LocalTargetInfo selectedTarget = null;
        //create a button for each SquadOrder, activate it on click
        private void DrawSquadOrders(GridLayout gridLayout)
        {
            int startX = 0;
            int startY = 0;

            foreach (var item in DefDatabase<SquadOrderDef>.AllDefsListForReading)
            {
                GUI.DrawTexture(gridLayout.GetCellRect(startX, startY), Command.BGTex);
                if (Widgets.ButtonImage(gridLayout.GetCellRect(startX, startY), item.Icon, true, item.defName))
                {
                    if (item.requiresTarget)
                    {
                        Find.Targeter.BeginTargeting(item.targetingParameters,
                            (LocalTargetInfo target) =>
                            {

                                selectedTarget = target;
                            }
                        );
                    }

                    foreach (var squadMember in master.SquadMembersPawns)
                    {
                        if (squadMember.IsPartOfSquad(out ISquadMember member))
                        {
                            SquadOrderWorker squadOrderWorker = item.CreateWorker(member);

                            if (squadOrderWorker.CanExecuteOrder(selectedTarget))
                            {
                                squadOrderWorker.ExecuteOrder(selectedTarget);
                            }

                        }
                    }
                }

                startX++;

                if (startX == 3)
                {
                    startX = 0;
                    startY++;
                }
            }
        }

        private void OrderUndeadToDefendPoint(LocalTargetInfo target)
        {
            if (target == null || master == null || master.SquadMembersPawns.NullOrEmpty())
            {
                return;
            }

            foreach (Pawn minion in master.SquadMembersPawns)
            {
                if (minion != null && minion.Spawned && !minion.Dead)
                {
                    if (minion.IsPartOfSquad(out ISquadMember squadMember))
                    {
                        squadMember.SetDefendPoint(target.Cell);
                    }
                }
            }
            // Display attack message
            Messages.Message("Ordered to Defend point" + target.Label, MessageTypeDefOf.NeutralEvent);
        }
        private void ClearOrderUndeadToDefendPoint()
        {
            if (master == null || master.SquadMembersPawns.NullOrEmpty())
            {
                return;
            }

            foreach (Pawn minion in master.SquadMembersPawns)
            {
                if (minion != null && minion.Spawned && !minion.Dead)
                {
                    if (minion.IsPartOfSquad(out ISquadMember squadMember))
                    {
                        squadMember.ClearDefendPoint();
                    }
                }
            }
            // Display attack message
            Messages.Message("Ordered to stop defending point", MessageTypeDefOf.NeutralEvent);
        }
        private void OrderUndeadToAttack(LocalTargetInfo target)
        {
            if (target == null || master == null || master.SquadMembersPawns.NullOrEmpty())
            {
                return;
            }

            foreach (Pawn minion in master.SquadMembersPawns)
            {
                if (minion != null && minion.Spawned && !minion.Dead)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                    job.playerForced = true;
                    job.killIncappedTarget = true;
                    minion.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }

            // Display attack message
            Messages.Message("Ordered to attack " + target.Label, MessageTypeDefOf.NeutralEvent);
        }
    }
}
