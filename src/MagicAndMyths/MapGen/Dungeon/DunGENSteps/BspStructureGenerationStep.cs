namespace MagicAndMyths
{
    public class BspStructureGenerationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            var bspGenerator = new BspStructureGenerator(context);
            bspGenerator.Generate();
        }
    }
}
