using Verse;

namespace MagicAndMyths
{
    /// <summary>
    /// Interface for anything that can act as a portal and teleport pawns
    /// </summary>
    public interface IPortalProvider
    {
        /// <summary>
        /// Whether the portal is currently open and functioning
        /// </summary>
        bool IsPortalActive { get; }

        /// <summary>
        /// Teleport a pawn through this portal
        /// </summary>
        /// <param name="pawn">The pawn to teleport</param>
        /// <returns>True if teleportation was successful, false otherwise</returns>
        bool TeleportPawn(Pawn pawn);
    }
}
