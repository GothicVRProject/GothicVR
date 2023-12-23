using System;
using System.Collections.Generic;
using PxCs.Data.Mesh;
using UnityEngine;
using ZenKit;

namespace GVR.Extensions
{
    public static class PxDataExtension
    {
        public static BoneWeight ToBoneWeight(this List<SoftSkinWeightEntry> weights)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights are null.");
            if (weights.Count == 0 || weights.Count > 4)
                throw new ArgumentOutOfRangeException($"Only 1...4 weights are currently supported but >{weights.Count}< provided.");

            var data = new BoneWeight();

            data.boneIndex0 = weights[0].NodeIndex;
            data.weight0 = weights[0].Weight;
            if (weights.Count == 1)
                return data;

            data.boneIndex1 = weights[1].NodeIndex;
            data.weight1 = weights[1].Weight;
            if (weights.Count == 2)
                return data;

            data.boneIndex2 = weights[2].NodeIndex;
            data.weight2 = weights[2].Weight;
            if (weights.Count == 3)
                return data;

            data.boneIndex3 = weights[3].NodeIndex;
            data.weight3 = weights[3].Weight;
            return data;
        }

        public static BoneWeight ToBoneWeight(this List<SoftSkinWeightEntry> weights, List<int> nodeMapping)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights are null.");
            if (weights.Count == 0 || weights.Count > 4)
                throw new ArgumentOutOfRangeException($"Only 1...4 weights are currently supported but >{weights.Count}< provided.");

            var data = new BoneWeight();

            for (var i = 0; i < weights.Count; i++)
            {
                var index = Array.IndexOf(nodeMapping.ToArray(), weights[i].NodeIndex);
                if (index == -1)
                    throw new ArgumentException($"No matching node index found in nodeMapping for weights[{i}].nodeIndex.");

                switch (i)
                {
                    case 0:
                        data.boneIndex0 = index;
                        data.weight0 = weights[i].Weight;
                        break;
                    case 1:
                        data.boneIndex1 = index;
                        data.weight1 = weights[i].Weight;
                        break;
                    case 2:
                        data.boneIndex2 = index;
                        data.weight2 = weights[i].Weight;
                        break;
                    case 3:
                        data.boneIndex3 = index;
                        data.weight3 = weights[i].Weight;
                        break;
                }
            }

            return data;
        }
    }
}
