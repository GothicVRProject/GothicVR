using System.Collections;
using GVR.Caches;
using GVR.Extensions;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolystripMeshCreator : MonoBehaviour
{
    private Material material;

    public void CreatePolyStrip(int numberOfSegments, Vector3 startPoint, Vector3 endPoint)
    {
        material = new Material(Shader.Find("Unlit/ThunderShader"));

        material.ToAdditiveMode();

        var texture = AssetCache.TryGetTexture("THUNDER_A0.TGA");

        if (texture != null)
        {
            material.mainTexture = texture;
        }

        Vector3 direction = (endPoint - startPoint);
        float segmentLength = direction.magnitude / numberOfSegments;
        direction.Normalize();

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material; // Set the material

        Vector3[] vertices = new Vector3[(numberOfSegments + 1) * 2];
        int[] triangles = new int[numberOfSegments * 6];
        Vector2[] uv = new Vector2[(numberOfSegments + 1) * 2];


        for (int i = 0; i <= numberOfSegments; i++)
        {
            Vector3 segmentStart = startPoint + direction * (segmentLength * i);

            vertices[i * 2] = segmentStart;
            vertices[i * 2 + 1] = (segmentStart + new Vector3(0, 30, 0));


            uv[i * 2] = new Vector2(0, (float)i / numberOfSegments);
            uv[i * 2 + 1] = new Vector2(1, (float)i / numberOfSegments);

            uv[i * 2].y = 1 - uv[i * 2].y;
            uv[i * 2 + 1].y = 1 - uv[i * 2 + 1].y;

            if (i < numberOfSegments)
            {
                int baseIndex = i * 6;
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
    }

    public IEnumerator RevealSegments()
    {
        var threshold = material.GetFloat("_ShowThreshold");
        // Assuming you're increasing _ShowThreshold over time in this coroutine
        while (threshold < 1)
        {
            threshold += 0.3f;
            material.SetFloat("_ShowThreshold", threshold);
            // Wait for the next frame
            yield return new WaitForSeconds(0.05f);
        }

        // _ShowThreshold has reached 1, now wait for an additional 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        // Set _ShowThreshold back to 0
        material.SetFloat("_ShowThreshold", 0);
    }
}