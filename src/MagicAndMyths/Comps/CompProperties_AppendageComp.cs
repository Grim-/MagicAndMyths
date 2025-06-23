using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AppendageComp : CompProperties
    {
        public CompProperties_AppendageComp()
        {
            compClass = typeof(BezierAppendage);
        }
    }
    public class BezierAppendage : ThingComp
    {
        public Vector3 localTargetPoint;
        public float thickness = 0.15f;
        public int segments = 16;
        public float curvature = 0.5f;
        private Mesh appendageMesh;
        private Material appendageMaterial;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            appendageMaterial = MaterialPool.MatFrom("VFX/spark_06", ShaderDatabase.Cutout);
            GenerateAppendageMesh();
            SetTarget(this.parent.Position.ToVector3Shifted() + new Vector3(5, 0, 5));
        }

        private Vector3 CalculateControlPoint()
        {
            Vector3 midpoint = localTargetPoint * 0.5f;
            Vector3 direction = localTargetPoint;
            Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x).normalized;
            float distance = direction.magnitude * curvature;
            return midpoint + perpendicular * distance;
        }

        private Vector3 GetBezierPoint(float t)
        {
            Vector3 controlPoint = CalculateControlPoint();
            float u = 1 - t;
            return u * u * Vector3.zero + 2 * u * t * controlPoint + t * t * localTargetPoint;
        }

        private Vector3 GetBezierTangent(float t)
        {
            Vector3 controlPoint = CalculateControlPoint();
            return (2 * (1 - t) * (controlPoint - Vector3.zero) + 2 * t * (localTargetPoint - controlPoint)).normalized;
        }

        private void GenerateAppendageMesh()
        {
            if (appendageMesh == null)
                appendageMesh = new Mesh();

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 point = GetBezierPoint(t);
                Vector3 forward = GetBezierTangent(t);
                Vector3 right = Vector3.Cross(forward, Vector3.up).normalized * thickness;

                verts.Add(point - right);
                verts.Add(point + right);

                float uvY = (float)i / segments;
                uvs.Add(new Vector2(0f, uvY));
                uvs.Add(new Vector2(1f, uvY));

                if (i < segments)
                {
                    int baseVert = i * 2;

                    tris.Add(baseVert);
                    tris.Add(baseVert + 2);
                    tris.Add(baseVert + 1);

                    tris.Add(baseVert + 1);
                    tris.Add(baseVert + 2);
                    tris.Add(baseVert + 3);
                }
            }

            appendageMesh.Clear();
            appendageMesh.SetVertices(verts);
            appendageMesh.SetTriangles(tris, 0);
            appendageMesh.SetUVs(0, uvs);
            appendageMesh.RecalculateNormals();
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (appendageMesh != null && appendageMaterial != null)
            {
                Vector3 drawPos = parent.DrawPos;
                Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(60, Vector3.up), Vector3.one * 100);
                Graphics.DrawMesh(appendageMesh, matrix, appendageMaterial, 5);
            }
        }

        public void SetTarget(Vector3 worldTarget)
        {
            localTargetPoint = worldTarget - parent.DrawPos;
            GenerateAppendageMesh();
        }

        public void ReachForThing(Thing thing)
        {
            if (thing != null)
            {
                SetTarget(thing.DrawPos);
            }
        }
    }
}