using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class RimNet : IExposable, ILoadReferenceable
    {
        public string ID;
        public List<Comp_NetworkNode> NetworkNodes = new List<Comp_NetworkNode>();
        public Action<NetworkMessage> OnMessageSent;

        protected Queue<NetworkMessage> MessageQueue = new Queue<NetworkMessage>();

        public RimNet()
        {
        }

        public RimNet(string iD)
        {
            ID = iD;
            NetworkNodes = new List<Comp_NetworkNode>();
        }

        public void RegisterNode(Comp_NetworkNode node)
        {
            if (node?.parent?.Map == null)
                return;
            if (!NetworkNodes.Contains(node))
            {
                NetworkNodes.Add(node);
                node.JoinNetwork(this);
            }
        }

        public void UnregisterNode(Comp_NetworkNode node)
        {
            if (node?.parent?.Map == null)
                return;
            if (NetworkNodes.Contains(node))
            {
                NetworkNodes.Remove(node);
                node.LeaveNetwork();
            }
        }

        public void BroadcastMessage(Comp_NetworkNode sender, NetworkMessage message)
        {
            if (sender?.parent?.Map == null)
                return;

            message.TargetNodeID = null;
            MessageQueue.Enqueue(message);
            OnMessageSent?.Invoke(message);
        }

        public void SendMessage(Comp_NetworkNode sender, string targetNodeID, NetworkMessage message)
        {
            if (sender?.parent?.Map == null)
                return;

            message.TargetNodeID = targetNodeID;
            MessageQueue.Enqueue(message);
            OnMessageSent?.Invoke(message);
        }

        public void ProcessMessages()
        {
            if (MessageQueue.Count == 0)
                return;

            NetworkMessage message = MessageQueue.Dequeue();

            if (message == null)
                return;

            if (string.IsNullOrEmpty(message.TargetNodeID))
            {
                foreach (var node in NetworkNodes)
                {
                    if (node.NodeID != message.SenderId)
                    {
                        DeliverMessage(node, message);
                    }
                }
            }
            else
            {
                Comp_NetworkNode targetNode = GetConnectedNode(message.TargetNodeID);
                if (targetNode != null)
                {
                    DeliverMessage(targetNode, message);
                }
            }
        }

        private void DeliverMessage(Comp_NetworkNode targetNode, NetworkMessage message)
        {
            if (!targetNode.CanReceive() || message.SenderId == targetNode.GetUniqueLoadID() || message.NetworkId != ID)
                return;

            try
            {
                if (message.commandWorker == null && message.commandDef != null)
                {
                    message.commandWorker = message.commandDef.CreateCommand();
                }

                if (message.commandWorker != null)
                {
                    message.commandWorker.ExecuteCommand(targetNode, message.Context);
                    targetNode.OnMessageReceived(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public bool IsConnectedNode(string nodeID)
        {
            return NetworkNodes.Any(x => x.NodeID == nodeID);
        }

        public Comp_NetworkNode GetConnectedNode(string nodeID)
        {
            if (NetworkNodes.Any(x => x.NodeID == nodeID))
            {
                return NetworkNodes.FirstOrDefault(x => x.NodeID == nodeID);
            }
            return null;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref ID, "id");
            Scribe_Collections.Look(ref NetworkNodes, "networkNodes", LookMode.Reference);
            Scribe_Collections.Look(ref MessageQueue, "messageQueue", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (NetworkNodes == null)
                    NetworkNodes = new List<Comp_NetworkNode>();
                if (MessageQueue == null)
                    MessageQueue = new Queue<NetworkMessage>();
            }
        }

        public string GetUniqueLoadID()
        {
            return "RimNetwork_" + Find.UniqueIDsManager.GetNextMessageID();
        }
    }
}