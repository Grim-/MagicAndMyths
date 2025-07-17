using RimWorld;
using RimWorld.Planet;

namespace MagicAndMyths
{
    public class BiomeWorker_NeverSpawn : BiomeWorker
    {
        public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
        {
            return -100f;
        }
    }
}
