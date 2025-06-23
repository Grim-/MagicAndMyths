using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class GameCondition_ManaSurge: GameCondition
    {
        public override void Init()
        {
            base.Init();
            Map map = (Map)this.SingleMap;
            MapComp_ManaManager manaManager = map.GetComponent<MapComp_ManaManager>();

            if (manaManager != null)
            {
                manaManager.SetManaFlowMultiplier(Rand.Range(0.3f, 3f));
            }
        }

        public override void End()
        {
            Map map = (Map)this.SingleMap;
            MapComp_ManaManager manaManager = map.GetComponent<MapComp_ManaManager>();

            if (manaManager != null)
            {
                manaManager.SetManaFlowMultiplier(1f);
            }
            base.End();
        }
    }
}

