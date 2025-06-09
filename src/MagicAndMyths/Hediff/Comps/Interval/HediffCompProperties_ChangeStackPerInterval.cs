using UnityEngine;

namespace MagicAndMyths
{
    public class HediffCompProperties_ChangeStackPerInterval : HediffCompProperties_BaseInterval
    {
        public int stacksToRemove = 1;

        public HediffCompProperties_ChangeStackPerInterval()
        {
            compClass = typeof(HediffComp_ChangeStackPerInterval);
        }
    }

    public class HediffComp_ChangeStackPerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_ChangeStackPerInterval Props => (HediffCompProperties_ChangeStackPerInterval)props;
        protected override void OnInterval()
        {
            base.OnInterval();
            if (this.parent is IStackableHediff withStacks)
            {
                if (Props.stacksToRemove > 0)
                {
                    withStacks.AddStack(Props.stacksToRemove);
                }
                else if (Props.stacksToRemove < 0)
                {
                    withStacks.RemoveStack(Mathf.Abs(Props.stacksToRemove));
                }

            }
        }
    }
}