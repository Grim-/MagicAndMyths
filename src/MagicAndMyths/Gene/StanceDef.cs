using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class StanceDef : Def
    {
        public string iconPath;
        public Color iconColor = Color.white;
        public Color uiColor = Color.white;
        public SoundDef activationSound;
        public EffecterDef activationEffecter;
        public BasicResourceGeneDef resourceGeneType;
        public float activationCost = 5f;
        public float upkeepCost = 0f;
        public bool requiresResourceToMaintain = true;
        public int cooldownTicks = 0;
        public List<HediffDef> hediffsToApply = new List<HediffDef>();
        public List<AbilityDef> abilitiesToGain = new List<AbilityDef>();
        public List<StanceDef> exclusiveWithStances = new List<StanceDef>();


        public List<HediffDef> hediffsToRemoveOnExit = new List<HediffDef>();

        public bool IsExclusiveWith(StanceDef otherStance)
        {
            return exclusiveWithStances.Contains(otherStance);
        }
    }
}
