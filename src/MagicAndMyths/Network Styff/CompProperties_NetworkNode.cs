using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_NetworkNode : CompProperties
    {
        public float defaultRange = 50f;

        public CompProperties_NetworkNode()
        {
            compClass = typeof(Comp_NetworkNode);
        }
    }

    //allows a thing to be networkable
    public class Comp_NetworkNode : ThingComp, ILoadReferenceable
    {
        public CompProperties_NetworkNode Props => (CompProperties_NetworkNode)props;
        private float range = 50f;
        protected MapComp_NetworkManager NetworkRouter => MapComp_NetworkManager.GetNetworkManager(this.parent.Map);

        public RimNet ConnectedNetwork = null;

        public ThingWithComps ParentThing => parent;

        public float Range
        {
            get => range;
            set => range = value;
        }

        private string nodeID;

        public string NodeID
        {
            get
            {
                if (string.IsNullOrEmpty(nodeID))
                    nodeID = "NetworkNode_" + Find.UniqueIDsManager.GetNextThingID();
                return nodeID;
            }
        }

        public bool IsConnectedToAnyNetwork => ConnectedNetwork != null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            NetworkRouter.RegisterNode(this);
        }

        public override void PostDeSpawn(Map previousMap)
        {
            base.PostDeSpawn(previousMap);
            NetworkRouter.UnregisterNode(this);
        }

        public void JoinNetwork(RimNet network)
        {
            ConnectedNetwork = network;
        }

        public void LeaveNetwork()
        {
            ConnectedNetwork = null;
        }

        public void SendMessage(string targetNodeID, NetworkCommandDef data, NetworkCommandContext commandContext)
        {
            if (!CanTransmit())
                return;

            var message = new NetworkMessage(NodeID, ConnectedNetwork.ID, data, commandContext);
            NetworkRouter.SendMessage(this, targetNodeID, message);
        }

        public bool CanTransmit()
        {
            var power = parent.TryGetComp<CompPowerTrader>();
            return (power == null || power.PowerOn) && ConnectedNetwork != null;
        }

        public bool CanReceive()
        {
            var power = parent.TryGetComp<CompPowerTrader>();
            return (power == null || power.PowerOn) && ConnectedNetwork != null;
        }

        public void OnMessageReceived(NetworkMessage message)
        {
            MoteMaker.ThrowText(this.parent.DrawPos, this.parent.Map, $"Received network message {message}");
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + $"Network status : {(IsConnectedToAnyNetwork ? "Connected" : "Disconnected")}";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref ConnectedNetwork, "connectedNetwork");
            Scribe_Values.Look(ref range, "networkRange", 50f);
        }

        public string GetUniqueLoadID()
        {
            return NodeID;
        }
    }


}