using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class AbilityResourceDef : Def
    {
        public string resourceName = "unnamed resource";
        public StatDef maxStat;
        public int maxStatDefault = 10;
        public StatDef regenTicks;
        public int regenTicksDefault = 2500;
        public StatDef regenStat;
        public int regenStatDefault = 1;
        public StatDef regenSpeedStat;
        public float regenSpeedStatDefault = 1f;
        public StatDef costMult;
        public float costMultStatDefault = 1f;
        public Color barColor = Color.cyan;
        public Color barHighlightColor = Color.white;

        public Type gizmoClass = typeof(GeneGizmo_BasicResource);

        public float MaxStatValue(Pawn Pawn)
        {
            if (maxStat != null)
            {
                return Pawn.GetStatValue(maxStat, true, 1250);
            }
            return maxStatDefault;
        }

        public int RegenTicksValue(Pawn Pawn)
        {
            if (regenTicks != null)
            {
                return (int)Pawn.GetStatValue(regenTicks, true, 1250);
            }
            return regenTicksDefault;
        }

        public int RegenStatValue(Pawn Pawn)
        {
            if (regenStat != null)
            {
                return (int)Pawn.GetStatValue(regenStat, true, 1250);
            }
            return regenStatDefault;
        }

        public float RegenSpeedStatValue(Pawn Pawn)
        {
            if (regenSpeedStat != null)
            {
                return Pawn.GetStatValue(regenSpeedStat, true, 1250);
            }
            return regenSpeedStatDefault;
        }

        public float CostMultValue(Pawn Pawn)
        {
            if (costMult != null)
            {
                return Pawn.GetStatValue(costMult, true, 1250);
            }
            return costMultStatDefault;
        }
    }

    public class PawnExtraResources
    {
        public AbilityResourceDef resourceDef;
        public bool hideWhenEmpty = true;
    }
}
