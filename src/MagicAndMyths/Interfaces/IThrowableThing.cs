using Verse;

namespace MagicAndMyths
{
    public interface IThrowableThing
    {
        bool IsThrowableAtAll { get; }

        DamageDef ImpactDamageType { get; }

        /// <summary>
        /// Called when a valid throw is first executed.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="map"></param>
        /// <param name="throwingPawn"></param>
        void OnThrown(IntVec3 position, Map map, Pawn throwingPawn = null);


        /// <summary>
        /// Called just before the thrown thing is respawned into the map.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="map"></param>
        /// <param name="throwingPawn"></param>
        void OnBeforeRespawn(IntVec3 position, Map map, Pawn throwingPawn = null);

        /// <summary>
        /// Called after the thrown thing is respawned into the map.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="thing"></param>
        /// <param name="map"></param>
        /// <param name="throwingPawn"></param>
        void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn);
    }
}