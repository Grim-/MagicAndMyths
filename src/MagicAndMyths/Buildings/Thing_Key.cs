using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Thing_Key : ThingWithComps
    {
        private Building doorReference = null;
        private Color? pairingColor;

        public override Color DrawColor => pairingColor != null ? pairingColor.Value : base.DrawColor;

        public override string Label
        {
            get
            {
                if (doorReference != null)
                {
                    return $"Key";
                }
                return base.Label;
            }
        }

        public void SetDoorReference(Building door, Color color)
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
