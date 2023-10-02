using GVR.Caches;
using GVR.Extensions;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Struct;
using PxCs.Data.Vob;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GVR.Creator.Meshes
{
    public class VobMeshCreator : AbstractMeshCreator<VobMeshCreator>
    {
        public GameObject Create(string objectName, PxMultiResolutionMeshData mrm, Vector3 position,
            PxMatrix3x3Data rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            var go = base.Create(objectName, mrm, position, rotation, withCollider, parent, rootGo);
            
            AddZsCollider(go);
            
            return go;
        }

        public GameObject Create(string objectName, PxModelData mdl, Vector3 position, Quaternion rotation,
            GameObject parent = null, GameObject rootGo = null)
        {
            var go = base.Create(objectName, mdl, position, rotation, parent, rootGo);

            AddZsCollider(go);

            return go;
        }


        /// <summary>
        /// Add ZenginSlot collider. i.e. positions where 
        /// </summary>
        private void AddZsCollider(GameObject go)
        {
            if (go.transform.childCount == 0)
                return;
            
            var zm = go.transform.GetChild(0);
            for (var i = 0; i < zm.childCount; i++)
            {
                var child = zm.GetChild(i);
                if (!child.name.StartsWithIgnoreCase("ZS"))
                    continue;
                
                var sphereCollider = child.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
            }
        }

        
        public void CreateDecal(PxVobData vob, GameObject parent)
        {
            if (!vob.vobDecal.HasValue)
            {
                Debug.LogWarning("No decalData was set for: " + vob.visualName);
                return;
            }

            var decalData = vob.vobDecal.Value;

            var decalProjectorGo = new GameObject(decalData.name);
            var decalProj = decalProjectorGo.AddComponent<DecalProjector>();
            var texture = AssetCache.I.TryGetTexture(vob.visualName);

            // x/y needs to be made twice the size and transformed from cm in m.
            // z - value is close to what we see in Gothic spacer.
            decalProj.size = new(decalData.dimension.X * 2 / 100, decalData.dimension.Y * 2 / 100, 0.5f);
            decalProjectorGo.SetParent(parent);
            SetPosAndRot(decalProjectorGo, vob.position.ToUnityVector(), vob.rotation);

            decalProj.pivot = Vector3.zero;
            decalProj.fadeFactor = decalOpacity;

            // FIXME use Prefab!
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.0/manual/creating-a-decal-projector-at-runtime.html
            var standardShader = Shader.Find("Shader Graphs/Decal");
            var material = new Material(standardShader);
            material.SetTexture(Shader.PropertyToID("Base_Map"), texture);

            decalProj.material = material;
        }
    }
}