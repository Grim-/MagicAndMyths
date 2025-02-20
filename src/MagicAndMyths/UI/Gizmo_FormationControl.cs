//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using Verse;

//namespace MagicAndMyths
//{
//    [StaticConstructorOnStartup]
//    public class Gizmo_FormationControl : Gizmo
//    {
//        private Hediff_UndeadMaster master;
//        private static readonly Vector2 BaseSize = new Vector2(120f, 80f);
//        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

//        public Gizmo_FormationControl(Hediff_UndeadMaster master)
//        {
//            this.master = master;
//            Order = -100f;
//        }

//        public override float GetWidth(float maxWidth)
//        {
//            return BaseSize.x;
//        }

//        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
//        {
//            Rect baseRect = new Rect(topLeft.x, topLeft.y, BaseSize.x, BaseSize.y);
//            Widgets.DrawWindowBackground(baseRect);


//            Rect formationRect = new Rect(baseRect.x + 5f, baseRect.y + 5f, baseRect.width - 10f, 22f);
//            if (Widgets.ButtonText(formationRect, "Type: " + master.FormationType.ToString()))
//            {
//                List<FloatMenuOption> options = new List<FloatMenuOption>();
//                foreach (FormationUtils.FormationType formation in Enum.GetValues(typeof(FormationUtils.FormationType)))
//                {
//                    options.Add(new FloatMenuOption(
//                        formation.ToString(),
//                        delegate { master.SetFormation(formation); }
//                    ));
//                }
//                Find.WindowStack.Add(new FloatMenu(options));
//            }


//            Rect toggleRect = new Rect(baseRect.x + 5f, baseRect.y + 30f, baseRect.width - 10f, 22f);
//            bool newInFormation = master.InFormation;
//            Widgets.CheckboxLabeled(toggleRect, "Formation", ref newInFormation);
//            if (newInFormation != master.InFormation)
//            {
//                master.InFormation = newInFormation;
//            }


//            Rect sliderRect = new Rect(baseRect.x + 5f, baseRect.y + 55f, baseRect.width - 10f, 22f);
//            float newDistance = master.FollowDistance;
//            newDistance = Widgets.HorizontalSlider(
//                sliderRect,
//                master.FollowDistance,
//                1f, 15f,
//                label: newDistance.ToString("F1")
//            );
//            if (newDistance != master.FollowDistance)
//            {
//                master.SetFollowDistance(newDistance);
//            }

//            if (Mouse.IsOver(baseRect))
//            {
//                TooltipHandler.TipRegion(baseRect, "Configure undead formation settings");
//            }

//            return new GizmoResult(GizmoState.Clear);
//        }
//    }
//}
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class Gizmo_FormationControl : Gizmo
    {
        private Hediff_UndeadMaster master;
        private static readonly Vector2 BaseSize = new Vector2(120f, 80f);
        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private const float ButtonGridWidth = 80f;
        public Gizmo_FormationControl(Hediff_UndeadMaster master)
        {
            this.master = master;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return BaseSize.x + ButtonGridWidth;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect baseRect = new Rect(topLeft.x, topLeft.y, BaseSize.x, BaseSize.y);
            Widgets.DrawWindowBackground(baseRect);
            Rect formationRect = new Rect(baseRect.x + 5f, baseRect.y + 5f, baseRect.width - 10f, 22f);
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
                Find.WindowStack.Add(new FloatMenu(options));
            }


            Rect toggleRect = new Rect(baseRect.x + 5f, baseRect.y + 30f, baseRect.width - 10f, 22f);
            bool newInFormation = master.InFormation;
            Widgets.CheckboxLabeled(toggleRect, "Formation", ref newInFormation);
            if (newInFormation != master.InFormation)
            {
                master.InFormation = newInFormation;
            }


            Rect sliderRect = new Rect(baseRect.x + 5f, baseRect.y + 55f, baseRect.width - 10f, 22f);
            float newDistance = master.FollowDistance;
            newDistance = Widgets.HorizontalSlider(
                sliderRect,
                master.FollowDistance,
                1f, 15f,
                label: newDistance.ToString("F1")
            );
            if (newDistance != master.FollowDistance)
            {
                master.SetFollowDistance(newDistance);
            }

            if (Mouse.IsOver(baseRect))
            {
                TooltipHandler.TipRegion(baseRect, "Configure undead formation settings");
            }



            Rect RightButtonGrid = new Rect(baseRect.max.x, topLeft.y, ButtonGridWidth, BaseSize.y);
            DrawGridButtons(RightButtonGrid);

            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawGridButtons(Rect GridButtonRect)
        {
            Widgets.DrawWindowBackground(GridButtonRect);
            Widgets.DrawBoxSolidWithOutline(GridButtonRect, Color.clear, Color.white);

            if (Widgets.ButtonImage(GridButtonRect, TexButton.Banish, Color.white, Color.white * 0.85f))
            {
                Find.Targeter.BeginTargeting(new TargetingParameters
                {
                    canTargetPawns = true,
                    canTargetBuildings = true,
                    canTargetAnimals = true,
                    mapObjectTargetsMustBeAutoAttackable = true
                },
                (LocalTargetInfo target) =>
                {
                    OrderUndeadToAttack(target);
                }
                );
            }
        }
        private void OrderUndeadToAttack(LocalTargetInfo target)
        {
            if (target == null || master == null || master.GetActiveCreatures().NullOrEmpty())
            {
                return;
            }

            foreach (Pawn minion in master.GetActiveCreatures())
            {
                if (minion != null && minion.Spawned && !minion.Dead)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                    job.playerForced = true;
                    minion.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }

            // Display attack message
            Messages.Message("Undead ordered to attack " + target.Label, MessageTypeDefOf.NeutralEvent);
        }
    }
}
