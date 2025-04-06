using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class Building_LockableDoor : Building_Door
    {
        private bool _IsLocked = false;
        private Key keyReference = null;


        public void SetKeyReference(Key keyThing)
        {
            keyReference = keyThing;
            keyReference.SetDoorReference(this);
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
                        if (TryFindAndConsumeKey(selPawn))
                        {
                            this.Unlock();
                        }
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

        private bool TryFindAndConsumeKey(Pawn Pawn)
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

        new public bool CanPhysicallyPass(Pawn p)
        {
            if (_IsLocked && !PawnCanOpen(p))
            {
                return false;
            }

            return base.CanPhysicallyPass(p);
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
