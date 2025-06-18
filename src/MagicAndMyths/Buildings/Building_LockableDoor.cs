using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public interface ILockableDoor
    {
        bool IsLocked { get; set; }
        Thing_Key KeyReference { get; set; }
        Color? PairingColor { get; set; }

        void SetKeyReference(Thing_Key keyThing, Color color);
        void Unlock();
        void Lock();
        bool PawnHasRequiredKey(Pawn pawn);
        bool TryFindAndConsumeKey(Pawn pawn);
    }

    public static class LockableDoorHelper
    {
        public static IEnumerable<FloatMenuOption> GetLockingFloatMenuOptions(ILockableDoor door, Pawn selPawn)
        {
            if (door.IsLocked)
            {
                if (door.PawnHasRequiredKey(selPawn))
                {
                    yield return new FloatMenuOption("Unlock", () =>
                    {
                        Job job = JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_UnlockDoor, door as Building);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                    });
                }
                else
                {
                    yield return new FloatMenuOption($"{DCUtility.FormatDCCheck(5, DCUtility.CalculateSkillBonus(selPawn, SkillDefOf.Crafting))}", () =>
                    {
                    });
                    yield return new FloatMenuOption($"Key Required {door.KeyReference}", () =>
                    {
                    }, MenuOptionPriority.DisabledOption);
                }
            }
        }

        public static string GetLockingInspectString(ILockableDoor door)
        {
            return $"Is Locked ? {door.IsLocked}";
        }

        public static void ExposeLockingData(ILockableDoor door)
        {
            bool isLocked = door.IsLocked;
            Thing_Key keyRef = door.KeyReference;

            Scribe_Values.Look(ref isLocked, "isLocked", false);
            Scribe_References.Look(ref keyRef, "keyReference");

            door.IsLocked = isLocked;
            door.KeyReference = keyRef;
        }
    }

    public class Building_LockableDoor : Building_Door, ILockableDoor
    {
        private bool _IsLocked = false;
        private Thing_Key keyReference = null;
        private Color? pairingColor;

        public bool IsLocked
        {
            get => _IsLocked;
            set => _IsLocked = value;
        }

        public Thing_Key KeyReference
        {
            get => keyReference;
            set => keyReference = value;
        }

        public Color? PairingColor
        {
            get => pairingColor;
            set => pairingColor = value;
        }

        public override Color DrawColor => pairingColor ?? base.DrawColor;

        public void SetKeyReference(Thing_Key keyThing, Color color)
        {
            keyReference = keyThing;
            pairingColor = color;
            keyThing.SetDoorReference(this, color);
        }

        public void Unlock()
        {
            _IsLocked = false;
        }

        public void Lock()
        {
            _IsLocked = true;
            if (this.Open)
            {
                this.DoorTryClose();
            }
        }

        public bool PawnHasRequiredKey(Pawn pawn)
        {
            return pawn.EquippedWornOrInventoryThings.Any(x => x == keyReference);
        }

        public bool TryFindAndConsumeKey(Pawn pawn)
        {
            if (PawnHasRequiredKey(pawn))
            {
                Thing keyInventory = pawn.inventory.innerContainer.Take(keyReference);
                if (keyInventory != null)
                {
                    if (keyInventory.Spawned)
                    {
                        keyInventory.DeSpawn();
                    }
                    Messages.Message($"{pawn.LabelCap} unlocked {this.LabelCap} using {keyInventory.LabelCap}!", MessageTypeDefOf.PositiveEvent);
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.GetFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            foreach (var option in LockableDoorHelper.GetLockingFloatMenuOptions(this, selPawn))
            {
                yield return option;
            }
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + LockableDoorHelper.GetLockingInspectString(this);
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return !_IsLocked && base.PawnCanOpen(p);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            LockableDoorHelper.ExposeLockingData(this);
        }
    }
    public class Building_LockableMultiTileDoor : Building_MultiTileDoor, ILockableDoor
    {
        private bool _IsLocked = false;
        private Thing_Key keyReference = null;
        private Color? pairingColor;

        public bool IsLocked
        {
            get => _IsLocked;
            set => _IsLocked = value;
        }

        public Thing_Key KeyReference
        {
            get => keyReference;
            set => keyReference = value;
        }

        public Color? PairingColor
        {
            get => pairingColor;
            set => pairingColor = value;
        }

        public override Color DrawColor => pairingColor ?? base.DrawColor;

        public void SetKeyReference(Thing_Key keyThing, Color color)
        {
            keyReference = keyThing;
            pairingColor = color;
            keyThing.SetDoorReference(this, color);
        }

        public void Unlock()
        {
            _IsLocked = false;
        }

        public void Lock()
        {
            _IsLocked = true;
            if (this.Open)
            {
                this.DoorTryClose();
            }
        }

        public bool PawnHasRequiredKey(Pawn pawn)
        {
            return pawn.EquippedWornOrInventoryThings.Any(x => x == keyReference);
        }

        public bool TryFindAndConsumeKey(Pawn pawn)
        {
            if (PawnHasRequiredKey(pawn))
            {
                Thing keyInventory = pawn.inventory.innerContainer.Take(keyReference);
                if (keyInventory != null)
                {
                    if (keyInventory.Spawned)
                    {
                        keyInventory.DeSpawn();
                    }
                    Messages.Message($"{pawn.LabelCap} unlocked {this.LabelCap} using {keyInventory.LabelCap}!", MessageTypeDefOf.PositiveEvent);
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.GetFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            foreach (var option in LockableDoorHelper.GetLockingFloatMenuOptions(this, selPawn))
            {
                yield return option;
            }
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + LockableDoorHelper.GetLockingInspectString(this);
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return !_IsLocked && base.PawnCanOpen(p);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            LockableDoorHelper.ExposeLockingData(this);
        }
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
