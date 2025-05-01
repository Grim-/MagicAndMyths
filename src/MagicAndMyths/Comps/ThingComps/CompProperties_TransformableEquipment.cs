using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TransformableEquipment : CompProperties
    {
        public ThingDef transformsInto;
        public string gizmoLabel = "Transform";

        public CompProperties_TransformableEquipment()
        {
            compClass = typeof(CompTransformableEquipment);
        }
    }

    public class CompTransformableEquipment : ThingComp
    {
        private ThingDef originalDef;

        public CompProperties_TransformableEquipment Props => (CompProperties_TransformableEquipment)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
                originalDef = parent.def;
        }

        private Gizmo GetTransformGizmo()
        {
            return new Command_Action
            {
                defaultLabel = Props.gizmoLabel,
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Transform"),
                action = TransformEquipment
            };
        }

        private void TransformEquipment()
        {

        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref originalDef, "originalDef");
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
                yield return gizmo;

            yield return GetTransformGizmo();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            yield return GetTransformGizmo();
        }
    }
}