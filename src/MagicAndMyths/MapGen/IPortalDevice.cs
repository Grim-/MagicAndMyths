using UnityEngine;

namespace MagicAndMyths
{
    public interface IPortalDevice
    {
        bool CanTeleport { get; }
        string ActionLabel { get; }
        string ActionDescription { get; }
        string ModeTooltip { get; }
        Color ModeTooltipColor { get; }
    }
}
