using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ZenKit.Vobs;

namespace GVR.Creator.Meshes.V2.Builder
{
    public class VobDecalMeshBuilder : AbstractMeshBuilder
    {
        // Decals work only on URP shaders. We therefore temporarily change everything to this
        // until we know how to change specifics to the cutout only. (e.g. bushes)
        private const float DecalOpacity = 0.75f;

        private IVirtualObject vob;
        private VisualDecal decal;

        public void SetDecalData(IVirtualObject vob, VisualDecal decal)
        {
            this.vob = vob;
            this.decal = decal;
        }

        public override GameObject Build()
        {
            // G1: One Decal has no value to recognize what it is. Most likely a setup bug to ignore at this point.
            if (!vob.Name.IsEmpty())
                return null;

            var decalProj = RootGo.AddComponent<DecalProjector>();
            var texture = TextureCache.TryGetTexture(vob.Name);

            // x/y needs to be made twice the size and transformed from cm in m.
            // z - value is close to what we see in Gothic spacer.
            decalProj.size = new(decal.Dimension.X * 2 / 100, decal.Dimension.Y * 2 / 100, 0.5f);
            SetPosAndRot(RootGo, RootPosition, RootRotation);

            decalProj.pivot = Vector3.zero;
            decalProj.fadeFactor = DecalOpacity;

            // FIXME use Prefab!
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.0/manual/creating-a-decal-projector-at-runtime.html
            var standardShader = Constants.ShaderDecal;
            var material = new Material(standardShader);
            material.SetTexture(Shader.PropertyToID("Base_Map"), texture);

            decalProj.material = material;

            return RootGo;
        }
    }
}
