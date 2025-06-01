using Verse;

namespace MagicAndMyths
{
    public class HediffWithStacks : HediffWithComps
    {
        protected int _CurrentStackLevel = 0;
        public int StackLevel => _CurrentStackLevel;


        public HediffStage GetStageForStackLevel(int Level)
        {
            if (def.stages == null)
            {
                return null;
            }

            if (Level > def.stages.Count)
            {
                return null;
            }

            return def.stages[Level];
        }

        public void AddStack(int stacksToAdd = 1)
        {
            _CurrentStackLevel += stacksToAdd;

            if (_CurrentStackLevel >= def.stages.Count)
            {
                _CurrentStackLevel = def.stages.Count;
            }
        }

        public void RemoveStack(int stacksToRemove = 1)
        {
            _CurrentStackLevel -= stacksToRemove;

            if (_CurrentStackLevel <= 0)
            {
                _CurrentStackLevel = 0;
            }
        }


        public void OnStackChange()
        {

        }

        public override HediffStage CurStage => GetStageForStackLevel(StackLevel);

    }


}
