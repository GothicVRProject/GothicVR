using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Extensions;
using UnityEngine;

namespace GVR.Caches
{
    /// <summary>
    /// We need to store various mappings and vertex data for MorphMeshes to work properly.
    /// We therefore use this separate cache class to ease reading.
    /// </summary>
    public static class MorphMeshCache
    {
        /// <summary>
        /// Vertex mapping is used for Mesh Morphing as we need to map Unity data back to ZenKit data for updates.
        ///
        /// In Unity we don't reuse vertices and always create a new one for another polygon (triangle).
        /// But when we re-map IMorphAnimation samples/vertices later on, we need to know the original vertexId from ZenKit.
        ///
        /// It goes like this:
        /// ["hum_head_bald] => {[0] => [i1,i2,i3,...]}
        ///  | Dict key is name of IMorphMesh
        ///                       | Array index is former vertexId
        ///                             | Data is new vertexIds from Unity mesh
        /// </summary>
        private static readonly Dictionary<string, List<List<int>>> VertexMapping = new();

        /// <summary>
        /// We also store the vertices migrated from ZenKit to Unity. They basically differentiate by:
        /// No vertex is reused for different triangles. Every time a vertex is needed, it got duplicated for Unity usage.
        /// </summary>
        private static readonly Dictionary<string, Vector3[]> UnityVertices = new();

        /// <summary>
        /// Save CPU cycles during runtime by caching the Morph values for every frame.
        ///
        /// e.g. ["Human-Head-Bald-Viseme"] = {[0] => {{x,y,z}, {x,y,z}}, [1] => {{x,y,z}, {x,y,z}}}
        /// [0] - is frame ID of the Morph Animation
        /// {{x,y,z}, {x,y,z}} - are the morph values of every vertex
        /// </summary>
        private static readonly Dictionary<string, List<Vector3[]>> AnimationMorphs = new();


        public static bool IsMappingAlreadyCached(string morphMeshName)
        {
            var preparedKey = GetPreparedKey(morphMeshName);

            return VertexMapping.ContainsKey(preparedKey);
        }

        public static void AddVertexMapping(string morphMeshName, int arraySize)
        {
            var preparedKey = GetPreparedKey(morphMeshName);

            VertexMapping.Add(preparedKey, new(arraySize));

            // Initialize
            Enumerable.Range(0, arraySize)
                .ToList()
                .ForEach(_ => VertexMapping[preparedKey].Add(new List<int>()));
        }

        public static void AddVertexMappingEntry(string preparedMorphMeshName, int originalVertexIndex, int additionalUnityVertexIndex)
        {
            var preparedKey = GetPreparedKey(preparedMorphMeshName);

            VertexMapping[preparedKey][originalVertexIndex].Add(additionalUnityVertexIndex);
        }

        public static void SetUnityVerticesForVertexMapping(string preparedMorphMeshName, Vector3[] unityVertices)
        {
            var preparedKey = GetPreparedKey(preparedMorphMeshName);

            UnityVertices.Add(preparedKey, unityVertices);
        }

        public static Vector3[] GetOriginalUnityVertices(string morphMeshName)
        {
            var preparedKey = GetPreparedKey(morphMeshName);

            return UnityVertices[preparedKey];
        }

        /// <summary>
        /// Cache and return MorphAnimation samples.
        ///
        /// [0] => [Vector1, V2, V3, ...]
        /// | Key is frameId
        ///        | Data is the already processed morph data (morph addition to original triangle data)
        /// </summary>
        public static List<Vector3[]> TryGetMorphData(string mmbName, string animationName)
        {
            var preparedMmbKey = GetPreparedKey(mmbName);
            var preparedAnimKey = GetPreparedKey(animationName);
            var preparedKey = $"{preparedMmbKey}-{preparedAnimKey}";

            if (AnimationMorphs.TryGetValue(preparedKey, out var data))
                return data;

            // Create logic
            var mmb = AssetCache.TryGetMmb(mmbName);
            var anim = mmb.Animations.First(anim => anim.Name.EqualsIgnoreCase(animationName));

            var originalVertexMapping = VertexMapping[preparedMmbKey];
            var originalUnityVertexData = UnityVertices[preparedMmbKey];
            // Original vertex count from ZenKit data.
            var vertexCount = anim.Vertices.Count;

            var newData = new List<Vector3[]>(anim.FrameCount);
            // Initialize - We set the VertexData for every frame to the original mesh value.
            Enumerable.Range(0, anim.FrameCount)
                .ToList()
                .ForEach(_ => newData.Add((Vector3[])originalUnityVertexData.Clone()));


            for (var i = 0; i < anim.SampleCount; i++)
            {
                var currentFrame = i / vertexCount;
                var currentZkVertexId = anim.Vertices[i % vertexCount];
                var morpMeshVectorAddition = anim.Samples[i].ToUnityVector();

                // For each unityVector at current frame, we add MorphMesh data.
                // Hint: If a vertex isn't named in the MorphMesh vertexIds, then we just leave it as original.
                foreach (var unityVertexId in originalVertexMapping[currentZkVertexId])
                {
                    newData[currentFrame][unityVertexId] = originalUnityVertexData[unityVertexId] + morpMeshVectorAddition;
                }
            }

            AnimationMorphs[preparedKey] = newData;

            return newData;
        }

        public static string GetPreparedKey(string key)
        {
            var lowerKey = key.ToLower();
            var extension = Path.GetExtension(lowerKey);

            if (extension == string.Empty)
                return lowerKey;
            else
                return lowerKey.Replace(extension, "");
        }

        public static void Dispose()
        {
            VertexMapping.Clear();
            UnityVertices.Clear();
            AnimationMorphs.Clear();
        }
    }
}
