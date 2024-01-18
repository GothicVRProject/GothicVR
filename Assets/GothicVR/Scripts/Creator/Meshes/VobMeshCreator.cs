using System.Linq;
using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ZenKit;
using ZenKit.Vobs;
using Material = UnityEngine.Material;

namespace GVR.Creator.Meshes
{
    public class VobMeshCreator : AbstractMeshCreator
    {
        public GameObject CreateVob(string objectName, IMultiResolutionMesh mrm, Vector3 position,
            Quaternion rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            var go = Create(objectName, mrm, position, rotation, withCollider, parent, rootGo);

            AddZsCollider(go);

            return go;
        }

        public GameObject CreateVob(string objectName, IModel mdl, Vector3 position, Quaternion rotation,
            GameObject parent = null, GameObject rootGo = null)
        {
            var go = CreateVob(objectName, mdl.Mesh, mdl.Hierarchy, position, rotation, parent, rootGo);

            AddZsCollider(go);

            return go;
        }

        public GameObject CreateVob(string objectName, IModelMesh mdm, IModelHierarchy mdh,
            Vector3 position, Quaternion rotation, GameObject parent = null, GameObject rootGo = null)
        {
            // Check if there are completely empty elements without any texture.
            // G1: e.g. Harp, Flute, and WASH_SLOT (usage moved to a FreePoint within daedalus functions)
            var noMeshTextures = mdm.Meshes.All(mesh => mesh.Mesh.SubMeshes.All(subMesh => subMesh.Material.Texture.IsEmpty()));
            var noAttachmentTextures = mdm.Attachments.All(att => att.Value.Materials.All(mat => mat.Texture.IsEmpty()));

            if (noMeshTextures && noAttachmentTextures)
                return null;
            else
                return Create(objectName, mdm, mdh, position, rotation, parent, rootGo);
        }

        public GameObject CreateVobDecal(IVirtualObject vob, VisualDecal decal, GameObject parent)
        {
            // G1: One Decal has no value to recognize what it is. Most likely a setup bug to ignore at this point.
            if (!vob.Name.IsEmpty())
                return null;

            var decalProjectorGo = new GameObject(vob.Name);
            var decalProj = decalProjectorGo.AddComponent<DecalProjector>();
            var texture = AssetCache.TryGetTexture(vob.Name);

            // x/y needs to be made twice the size and transformed from cm in m.
            // z - value is close to what we see in Gothic spacer.
            decalProj.size = new(decal.Dimension.X * 2 / 100, decal.Dimension.Y * 2 / 100, 0.5f);
            decalProjectorGo.SetParent(parent);
            SetPosAndRot(decalProjectorGo, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion());

            decalProj.pivot = Vector3.zero;
            decalProj.fadeFactor = DecalOpacity;

            // FIXME use Prefab!
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.0/manual/creating-a-decal-projector-at-runtime.html
            var standardShader = Constants.ShaderDecal;
            var material = new Material(standardShader);
            material.SetTexture(Shader.PropertyToID("Base_Map"), texture);

            decalProj.material = material;

            return decalProjectorGo;
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
    }
}
