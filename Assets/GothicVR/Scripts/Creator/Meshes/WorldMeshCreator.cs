using System.Threading.Tasks;
using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using UnityEngine;

namespace GVR.Creator.Meshes
{
    public class WorldMeshCreator : AbstractMeshCreator<WorldMeshCreator>
    {
        public async Task<GameObject> CreateAsync(WorldData world, GameObject parent, int meshesPerFrame)
        {
            var meshObj = new GameObject()
            {
                name = "Mesh",
                isStatic = true
            };
            meshObj.SetParent(parent);

            // Track the progress of each sub-mesh creation separately
            int numSubMeshes = world.subMeshes.Values.Count;
            int meshesCreated = 0;
            
            foreach (var subMesh in world.subMeshes.Values)
            {
                var subMeshObj = new GameObject()
                {
                    name = subMesh[0].material.name!,
                    isStatic = true
                };
                subMeshObj.SetParent(meshObj);

                var i = 0;
                foreach (var subSubMesh in subMesh)
                {
                    var subSubMeshObj = new GameObject()
                    {
                        name = i++.ToString(),
                        isStatic = true
                    };
                    
                    var meshFilter = subSubMeshObj.AddComponent<MeshFilter>();
                    var meshRenderer = subSubMeshObj.AddComponent<MeshRenderer>();

                    PrepareMeshRenderer(meshRenderer, subSubMesh);
                    PrepareMeshFilter(meshFilter, subSubMesh);
                    PrepareMeshCollider(subSubMeshObj, meshFilter.mesh, subSubMesh.material);

                    subSubMeshObj.SetParent(subMeshObj);

                    LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.WorldMesh, 1f / numSubMeshes);

                    if (++meshesCreated % meshesPerFrame == 0)
                        await Task.Yield(); // Yield to allow other operations to run in the frame  
                }
            }
            
            return meshObj;
        }
    }
}