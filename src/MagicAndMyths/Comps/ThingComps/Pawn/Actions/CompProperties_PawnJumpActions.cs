using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class CompProperties_PawnJumpActions : CompProperties
    {
        public float jumpMaxRange = 8f;
        public SoundDef jumpSound;

        public CompProperties_PawnJumpActions()
        {
            compClass = typeof(Comp_PawnJumpActions);
        }
    }

    [StaticConstructorOnStartup]
    public class Comp_PawnJumpActions : Comp_PawnActionBase
    {
        private static Texture2D defaultIcon = ContentFinder<Texture2D>.Get("UI/UIJump");
        private Pawn Pawn => parent as Pawn;
        public CompProperties_PawnJumpActions Props => (CompProperties_PawnJumpActions)props;

        protected int LastJumpAttemptTick = -1;
        protected TargetingParameters JumpTargetLocationParams = new TargetingParameters
        {
            canTargetLocations = true,
            canTargetHumans = false,
            canTargetAnimals = false,
            canTargetCorpses = false,
            canTargetFires = false,
            canTargetItems = false,
            canTargetMechs = false,
            canTargetMutants = false,
            canTargetBloodfeeders = false,
            canTargetSelf = false,
            canTargetPlants = false,
            canTargetPawns = false,
            canTargetBuildings = false,
        };

        public int JumpRangeCells
        {
            get => Mathf.Max(1, DCUtility.GetStatBonus(Pawn, MagicAndMythDefOf.Stat_Strength)) * 2;
        }

        public override bool CanPerformAction(Pawn pawn)
        {
            return pawn.health.hediffSet.GetNotMissingParts().Any(limb => limb.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore)) && base.CanPerformAction(pawn);
        }

        private bool CanJumpToLocation(IntVec3 targetCell)
        {
            if (Pawn == null)
                return false;

            if (!CanPerformAction(Pawn))
                return false;

            if (!targetCell.Walkable(Pawn.Map) || targetCell.Fogged(Pawn.Map))
                return false;

            if (!targetCell.InHorDistOf(Pawn.Position, JumpRangeCells))
                return false;

            return true;
        }

        private void BeginJumpToLocation()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetPawns = false,
                validator = (x) => CanJumpToLocation(x.Cell)
            },
            delegate (LocalTargetInfo target)
            {
                IntVec3 spawnPosition = Pawn.Position;
                IntVec3 tagetPosition = target.Cell;
                Map map = Pawn.Map;

                PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_SimpleFlyer, Pawn, tagetPosition, null, null);
                GenSpawn.Spawn(pawnFlyer, spawnPosition, map);
                LastJumpAttemptTick = Current.Game.tickManager.TicksGame;
            });
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Pawn == null || !Pawn.Spawned || !Pawn.IsColonistPlayerControlled)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = $"Jump up to {JumpRangeCells} cells",
                defaultDesc = "Jump to a target location.",
                icon = defaultIcon,
                action = BeginJumpToLocation,
                Disabled = !CanPerformAction(Pawn),
                disabledReason = !CanPerformAction(Pawn) ? "This pawn can't jump" : ""
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref LastJumpAttemptTick, "LastJumpAttemptTick", -1);
        }
    }
}