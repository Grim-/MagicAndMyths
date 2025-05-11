using RimWorld;
using SquadBehaviour;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Hediff_Undead : HediffWithComps, IRotDrawOverrider
    {
        UndeadHediffDef Def => (UndeadHediffDef)def;

        public Pawn Master => this.pawn;
        public override string Label => base.Label;
        public override string Description => base.Description + $"\nSquad Leader: {SquadLeader.SquadLeaderPawn.Name}";

        public Comp_PawnSquadLeader SquadLeader
        {
            get
            {

                if (Master != null && Master.TryGetComp(out Comp_PawnSquadLeader pawnSquadLeader))
                {
                    return pawnSquadLeader;
                }

                return null;
            }
        }

        private bool ShouldOverrideRotDraw = false;
        public bool ShouldOverride => ShouldOverrideRotDraw;

        private RotDrawMode _OverridenRotDrawMode = RotDrawMode.Dessicated;
        public RotDrawMode OverridenRotDrawMode => _OverridenRotDrawMode;


        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            ApplyVisual();
        }


        private void ApplyVisual()
        {
            if (this.pawn.story != null)
            {
               // this.pawn.story.skinColorOverride = Color.black;
                if (this.pawn.RaceProps.Humanlike)
                {
                    this.pawn.story.HairColor = new Color(0.85f, 0.85f, 0.85f);
                }       
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.pawn != null && !this.pawn.Dead && !this.pawn.Destroyed && this.Def != null && this.pawn.IsHashIntervalTick(Def.regenTicks))
            {
                //this.pawn.QuickHeal(Def.baseHealAmount);
                HandleNeeds();

                if (this.Master == null)
                {
                    Log.Message("Undead master is null removing");

                    this.Severity = 0;
                }
            }
        }


        public void TurnFeral()
        {

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (this.SquadLeader != null)
            {
                this.SquadLeader.RemoveFromSquad(this.pawn, true);
            }
            base.Notify_PawnDied(dinfo, culprit);
        }

        private void HandleNeeds()
        {
            foreach (var item in this.pawn.needs.AllNeeds)
            {
                item.CurLevel = item.MaxLevel;
            }

            if (this.pawn.IsSlave)
            {
                Need_Suppression suppression = this.pawn.needs.TryGetNeed<Need_Suppression>();
                if (suppression != null)
                {
                    suppression.CurLevel = suppression.MaxLevel;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_References.Look(ref referencedPawn, "referencedPawn");
            //Scribe_Values.Look(ref squadMemberState, "squadMemberState");
        }
    }
}
