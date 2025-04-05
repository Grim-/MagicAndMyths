using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Building_HiddenDoor : Building_Door
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            this.DoorPreDraw();
            float offsetDist = 0f + 0.45f * this.OpenPct;
            if (this.Rotation.IsHorizontal)
            {
                DrawMoversVertical(drawLoc, offsetDist, this.Graphic, AltitudeLayer.DoorMoveable.AltitudeFor(), Vector3.one, this.Graphic.ShadowGraphic);
            }
            else
            {
                DrawMoversHorizontal(drawLoc, offsetDist, this.Graphic, AltitudeLayer.DoorMoveable.AltitudeFor(), Vector3.one, this.Graphic.ShadowGraphic);
            }
        }
        protected void DrawMoversHorizontal(Vector3 drawPos, float offsetDist, Graphic graphic, float altitude, Vector3 drawScaleFactor, Graphic_Shadow shadowGraphic)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 vector;
                Mesh mesh;
                if (i == 0)
                {
                    vector = new Vector3(-offsetDist, 0f, 0f);
                    mesh = MeshPool.plane10;
                }
                else
                {
                    vector = new Vector3(offsetDist, 0f, 0f);
                    mesh = MeshPool.plane10Flip;
                }

                Vector3 vector2 = drawPos;
                vector2.y = altitude;
                vector2 += vector;
                Graphics.DrawMesh(mesh, Matrix4x4.TRS(vector2, base.Rotation.AsQuat, new Vector3((float)this.def.size.x * drawScaleFactor.x, drawScaleFactor.y, (float)this.def.size.z * drawScaleFactor.z)), graphic.MatAt(base.Rotation, this), 0);
                if (shadowGraphic != null)
                {
                    shadowGraphic.DrawWorker(vector2, base.Rotation, this.def, this, 0f);
                }
            }
        }

        protected void DrawMoversVertical(Vector3 drawPos, float offsetDist, Graphic graphic, float altitude, Vector3 drawScaleFactor, Graphic_Shadow shadowGraphic)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 vector;
                Mesh mesh;
                if (i == 0)
                {
                    vector = new Vector3(0f, offsetDist, 0f);
                    mesh = MeshPool.plane10;
                }
                else
                {
                    vector = new Vector3(0f, -offsetDist, 0f);
                    mesh = MeshPool.plane10Flip;
                }

                Vector3 vector2 = drawPos;
                vector2.y = altitude;
                vector2 += vector;
                Graphics.DrawMesh(mesh, Matrix4x4.TRS(vector2, base.Rotation.AsQuat, new Vector3((float)this.def.size.x * drawScaleFactor.x, drawScaleFactor.y, (float)this.def.size.z * drawScaleFactor.z)), graphic.MatAt(base.Rotation, this), 0);
                if (shadowGraphic != null)
                {
                    shadowGraphic.DrawWorker(vector2, base.Rotation, this.def, this, 0f);
                }
            }
        }
    }
}
