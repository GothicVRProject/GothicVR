using PxCs.Data.Mesh;
using System;
using UnityEngine;

namespace GVR.Phoenix.Util
{
    public static class PxDataExtension
    {
        public static BoneWeight ToBoneWeight(this PxWeightEntryData[] weights)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights is null.");
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
    }
}
