using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class VisualLightningStrike
    {
        private IntVec3 strikeLoc;
        private Mesh boltMesh;
        private float fadeOut = 1f;
        private static readonly Material LightningMat = MaterialPool.MatFrom("Weather/LightningBolt", ShaderDatabase.MoteGlow);

        public bool Expired => fadeOut <= 0f;

        public VisualLightningStrike(IntVec3 location)
        {
            strikeLoc = location;
            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
        }

        public void Draw()
        {
            if (!Expired)
            {
                Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather),
                    Quaternion.identity,
                    FadedMaterialPool.FadedVersionOf(LightningMat, fadeOut), 0);
                fadeOut -= 0.1f;
            }
        }
    }
}
