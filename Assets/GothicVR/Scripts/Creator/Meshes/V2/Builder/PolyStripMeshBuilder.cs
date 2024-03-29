
using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using UnityEngine;

namespace GVR.Creator.Meshes.V2.Builder
{
    public class PolyStripMeshBuilder : AbstractMeshBuilder
    {
        private int numberOfSegments;
        private Vector3 startPoint;
        private Vector3 endPoint;

        public void SetPolyStripData(int numberSegments, Vector3 start, Vector3 end)
        {
            numberOfSegments = numberSegments;
            startPoint = start;
            endPoint = end;
        }

        public override GameObject Build()
        {
            var material = new Material(Constants.ShaderThunder);

            material.ToAdditiveMode();

            var texture = TextureCache.TryGetTexture("THUNDER_A0.TGA");

            if (texture != null)
            {
                material.mainTexture = texture;
            }

            var direction = (endPoint - startPoint);
            var segmentLength = direction.magnitude / numberOfSegments;
            direction.Normalize();

            var mesh = new Mesh();
            RootGo.GetComponent<MeshFilter>().mesh = mesh;
            RootGo.GetComponent<MeshRenderer>().material = material; // Set the material

            var vertices = new Vector3[(numberOfSegments + 1) * 2];
            var triangles = new int[numberOfSegments * 6];
            var uv = new Vector2[(numberOfSegments + 1) * 2];


            for (var i = 0; i <= numberOfSegments; i++)
            {
                var segmentStart = startPoint + direction * (segmentLength * i);

                vertices[i * 2] = segmentStart;
                vertices[i * 2 + 1] = (segmentStart + new Vector3(0, 30, 0));


                uv[i * 2] = new Vector2(0, (float)i / numberOfSegments);
                uv[i * 2 + 1] = new Vector2(1, (float)i / numberOfSegments);

                uv[i * 2].y = 1 - uv[i * 2].y;
                uv[i * 2 + 1].y = 1 - uv[i * 2 + 1].y;

                if (i < numberOfSegments)
                {
                    var baseIndex = i * 6;
                    triangles[baseIndex] = i * 2;
                    triangles[baseIndex + 1] = i * 2 + 1;
                    triangles[baseIndex + 2] = i * 2 + 2;

                    triangles[baseIndex + 3] = i * 2 + 2;
                    triangles[baseIndex + 4] = i * 2 + 1;
                    triangles[baseIndex + 5] = i * 2 + 3;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            return RootGo;
        }
    }
}
