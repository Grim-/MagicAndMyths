using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class SquadOrderDef : Def
    {
        public string uiIconPath = "";
        private Texture2D _Icon = null;
        public Texture2D Icon
        {
            get
            {
                if (_Icon == null)
                {
                    string path = String.IsNullOrEmpty(uiIconPath) ? "UI/Designators/Cancel" : uiIconPath;

                    _Icon = ContentFinder<Texture2D>.Get(path);
                }

                return _Icon;
            }
        }

        public bool requiresTarget = true;
        public TargetingParameters targetingParameters;
        public Type workerClass;

        public SquadOrderWorker CreateWorker(ISquadMember SquadMember)
        {
            SquadOrderWorker SquadOrderWorker = (SquadOrderWorker)Activator.CreateInstance(workerClass);
            SquadOrderWorker.SquadMember = SquadMember;
            SquadOrderWorker.SquadLeader = SquadMember.SquadLeader;
            return SquadOrderWorker;
        }
    }
}
