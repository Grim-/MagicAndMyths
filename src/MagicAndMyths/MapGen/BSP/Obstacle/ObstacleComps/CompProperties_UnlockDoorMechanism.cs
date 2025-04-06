using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_UnlockDoorMechanism : CompProperties
    {
        public CompProperties_UnlockDoorMechanism()
        {
            compClass = typeof(CompMechanism_UnlockLinkedDoor);
        }
    }

    public class CompMechanism_UnlockLinkedDoor : CompMechanism
    {

        private Building_Door LinkedDoor;
        public override void OnSolutionComplete()
        {
            if (LinkedDoor != null)
            {
                LinkedDoor.SetForbidden(false);
            }
        }

        public void SetLinkedDoor(Building_Door door)
        {
            LinkedDoor = door;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref LinkedDoor, "linkedDoor");
        }
    }
}
