using System;
using PxCs.Data.Mesh;
using UnityEngine;

namespace GVR.Extensions
{
    public static class PxDataExtension
    {
        public static BoneWeight ToBoneWeight(this PxWeightEntryData[] weights)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights are null.");
            if (weights.Length == 0 || weights.Length > 4)
                throw new ArgumentOutOfRangeException($"Only 1...4 weights are currently supported but >{weights.Length}< provided.");

            var data = new BoneWeight();

            data.boneIndex0 = weights[0].nodeIndex;
            data.weight0 = weights[0].weight;
            if (weights.Length == 1)
                return data;

            data.boneIndex1 = weights[1].nodeIndex;
            data.weight1 = weights[1].weight;
            if (weights.Length == 2)
                return data;

            data.boneIndex2 = weights[2].nodeIndex;
            data.weight2 = weights[2].weight;
            if (weights.Length == 3)
                return data;

            data.boneIndex3 = weights[3].nodeIndex;
            data.weight3 = weights[3].weight;
            return data;
        }

        public static BoneWeight ToBoneWeight(this PxWeightEntryData[] weights, int[] nodeMapping)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights are null.");
            if (weights.Length == 0 || weights.Length > 4)
                throw new ArgumentOutOfRangeException($"Only 1...4 weights are currently supported but >{weights.Length}< provided.");

            var data = new BoneWeight();

            for (int i = 0; i < weights.Length; i++)
            {
                int index = Array.IndexOf(nodeMapping, weights[i].nodeIndex);
                if (index == -1)
                    throw new ArgumentException($"No matching node index found in nodeMapping for weights[{i}].nodeIndex.");

                switch (i)
                {
                    case 0:
                        data.boneIndex0 = index;
                        data.weight0 = weights[i].weight;
                        break;
                    case 1:
                        data.boneIndex1 = index;
                        data.weight1 = weights[i].weight;
                        break;
                    case 2:
                        data.boneIndex2 = index;
                        data.weight2 = weights[i].weight;
                        break;
                    case 3:
                        data.boneIndex3 = index;
                        data.weight3 = weights[i].weight;
                        break;
                }
            }

            return data;
        }
    }
}