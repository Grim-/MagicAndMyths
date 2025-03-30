using RimWorld;
using RimWorld.Planet;

namespace MagicAndMyths
{
    public class BiomeWorker_NeverSpawn : BiomeWorker
    {
        public override float GetScore(Tile tile, int tileID)
        {
            return -100f;
        }
    }
}
