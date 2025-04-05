using System;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class RoomConnectionRule
    {
        // Room type this rule applies to
        public RoomType roomType;

        // Maximum number of connections allowed
        public int maxConnections = int.MaxValue;

        // Minimum number of connections required
        public int minConnections = 1;

        // Priority for this rule (higher values are applied first)
        public int priority = 0;

        // Optional: Specific room types this room prefers to connect to
        public List<RoomType> preferredConnectionTypes;

        // Optional: Room types this room should avoid connecting to
        public List<RoomType> avoidedConnectionTypes;

        // Optional: Custom validation callback
        public Func<BspUtility.BspNode, RoomConnection, bool> customValidator;
    }
}
