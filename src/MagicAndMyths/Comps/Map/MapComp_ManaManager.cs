using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MapComp_ManaManager : MapComponent
    {
        private float manaFlowMultiplier = 1.0f;
        private int lastUpdateTick = -1;



        protected bool _IsManaEnabled = true;
        public bool IsManaEnabled
        {
            get => _IsManaEnabled;
            set => _IsManaEnabled = value;
        }

        public float ManaFlowMultiplier
        {
            get { return this.manaFlowMultiplier; }
        }

        public MapComp_ManaManager(Map map) : base(map)
        {

        }


        public void SetManaActive(bool IsActive)
        {
            _IsManaEnabled = IsActive;
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                this.UpdateManaFlow();
            }
        }

        private void UpdateManaFlow()
        {
            this.lastUpdateTick = Find.TickManager.TicksGame;
        }

        public void SetManaFlowMultiplier(float multiplier)
        {
            this.manaFlowMultiplier = Mathf.Clamp(multiplier, 0.1f, 3.0f);
        }

        public void ModifyManaFlowMultiplier(float delta)
        {
            this.SetManaFlowMultiplier(this.manaFlowMultiplier + delta);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _IsManaEnabled, "IsManaDisabled");
            Scribe_Values.Look<float>(ref this.manaFlowMultiplier, "manaFlowMultiplier", 1.0f);
            Scribe_Values.Look<int>(ref this.lastUpdateTick, "lastUpdateTick", -1);
        }
    }
}

