using System;
using Verse;

namespace MagicAndMyths
{
    public class NetworkMessage : IExposable
    {
        public string MessageId = string.Empty;
        public string SenderId = string.Empty;
        public string NetworkId;
        public string TargetNodeID;
        public NetworkCommandDef commandDef;
        public NetworkCommandWorker commandWorker;
        public NetworkCommandContext Context;
        public int TTL;

        public NetworkMessage()
        {
        }

        public NetworkMessage(string networkId, NetworkCommandDef def, NetworkCommandContext commandContext)
        {
            MessageId = Guid.NewGuid().ToString();
            NetworkId = networkId;
            commandDef = def;
            Context = commandContext;
            TTL = 5;
        }

        public NetworkMessage(string senderId, string networkId, NetworkCommandDef def, NetworkCommandContext commandContext)
        {
            MessageId = Guid.NewGuid().ToString();
            SenderId = senderId;
            NetworkId = networkId;
            commandDef = def;
            Context = commandContext;
            TTL = 5;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref MessageId, "messageId");
            Scribe_Values.Look(ref SenderId, "senderId");
            Scribe_Values.Look(ref NetworkId, "networkId");
            Scribe_Values.Look(ref TargetNodeID, "targetNodeId");
            Scribe_Values.Look(ref TTL, "ttl");
            Scribe_Defs.Look(ref commandDef, "commandDef");
            Scribe_Deep.Look(ref Context, "context");

            if (Scribe.mode == LoadSaveMode.LoadingVars && commandDef != null)
            {
                commandWorker = commandDef.CreateCommand();
            }

            if (commandWorker != null)
            {
                commandWorker.ExposeData();
            }
        }
    }
}