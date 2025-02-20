using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class Hediff_UndeadMaster : HediffWithComps
    {
        private const int undeadLimit = 500;
        private List<Pawn> storedUndead = new List<Pawn>();
        private List<Pawn> activeUndead = new List<Pawn>();


        private float _FollowDistance = 5f;

        public float FollowDistance => _FollowDistance;

        public bool InFormation = true;

        private FormationUtils.FormationType _FormationType = FormationUtils.FormationType.Column;
        public FormationUtils.FormationType FormationType => _FormationType;
        public override string Description => base.Description + $"\r\nThis pawn has absorbed {storedUndead.Count} spirits.";


        public List<Pawn> GetStored()
        {
            return storedUndead;
        }
        public List<Pawn> GetActiveCreatures()
        {
            return new List<Pawn>(activeUndead);
        }


        public void SetFormation(FormationUtils.FormationType formationType)
        {
            _FormationType = formationType;
        }
        public void SetFollowDistance(float distance)
        {
            _FollowDistance = distance;
        }
        public bool IsCreatureActive(Pawn absorbedPawn)
        {
            return activeUndead.Any(p => p == absorbedPawn);
        }

        public int GetAbsorbedCreatureCount(Pawn Pawn)
        {
            return storedUndead.FindAll(x => x.kindDef == pawn.kindDef).Count;
        }

        public void RemoveSummon(Pawn pawn, bool removeStoredPawn = true)
        {
            if (IsCreatureActive(pawn))
            {
                UnsummonCreature(pawn);
            }

            if (removeStoredPawn)
            {
                storedUndead.Remove(pawn);
            }
        }

        public Pawn GetActiveSummonOfPawn(Pawn absorbedPawn)
        {
            return activeUndead.FirstOrDefault(p => p == absorbedPawn);
        }


        public bool UnSummonAll()
        {
            foreach (var creature in GetActiveCreatures())
            {
                if (!UnsummonCreature(creature))
                {
                    return false;
                }
            }

            return true;
        }

        public bool UnsummonCreature(Pawn pawn)
        {
            if (activeUndead.Remove(pawn))
            {
                if (pawn.Spawned && !pawn.Destroyed)
                {
                    pawn.DeSpawn();
                }

                return true;
            }
            return false;
        }

        public void ToggleALLCallToArms()
        {
            foreach (var creature in GetActiveCreatures())
            {
                if (creature.IsControlledSummon(out Hediff_Undead undead))
                {
                    undead.CalledToArms = !undead.CalledToArms;
                }
            }
        }
        public void ToggleALLAllowColonistBehaviour()
        {
            foreach (var creature in GetActiveCreatures())
            {
                if (creature.IsControlledSummon(out Hediff_Undead undead))
                {
                    undead.AllowColonistBehaviour = !undead.AllowColonistBehaviour;
                }
            }
        }

        public bool SummonCreatureInFormation(Pawn absorbedPawn)
        {
            if (!HasAbsorbedCreature(absorbedPawn))
            {
                return false;
            }

            IntVec3 position = FormationUtils.GetFormationPosition(
                FormationType,
                pawn.Position.ToVector3Shifted(),
                pawn.Rotation,
                storedUndead.IndexOf(absorbedPawn),
                storedUndead.Count);

            if (SummonCreature(absorbedPawn, position))
            {
                return true;
            }
            return false;
        }

        public bool SummonCreature(Pawn absorbedPawn, IntVec3 Position)
        {
            if (HasAbsorbedCreature(absorbedPawn))
            {
                Pawn summonedPawn = absorbedPawn;

                if (!summonedPawn.Spawned)
                {
                    GenSpawn.Spawn(summonedPawn, Position, pawn.Map);
                }


                if (summonedPawn.Faction != Faction.OfPlayer)
                {
                    summonedPawn.SetFaction(Faction.OfPlayer, this.pawn);
                }


                Hediff_Undead shikigami = (Hediff_Undead)summonedPawn.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_Undead);
                shikigami.SetMaster(pawn);
                if (summonedPawn.abilities == null)
                {
                    summonedPawn.abilities = new Pawn_AbilityTracker(summonedPawn);
                }

                activeUndead.Add(summonedPawn);
                //DraftingUtility.MakeDraftable(summonedPawn);
                return true;
            }
            return false;
        }

        public bool HasAbsorbedCreature(Pawn absorbedPawn)
        {
            return storedUndead.Contains(absorbedPawn);
        }

        public void DeleteAbsorbedCreature(Pawn absorbedPawn)
        {
            if (storedUndead.Remove(absorbedPawn))
            {
                Pawn activeSummon = GetActiveSummonOfPawn(absorbedPawn);
                if (activeSummon != null)
                {
                    UnsummonCreature(activeSummon);
                }
            }
        }

        public bool CanAbsorbNewCreature()
        {
            return storedUndead.Count < undeadLimit;
        }

        public void AbsorbCreature(Pawn targetPawn)
        {
            if (CanAbsorbNewCreature())
            {
                targetPawn.TryMakeUndeadSummon(this.pawn);
                storedUndead.Add(targetPawn);

                if (targetPawn.Spawned)
                {
                    targetPawn.DeSpawn(DestroyMode.Vanish);
                }
            }
        }


        public IntVec3 GetFormationPositionFor(Pawn Pawn)
        {
            if (activeUndead.Contains(Pawn))
            {
                return FormationUtils.GetFormationPosition(
                    FormationType,
                    pawn.Position.ToVector3(),
                    pawn.Rotation,
                    activeUndead.IndexOf(Pawn),
                    activeUndead.Count);
            }

            return IntVec3.Invalid;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Gizmo_FormationControl(this);
        }

        private void ReleaseUndead(Pawn undead)
        {
            if (!undead.Spawned)
            {
                GenSpawn.Spawn(undead, pawn.Position, this.pawn.MapHeld);
            }

            // Remove the Shikigami hediff
            Hediff shikigamiHediff = undead.health.hediffSet.GetFirstHediffOfDef(MagicAndMythDefOf.DeathKnight_Undead);
            if (shikigamiHediff != null)
            {
                undead.health.RemoveHediff(shikigamiHediff);
            }

            if (undead.Faction != Faction.OfPirates)
            {
                undead.SetFaction(Faction.OfPirates);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storedUndead, "storedCursedSpirits", LookMode.Deep);
            Scribe_Collections.Look(ref activeUndead, "activeCursedSpirits", LookMode.Reference);
        }
    }
}
