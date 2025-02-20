using Verse;

namespace MagicAndMyths
{
    public static class DraftingUtility
    {
        public static WorldComponent_DraftableCreatures DraftManager
        {
            get
            {
                if (Current.Game != null && Current.Game.World != null)
                {
                    return Current.Game.World.GetComponent<WorldComponent_DraftableCreatures>();
                }

                return null;
            }
        }


        public static void RegisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.RegisterDraftableCreature(pawn);
            }
        }

        public static void UnregisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.UnregisterDraftableCreature(pawn);
            }
        }

        public static bool IsDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                return DraftManager.IsDraftableCreature(pawn);
            }

            return false;
        }

        public static void MakeDraftable(this Pawn pawn)
        {
            RegisterDraftableCreature(pawn);
        }
    }
}
