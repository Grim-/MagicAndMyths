using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class GameCondition_ManaPurge : GameCondition
    {
        public override float SkyGazeChanceFactor(Map map)
        {
            return base.SkyGazeChanceFactor(map) * 0.5f;
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            //SkyTarget? skyTarget =  base.SkyTarget(map);


            //if (skyTarget.HasValue)
            //{
            //    SkyTarget target = skyTarget.Value;
            //    target.colors.sky = Color.cyan;
            //}

            return new Verse.SkyTarget(1, new SkyColorSet(Color.cyan, Color.white, Color.clear, 2), 1f, 1f);
        }

        public override void Init()
        {
            base.Init();
            Map map = (Map)this.SingleMap;
            MapComp_ManaManager manaManager = map.GetComponent<MapComp_ManaManager>();

            if (manaManager != null)
            {
                manaManager.SetManaActive(false);
            }
        }

        public override void End()
        {
            Map map = (Map)this.SingleMap;
            MapComp_ManaManager manaManager = map.GetComponent<MapComp_ManaManager>();

            if (manaManager != null)
            {
                manaManager.SetManaActive(true);
            }
            base.End();
        }
    }
}

