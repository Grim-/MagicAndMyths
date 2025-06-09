using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class FriendlyFireSettings
    {
        public bool canTargetHostile = true;
        public bool canTargetFriendly = true;
        public bool canTargetNeutral = true;



        public bool CanTargetThing(Thing thing, Faction sourceFaction)
        {
            if (thing.Faction == null)
                return canTargetNeutral;

            if (thing.Faction == sourceFaction && canTargetFriendly)
                return true;

            if (canTargetHostile && thing.Faction.HostileTo(sourceFaction))
                return true;

            if (canTargetNeutral && !thing.Faction.HostileTo(sourceFaction) && thing.Faction != sourceFaction)
                return true;

            return false;
        }

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