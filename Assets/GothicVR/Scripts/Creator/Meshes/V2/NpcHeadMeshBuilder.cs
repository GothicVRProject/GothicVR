using System.Collections.Generic;
using System.Text.RegularExpressions;
using GVR.Caches;
using GVR.Extensions;
using GVR.Npc;
using GVR.Properties;
using GVR.Vm;
using UnityEngine;
using ZenKit;
using Mesh = UnityEngine.Mesh;

namespace GVR.Creator.Meshes.V2
{
    public class NpcHeadMeshBuilder : NpcMeshBuilder
    {
        private bool isMorphMeshMappingAlreadyCached;
        private string headName;

        public override void SetBodyData(VmGothicExternals.ExtSetVisualBodyData body)
        {
            // We prepare the key now, so that we don't need to recreate it every PrepareMeshFilterMorphMeshEntry() call later.
            headName = MorphMeshCache.GetPreparedKey(body.Head);

            base.SetBodyData(body);
        }

        public override GameObject Build()
        {
            BuildHead();

            return RootGo;
        }

        private void BuildHead()
        {
            if (string.IsNullOrEmpty(headName))
            {
                return;
            }

            var headGo = RootGo.FindChildRecursively("BIP01 HEAD");
            var morphMesh = AssetCache.TryGetMmb(headName);

            if (headGo == null)
            {
                Debug.LogWarning($"No NPC head found for {ObjectName}");
                return;
            }

            var props = RootGo.GetComponent<NpcProperties>();

            // Cache it for faster use during runtime
            props.head = headGo.transform;
            props.headMorph = headGo.AddComponent<HeadMorph>();

            var headMeshFilter = headGo.AddComponent<MeshFilter>();
            var headMeshRenderer = headGo.AddComponent<MeshRenderer>();
            PrepareMeshFilter(headMeshFilter, morphMesh.Mesh, headMeshRenderer);
            PrepareMeshRenderer(headMeshRenderer, morphMesh.Mesh);
        }

        /// <summary>
        /// Change texture name based on VisualBodyData.
        /// </summary>
        protected override Texture2D GetTexture(string name)
        {
            string finalTextureName = name;

            // FIXME - We don't have different mouths in Gothic1. Need to recheck it in Gothic2.
            if (name.ToUpper().EndsWith("MOUTH_V0.TGA"))
            {
                finalTextureName = name;
            }
            else if (name.ToUpper().EndsWith("TEETH_V0.TGA"))
            {
                // e.g. Some_Texture_V0.TGA --> Some_Texture_V1.TGA
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0", $"V{bodyData.TeethTexNr}");
            }
            else if (name.ToUpper().EndsWith("V0_C0.TGA"))
            {
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0_C0",
                    $"V{bodyData.HeadTexNr}_C{bodyData.BodyTexColor}");
            }

            return base.GetTexture(finalTextureName);
        }

        protected override void CreateMorphMeshBegin(IMultiResolutionMesh mrm, Mesh mesh)
        {
            // MorphMeshes will change the vertices. This call optimizes performance.
            mesh.MarkDynamic();

            isMorphMeshMappingAlreadyCached = MorphMeshCache.IsMappingAlreadyCached(headName);
            if (isMorphMeshMappingAlreadyCached)
            {
                return;
            }

            MorphMeshCache.AddVertexMapping(headName, mrm.PositionCount);
        }

        protected override void CreateMorphMeshEntry(int index1, int preparedVerticesCount)
        {
            // We add mapping data to later reuse for IMorphAnimation samples
            if (isMorphMeshMappingAlreadyCached)
            {
                return;
            }

            MorphMeshCache.AddVertexMappingEntry(headName, index1, preparedVerticesCount - 1);
        }

        protected override void CreateMorphMeshEnd(List<Vector3> preparedVertices)
        {
            if (isMorphMeshMappingAlreadyCached)
            {
                return;
            }

            MorphMeshCache.SetUnityVerticesForVertexMapping(headName, preparedVertices.ToArray());
        }
    }
}
