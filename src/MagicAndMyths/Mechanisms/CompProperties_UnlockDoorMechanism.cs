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
        private Building_LockableDoor LinkedDoor;
        public override void OnSolutionComplete()
        {
            if (LinkedDoor != null)
            {
                LinkedDoor.Unlock();
            }
        }

        public void SetLinkedDoor(Building_LockableDoor door)
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
