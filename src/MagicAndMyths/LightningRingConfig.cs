namespace MagicAndMyths
{
    public class LightningRingConfig
    {
        public int Strikes { get; set; }
        public float Radius { get; set; }

        public LightningRingConfig(int strikes, float radius)
        {
            Strikes = strikes;
            Radius = radius;
        }
    }
}
