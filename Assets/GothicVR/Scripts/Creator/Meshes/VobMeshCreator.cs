using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using JetBrains.Annotations;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Struct;
using PxCs.Data.Vob;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GVR.Creator.Meshes
{
    public class VobMeshCreator : AbstractMeshCreator
    {
        // As we subclass the main Mesh Creator, we need to have a parent-child inheritance instance.
        // Needed e.g. for overwriting PrepareMeshRenderer() to change specific behaviour.
        private static readonly VobMeshCreator Self = new();

        public static GameObject Create(string objectName, PxMultiResolutionMeshData mrm, Vector3 position,
            PxMatrix3x3Data rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            var go = Self.CreateInternal(objectName, mrm, position, rotation, withCollider, parent, rootGo);
            
            Self.AddZsCollider(go);
            
            return go;
        }

        public static GameObject Create(string objectName, PxModelData mdl, Vector3 position, Quaternion rotation,
            GameObject parent = null, GameObject rootGo = null)
        {
            var go = Self.CreateInternal(objectName, mdl, position, rotation, parent, rootGo);

            Self.AddZsCollider(go);

            return go;
        }

        /// <summary>
        /// Add ZengineSlot collider. i.e. positions where an NPC can sit on a bench.
        /// </summary>
        private void AddZsCollider([CanBeNull] GameObject go)
        {
            if (go == null || go.transform.childCount == 0)
                return;
            
            var zm = go.transform.GetChild(0);
            for (var i = 0; i < zm.childCount; i++)
            {
                var child = zm.GetChild(i);
                if (!child.name.StartsWithIgnoreCase("ZS"))
                    continue;
                
                // Used for event triggers with NPCs.
                var coll = child.AddComponent<SphereCollider>();
                coll.isTrigger = true;
            }
        }

        public static GameObject Create(string objectName, PxModelMeshData mdm, PxModelHierarchyData mdh,
            Vector3 position, Quaternion rotation, GameObject parent = null, GameObject rootGo = null)
        {
            // Check if there are completely empty elements without any texture.
            // G1: e.g. Harp, Flute, and WASH_SLOT (usage moved to a FreePoint within daedalus functions)
            var noMeshTextures = mdm.meshes.All(mesh => mesh.mesh.subMeshes.All(subMesh => subMesh.material.texture == ""));
            var noAttachmentTextures = mdm.attachments.All(att => att.Value.materials.All(mat => mat.texture == ""));

            if (noMeshTextures && noAttachmentTextures)
                return null;
            else
                return Self.CreateInternal(objectName, mdm, mdh, position, rotation, parent, rootGo);
        }

        public static GameObject CreateDecal(PxVobData vob, GameObject parent)
        {
            // G1: One Decal has no value to recognize what it is. Most likely a setup bug to ignore at this point.
            if (!vob.vobDecal.HasValue)
                return null;

            var decalData = vob.vobDecal.Value;

            var decalProjectorGo = new GameObject(decalData.name);
            var decalProj = decalProjectorGo.AddComponent<DecalProjector>();
            var texture = AssetCache.TryGetTexture(vob.visualName);

            // x/y needs to be made twice the size and transformed from cm in m.
            // z - value is close to what we see in Gothic spacer.
            decalProj.size = new(decalData.dimension.X * 2 / 100, decalData.dimension.Y * 2 / 100, 0.5f);
            decalProjectorGo.SetParent(parent);
            Self.SetPosAndRot(decalProjectorGo, vob.position.ToUnityVector(), vob.rotation);

            decalProj.pivot = Vector3.zero;
            decalProj.fadeFactor = decalOpacity;

            // FIXME use Prefab!
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.0/manual/creating-a-decal-projector-at-runtime.html
            var standardShader = Shader.Find("Shader Graphs/Decal");
            var material = new Material(standardShader);
            material.SetTexture(Shader.PropertyToID("Base_Map"), texture);

            decalProj.material = material;

            return decalProjectorGo;
        }
    }
}
