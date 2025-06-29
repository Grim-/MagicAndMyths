using System;
using Verse;

namespace MagicAndMyths
{
    public class NetworkCommandContext : IExposable
    {
        public virtual void ExposeData()
        {

        }
    }
    public class NetworkCommandDef : Def
    {
        public Type commandClass;

        public NetworkCommandWorker CreateCommand()
        {
            NetworkCommandWorker networkCommandWorker = (NetworkCommandWorker)Activator.CreateInstance(commandClass);
            networkCommandWorker.def = this;
            return networkCommandWorker;
        }
    }
}