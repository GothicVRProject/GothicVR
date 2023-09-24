using GVR.Caches;
using GVR.Extensions;
using PxCs.Data.Vob;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GVR.Creator.Meshes
{
    public class VobMeshCreator : AbstractMeshCreator<VobMeshCreator>
    {
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