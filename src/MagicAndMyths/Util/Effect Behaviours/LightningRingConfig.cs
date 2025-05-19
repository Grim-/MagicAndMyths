namespace MagicAndMyths
{
    public class LightningRingConfig
    {
        public int Strikes = 1;
        public float Radius = 1f;

        public LightningRingConfig()
        {

        }

        public LightningRingConfig(int strikes, float radius)
        {
            Strikes = strikes;
            Radius = radius;
        }
    }
}
