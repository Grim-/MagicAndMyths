using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class BspNode : IExposable
    {
        public CellRect rect;
        public BspNode left;
        public BspNode right;
        public List<string> tags = new List<string>();
        public List<BspNode> connectedNodes = new List<BspNode>();

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public void AddTag(string tag)
        {
            if (tags == null)
            {
                tags = new List<string>();
            }
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        public bool HasTag(string tag)
        {
            return tags != null && tags.Contains(tag);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref rect, "rect");
            Scribe_Deep.Look(ref left, "left");
            Scribe_Deep.Look(ref right, "right");
            Scribe_Collections.Look(ref tags, "tags", LookMode.Value);
            Scribe_Collections.Look(ref connectedNodes, "connectedNodes", LookMode.Reference);
        }
    }
}