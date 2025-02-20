using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public interface IDrawEquippedGizmos
    {
        IEnumerable<Gizmo> GetEquippedGizmos();
    }
}
