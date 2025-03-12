using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_VerbGiverWithGizmos : HediffCompProperties_VerbGiver
    {
        public HediffCompProperties_VerbGiverWithGizmos()
        {
            compClass = typeof(HediffComp_VerbGiverWithGizmos);
        }
    }

    public class HediffComp_VerbGiverWithGizmos : HediffComp_VerbGiver
    {

        public override void CompPostMake()
        {
            base.CompPostMake();

            foreach (var item in this.VerbTracker.AllVerbs)
            {
                item.verbTracker = this.Pawn.verbTracker;
                this.Pawn.VerbTracker.AllVerbs.Add(item);
                Log.Message("added verb");
            }
        }

        //public override IEnumerable<Gizmo> CompGetGizmos()
        //{
        //    //if (this.VerbTracker != null)
        //    //{
        //    //    foreach (var item in this.VerbTracker.AllVerbs)
        //    //    {
        //    //        yield return this.CreateVerbTargetCommand(this.Pawn, item);
        //    //    }
        //    //}
        //}

        private Command_VerbTarget CreateVerbTargetCommand(Thing ownerThing, Verb verb)
        {
            Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
            ThingStyleDef styleDef = ownerThing.StyleDef;
            command_VerbTarget.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description.CapitalizeFirst();
            command_VerbTarget.icon = ((styleDef != null && styleDef.UIIcon != null) ? styleDef.UIIcon : ownerThing.def.uiIcon);
            command_VerbTarget.iconAngle = ownerThing.def.uiIconAngle;
            command_VerbTarget.iconOffset = ownerThing.def.uiIconOffset;
            command_VerbTarget.tutorTag = "VerbTarget";
            command_VerbTarget.verb = verb;
            if (verb.caster.Faction != Faction.OfPlayer && !DebugSettings.ShowDevGizmos)
            {
                command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                string reason;
                if (verb.CasterPawn.RaceProps.IsMechanoid && !MechanitorUtility.EverControllable(verb.CasterPawn) && !DebugSettings.ShowDevGizmos)
                {
                    command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
                }
                else if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                {
                    command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (!verb.CasterPawn.Drafted && !DebugSettings.ShowDevGizmos)
                {
                    command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (verb is Verb_LaunchProjectile)
                {
                    Apparel apparel = verb.FirstApparelPreventingShooting();
                    if (apparel != null)
                    {
                        command_VerbTarget.Disable("ApparelPreventsShooting".Translate(verb.CasterPawn.Named("PAWN"), apparel.Named("APPAREL")).CapitalizeFirst());
                    }
                }
                else if (EquipmentUtility.RolePreventsFromUsing(verb.CasterPawn, verb.EquipmentSource, out reason))
                {
                    command_VerbTarget.Disable(reason);
                }
            }
            return command_VerbTarget;
        }
    }
}
