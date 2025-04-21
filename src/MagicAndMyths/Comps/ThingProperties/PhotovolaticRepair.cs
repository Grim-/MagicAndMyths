namespace MagicAndMyths
{
    public class PhotovolaticRepair : PhotovolaticPropertyWorker
    {
        protected override void OnRechargeTick()
        {
            base.OnRechargeTick();

            if (parent != null && parent.Spawned)
            {
                if (CanRecharge())
                {
                    if (parent.def.useHitPoints)
                    {
                        parent.HitPoints += 2;
                    }
                }
            }
        }
        public override string GetDescription()
        {
            return "Regenerates a little every hour it is in the sun";
        }
    }
}
