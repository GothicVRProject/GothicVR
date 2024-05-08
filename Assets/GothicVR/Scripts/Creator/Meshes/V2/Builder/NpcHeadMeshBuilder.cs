using System.Text.RegularExpressions;
using GVR.Extensions;
using GVR.Npc;
using GVR.Properties;
using UnityEngine;

namespace GVR.Creator.Meshes.V2.Builder
{
    public class NpcHeadMeshBuilder : NpcMeshBuilder
    {
        public override GameObject Build()
        {
            var headGo = RootGo.FindChildRecursively("BIP01 HEAD");

            if (headGo == null)
            {
                Debug.LogWarning($"No NPC head found for {RootGo.name}");
                return RootGo;
            }

            var props = RootGo.GetComponent<NpcProperties>();

            // Cache it for faster use during runtime
            props.head = headGo.transform;
            props.headMorph = headGo.AddComponent<HeadMorph>();
            props.headMorph.HeadName = props.BodyData.Head;

            var headMeshFilter = headGo.AddComponent<MeshFilter>();
            var headMeshRenderer = headGo.AddComponent<MeshRenderer>();
            PrepareMeshFilter(headMeshFilter, Mmb.Mesh, headMeshRenderer);
            PrepareMeshRenderer(headMeshRenderer, Mmb.Mesh);

            return RootGo;
        }

        /// <summary>
        /// Change texture name based on VisualBodyData.
        /// </summary>
        protected override Texture2D GetTexture(string name)
        {
            string finalTextureName = name;

            // FIXME - We don't have different mouths in Gothic1. Need to recheck it in Gothic2.
            if (name.ToUpper().EndsWith("MOUTH_V0.TGA"))
            {
                finalTextureName = name;
            }
            else if (name.ToUpper().EndsWith("TEETH_V0.TGA"))
            {
                // e.g. Some_Texture_V0.TGA --> Some_Texture_V1.TGA
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0", $"V{bodyData.TeethTexNr}");
            }
            else if (name.ToUpper().EndsWith("V0_C0.TGA"))
            {
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0_C0",
                    $"V{bodyData.HeadTexNr}_C{bodyData.BodyTexColor}");
            }

            return base.GetTexture(finalTextureName);
        }
    }
}
