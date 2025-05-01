using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class ComboReaction
    {
        public List<DamageDef> reactionDamageType;
        public List<HediffDef> reactionHediff;
        public string reactionDescription = "Dont leave me empty";
        public Type workerClass;
        public ComboReactionProperties reactionProperties;
        public bool removeOnReact = true;
        public EffecterDef reactionEffecter;

        public ComboReactionWorker ExecuteWorker(Pawn pawn, HediffComp_ComboReactor parent)
        {
            ComboReactionWorker worker = (ComboReactionWorker)Activator.CreateInstance(workerClass);
            worker.Def = this;
            worker.Comp = parent;
            worker.pawn = pawn;
            worker.DoReaction(pawn);
            return worker;
        }
    }

    public class ComboReactionProperties
    {
        public DamageDef damageDef;
        public FloatRange damageRange;
        public FloatRange armourpenRange;

        public bool isAOE = false;
        public float radius = 5;
        public bool canTargetHostile = true;
        public bool canTargetFriendly = false;
        public bool canTargetNeutral = false;
        public FloatRange severityRange;
    }
}