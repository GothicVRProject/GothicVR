using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        void Start()
        {
            var world = new PhoenixBridge().GetWorld();


            var root = new GameObject("World");

            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(root);
            var meshObj = new GameObject(string.Format("Mesh"));
            meshObj.transform.parent = root.transform;

            var meshFilter = meshObj.AddComponent<MeshFilter>();
            var meshRenderer = meshObj.AddComponent<MeshRenderer>();

            _PrepareMeshRenderer(meshRenderer);
            _PrepareMeshFilter(meshFilter, world);

            root.transform.localScale = Vector3.one / 100;

        }

        private void _PrepareMeshRenderer(MeshRenderer meshRenderer)
        {
            var standardShader = Shader.Find("Standard");

            meshRenderer.materials = new[] { new Material(standardShader), new Material(standardShader), new Material(standardShader) };

            meshRenderer.materials[0].color = Color.red;
            meshRenderer.materials[1].color = Color.yellow;
            meshRenderer.materials[2].color = Color.green;

            //using (var imageFile = File.OpenRead("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\___backup-modding\\GOTHIC MOD Development Kit\\VDFS-Tool\\_WORK\\DATA\\TEXTURES\\DESKTOP\\NOMIP\\INV_SLOT.TGA"))
            //{
            //    var texture = _LoadTGA(imageFile);
            //    material.mainTexture = texture;
            //}
        }
        private void _PrepareMeshFilter(MeshFilter meshFilter, World world)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.vertices = world.vertices.ToArray();
            mesh.triangles = world.triangles.ToArray();

            mesh.subMeshCount = 3;
            int oneThird = world.triangles.Count / 3;
            //meshFilter.mesh.SetSubMesh(1, new SubMeshDescriptor(0, world.triangles.Count));
            meshFilter.mesh.SetSubMesh(0, new SubMeshDescriptor(oneThird * 0, oneThird * 1 - 1));
            meshFilter.mesh.SetSubMesh(1, new SubMeshDescriptor(oneThird * 1, oneThird * 2 - 1));
            meshFilter.mesh.SetSubMesh(2, new SubMeshDescriptor(oneThird * 2, oneThird * 3 - 1));
        }

        // Credits: https://gist.github.com/mikezila/10557162
        //private Texture2D _LoadTGA(Stream TGAStream)
        //{

        //    using (BinaryReader r = new BinaryReader(TGAStream))
        //    {
        //        // Skip some header info we don't care about.
        //        // Even if we did care, we have to move the stream seek point to the beginning,
        //        // as the previous method in the workflow left it at the end.
        //        r.BaseStream.Seek(12, SeekOrigin.Begin);

        //        short width = r.ReadInt16();
        //        short height = r.ReadInt16();
        //        int bitDepth = r.ReadByte();

        //        // Skip a byte of header information we don't care about.
        //        r.BaseStream.Seek(1, SeekOrigin.Current);

        //        Texture2D tex = new Texture2D(width, height);
        //        Color32[] pulledColors = new Color32[width * height];

        //        if (bitDepth == 32)
        //        {
        //            for (int i = 0; i < width * height; i++)
        //            {
        //                byte red = r.ReadByte();
        //                byte green = r.ReadByte();
        //                byte blue = r.ReadByte();
        //                byte alpha = r.ReadByte();

        //                pulledColors[i] = new Color32(blue, green, red, alpha);
        //            }
        //        }
        //        else if (bitDepth == 24)
        //        {
        //            for (int i = 0; i < width * height; i++)
        //            {
        //                byte red = r.ReadByte();
        //                byte green = r.ReadByte();
        //                byte blue = r.ReadByte();

        //                pulledColors[i] = new Color32(blue, green, red, 1);
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("TGA texture had non 32/24 bit depth.");
        //        }

        //        tex.SetPixels32(pulledColors);
        //        tex.Apply();
        //        return tex;

        //    }
        //}
    }
}