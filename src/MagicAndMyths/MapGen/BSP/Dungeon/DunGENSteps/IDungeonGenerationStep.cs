namespace MagicAndMyths
{
    public interface IDungeonGenerationStep
    {
        void Execute(DungeonGenerationContext context);
    }
}
