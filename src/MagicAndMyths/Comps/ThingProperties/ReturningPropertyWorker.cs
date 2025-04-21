using Verse;

namespace MagicAndMyths
{
    public class ReturningPropertyWorker : ThingPropertyWorker
    {
        public override void OnThingThrown(Thing target, IntVec3 position)
        {
            base.OnThingThrown(target, position);

            if (target != null && target is Pawn pawn && pawn.equipment != null && pawn.equipment.Primary == null)
            {
                pawn.equipment.AddEquipment(this.parent);
            }
        }

        public override string GetDescription()
        {
            return "This equipment will return to its thrower when thrown.";
        }
    }
}
