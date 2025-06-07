namespace MagicAndMyths
{
    public class FriendlyFireSettings
    {
        public bool canTargetHostile = true;
        public bool canTargetFriendly = true;
        public bool canTargetNeutral = true;

        public static FriendlyFireSettings AllFriendly()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = true,
                canTargetHostile = false,
                canTargetNeutral = true
            };
        }

        public static FriendlyFireSettings FriendlyFactionOnly()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = true,
                canTargetHostile = false,
                canTargetNeutral = false
            };
        }

        public static FriendlyFireSettings HostileOnly()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = false,
                canTargetHostile = true,
                canTargetNeutral = false
            };
        }

        public static FriendlyFireSettings All()
        {
            return new FriendlyFireSettings()
            {
                canTargetFriendly = true,
                canTargetHostile = true,
                canTargetNeutral = true
            };
        }
    }
}