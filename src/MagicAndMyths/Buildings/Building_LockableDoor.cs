using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class Building_LockableDoor : Building_Door
    {
        private bool _IsLocked = false;
        private Key keyReference = null;


        private Color? pairingColor;

        public override Color DrawColor => pairingColor != null ? pairingColor.Value : base.DrawColor;

        public void SetKeyReference(Key keyThing, Color color)
        {
            keyReference = keyThing;
            pairingColor = color;
            keyReference.SetDoorReference(this, color);
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

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.GetFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            if (_IsLocked)
            {
                if (PawnHasRequiredKey(selPawn))
                {
                    yield return new FloatMenuOption("Unlock", () =>
                    {
                        Job job = JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_UnlockDoor, this);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                    });
                }
                else
                {
                    yield return new FloatMenuOption($"Key Required {keyReference}", () =>
                    {

                    }, MenuOptionPriority.DisabledOption);
                }
            }
        }

        private bool PawnHasRequiredKey(Pawn Pawn)
        {
            return Pawn.EquippedWornOrInventoryThings.Any(x => x == keyReference);
        }

        public bool TryFindAndConsumeKey(Pawn Pawn)
        {
            if (PawnHasRequiredKey(Pawn))
            {
                Thing keyInventory = Pawn.inventory.innerContainer.Take(keyReference);
                if (keyInventory != null)
                {
                    if (keyInventory.Spawned)
                    {
                        keyInventory.DeSpawn();
                    }

                    Messages.Message($"{Pawn.LabelCap} unlocked {this.LabelCap} using {keyInventory.LabelCap}!", MessageTypeDefOf.PositiveEvent);
                    return true;
                }

            }
            return false;
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + $"Is Locked ? {_IsLocked}";
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return !_IsLocked && base.PawnCanOpen(p);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _IsLocked, "isLocked", false);
            Scribe_References.Look(ref keyReference, "keyReference");
        }
    }
}
