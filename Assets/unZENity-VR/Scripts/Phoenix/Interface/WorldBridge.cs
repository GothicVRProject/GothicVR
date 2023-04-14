using PxCs;
using System;
using System.Collections.Generic;
using UZVR.Phoenix.Data;
using UZVR.Phoenix.Util;
using static UZVR.Phoenix.Data.WorldData;

namespace UZVR.Phoenix.Interface
{
    public static class WorldBridge
    {
        public static WorldData LoadWorld(IntPtr vdfsPtr, string worldName)
        {
            var worldPtr = PxWorld.pxWorldLoadFromVdf(vdfsPtr, worldName);
            if (worldPtr == IntPtr.Zero)
                throw new ArgumentException($"World >{worldName}< couldn't be found.");

            var worldMeshPtr = PxWorld.pxWorldGetMesh(worldPtr);
            if (worldMeshPtr == IntPtr.Zero)
                throw new ArgumentException($"No mesh in world >{worldName}< found.");

            WorldData world = new()
            {
                vertexIndices = PxMesh.GetPolygonVertexIndices(worldMeshPtr),
                materialIndices = PxMesh.GetPolygonMaterialIndices(worldMeshPtr),
                featureIndices = PxMesh.GetPolygonFeatureIndices(worldMeshPtr),

                vertices = PxMesh.GetVertices(worldMeshPtr),
                features = PxMesh.GetFeatures(worldMeshPtr),
                materials = PxMesh.GetMaterials(worldMeshPtr),

                waypoints = PxWorld.GetWayPoints(worldPtr),
                waypointEdges = PxWorld.GetWayEdges(worldPtr)
            };

            PxWorld.pxWorldDestroy(worldPtr);

            return world;
        }

        public static Dictionary<int, SubMeshData> CreateSubmeshesForUnity(WorldData world)
        {
            Dictionary<int, SubMeshData> subMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId--)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];

                // The materialIndex was never used before.
                if (!subMeshes.ContainsKey(materialIndex))
                {
                    var newSubMesh = new SubMeshData()
                    {
                        materialIndex = materialIndex,
                        material = world.materials[materialIndex]
                    };

                    subMeshes.Add(materialIndex, newSubMesh);
                }

                var currentSubMesh = subMeshes[materialIndex];
                var origVertexIndex = vertexIndices[loopVertexIndexId];

                currentSubMesh.vertices.Add(vertices[origVertexIndex].ToUnityVector());

                var featureIndex = featureIndices[loopVertexIndexId];
                currentSubMesh.uvs.Add(features[featureIndex].texture.ToUnityVector());
                currentSubMesh.normals.Add(features[featureIndex].normal.ToUnityVector());

                currentSubMesh.triangles.Add(currentSubMesh.vertices.Count - 1);
            }

            return subMeshes;
        }
    }
}