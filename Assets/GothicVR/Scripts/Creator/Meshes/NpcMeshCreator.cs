using System.Text.RegularExpressions;
using GVR.Caches;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using UnityEngine;

namespace GVR.Creator.Meshes
{
    public class NpcMeshCreator : AbstractMeshCreator<NpcMeshCreator>
    {
        private int tmpBodyTexNr;
        private int tmpBodyTexColor;
        
        private VmGothicBridge.Mdl_SetVisualBodyData tempBodyData;

        public GameObject CreateNpc(string npcName, PxModelMeshData mdm, PxModelHierarchyData mdh,
            PxMorphMeshData morphMesh, VmGothicBridge.Mdl_SetVisualBodyData bodyData, GameObject parent)
        {
            tmpBodyTexNr = bodyData.bodyTexNr;
            tmpBodyTexColor = bodyData.bodyTexColor;
            
            var npcGo = Create(npcName, mdm, mdh, default, default, parent);

            AddHead(npcName, npcGo, morphMesh);
            
            return npcGo;
        }

        private void AddHead(string npcName, GameObject npcGo, PxMorphMeshData morphMesh)
        {
            var headGo = npcGo.FindChildRecursively("BIP01 HEAD");

            if (headGo == null)
            {
                Debug.LogWarning($"No NPC head found for {npcName}");
                return;
            }

            var headMeshFilter = headGo.AddComponent<MeshFilter>();
            var headMeshRenderer = headGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(headMeshRenderer, morphMesh.mesh);
            PrepareMeshFilter(headMeshFilter, morphMesh.mesh);
        }

        /// <summary>
        /// Change texture name based on VisualBodyData.
        /// </summary>
        protected override Texture2D GetTexture(string name)
        {
            // FIXME: Dirty hack. Needs to be optimized.
            if (name.ToUpper().Contains("MOUTH") || name.ToUpper().Contains("TEETH"))
                return base.GetTexture(name);
            
            if (!name.ToUpper().EndsWith("V0_C0.TGA"))
            {
                Debug.LogError($"The format of body texture isn't right for ${name}");
                return base.GetTexture(name);
            }

            var formattedTextureName = Regex.Replace(name, "(?<=.*?)V0_C0", $"V{tmpBodyTexNr}_C{tmpBodyTexColor}");

            return AssetCache.I.TryGetTexture(formattedTextureName);
        }
    }
}