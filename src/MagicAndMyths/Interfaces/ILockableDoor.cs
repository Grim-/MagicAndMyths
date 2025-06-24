using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public interface ILockableDoor
    {
        bool IsLocked { get; set; }
        Thing_Key KeyReference { get; set; }
        Color? PairingColor { get; set; }

        IntVec3 Position { get; set; }


        Thing Thing { get; }

        void SetKeyReference(Thing_Key keyThing, Color color);
        void Unlock();
        void Lock();
        bool PawnHasRequiredKey(Pawn pawn);
        bool TryFindAndConsumeKey(Pawn pawn);
    }


    //public class Building_LockableDoor : Building_Door
    //{
    //    private bool _IsLocked = false;
    //    private Thing_Key keyReference = null;


    //    private Color? pairingColor;

    //    public override Color DrawColor => pairingColor != null ? pairingColor.Value : base.DrawColor;

    //    public void SetKeyReference(Thing_Key keyThing, Color color)
    //    {
    //        keyReference = keyThing;
    //        pairingColor = color;
    //        keyReference.SetDoorReference(this, color);
    //    }

    //    public void Unlock()
    //    {
    //        _IsLocked = false;
    //    }

    //    public void Lock()
    //    {
    //        _IsLocked = true;

    //        if (this.Open)
    //        {
    //            this.DoorTryClose();
    //        }
    //    }

    //    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    //    {
    //        foreach (var item in base.GetFloatMenuOptions(selPawn))
    //        {
    //            yield return item;
    //        }

    //        if (_IsLocked)
    //        {
    //            if (PawnHasRequiredKey(selPawn))
    //            {
    //                yield return new FloatMenuOption("Unlock", () =>
    //                {
    //                    Job job = JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_UnlockDoor, this);
    //                    selPawn.jobs.StartJob(job, JobCondition.InterruptOptional);
    //                });
    //            }
    //            else
    //            {
    //                yield return new FloatMenuOption($"{DCUtility.FormatDCCheck(5, DCUtility.CalculateSkillBonus(selPawn, SkillDefOf.Crafting))}", () =>
    //                {

    //                });
    //                yield return new FloatMenuOption($"Key Required {keyReference}", () =>
    //                {

    //                }, MenuOptionPriority.DisabledOption);
    //            }
    //        }
    //    }

    //    private bool PawnHasRequiredKey(Pawn Pawn)
    //    {
    //        return Pawn.EquippedWornOrInventoryThings.Any(x => x == keyReference);
    //    }

    //    public bool TryFindAndConsumeKey(Pawn Pawn)
    //    {
    //        if (PawnHasRequiredKey(Pawn))
    //        {
    //            Thing keyInventory = Pawn.inventory.innerContainer.Take(keyReference);
    //            if (keyInventory != null)
    //            {
    //                if (keyInventory.Spawned)
    //                {
    //                    keyInventory.DeSpawn();
    //                }

    //                Messages.Message($"{Pawn.LabelCap} unlocked {this.LabelCap} using {keyInventory.LabelCap}!", MessageTypeDefOf.PositiveEvent);
    //                return true;
    //            }

    //        }
    //        return false;
    //    }

    //    public override string GetInspectString()
    //    {
    //        return base.GetInspectString() + $"Is Locked ? {_IsLocked}";
    //    }

    //    public override bool PawnCanOpen(Pawn p)
    //    {
    //        return !_IsLocked && base.PawnCanOpen(p);
    //    }

    //    public override void ExposeData()
    //    {
    //        base.ExposeData();

    //        Scribe_Values.Look(ref _IsLocked, "isLocked", false);
    //        Scribe_References.Look(ref keyReference, "keyReference");
    //    }
    //}
}
