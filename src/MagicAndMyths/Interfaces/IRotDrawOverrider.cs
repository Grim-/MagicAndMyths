using Verse;

namespace MagicAndMyths
{
    public interface IRotDrawOverrider
    {
        bool ShouldOverride { get; }
        RotDrawMode OverridenRotDrawMode { get; }
    }
}
