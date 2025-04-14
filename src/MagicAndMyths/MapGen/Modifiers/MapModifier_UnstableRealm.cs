using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_UnstableRealm : MapModifier
    {
        private WorldComp_DungeonManager _dungeonManager;
        private WorldComp_DungeonManager DungeonManager
        {
            get
            {
                if (_dungeonManager == null)
                {
                    _dungeonManager = Find.World.GetComponent<WorldComp_DungeonManager>();
                }
                return _dungeonManager;
            }
        }

        private int TimeToLive = 60000;
        public override int MinTicksBetweenEffects => TimeToLive;
        public override int MaxTicksBetweenEffects => TimeToLive;


        protected override bool AppliesInstantly => false;

        public MapModifier_UnstableRealm(Map map, int timeToLive = 60000) : base(map)
        {
            TimeToLive = timeToLive;
        }

        public override void ApplyEffect()
        {
            if (DungeonManager != null)
            {
                DungeonManager.TryCloseMap(this.map);
            }
        }

        public override Texture2D GetModifierTexture()
        {
            return ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/FoodMeals", true);
        }

        public override string GetModifierExplanation()
        {
            return $"This realm is unstable! It will collapse, ejecting all non-dungeon denizens in {(TimeToLive - ticksUntilNext).ToStringSecondsFromTicks()}!";
        }
    }
}

