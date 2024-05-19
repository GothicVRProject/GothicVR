using GVR.Caches;
using GVR.Context;
using GVR.Creator.Meshes.V2;
using UnityEngine;

namespace GVR.Lab.Handler
{
    public class LabLadderLabHandler : MonoBehaviour, ILabHandler
    {
        public GameObject ladderSlot;

        public void Bootstrap()
        {
            var ladderName = "LADDER_3.MDL";
            var mdl = AssetCache.TryGetMdl(ladderName);

            var vobObj = MeshFactory.CreateVob(ladderName, mdl, Vector3.zero, Quaternion.Euler(0, 270, 0),
                ladderSlot, useTextureArray: false);

            GVRContext.ClimbingAdapter.AddClimbingComponent(vobObj);
        }
    }
}
