using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Key : Thing
    {
        private Building_LockableDoor doorReference = null;
        private Color? pairingColor;

        public override Color DrawColor => pairingColor != null ? pairingColor.Value : base.DrawColor;

        public override string Label
        {
            get
            {
                if (doorReference != null)
                {
                    return $"Key ({doorReference}";
                }
                return base.Label;
            }
        }

        public void SetDoorReference(Building_LockableDoor door, Color color)
        {
            doorReference = door;
            pairingColor = color;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref doorReference, "doorReference");
        }
    }
}
