using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Extensions;
using GVR.Globals;
using GVR.Vm;
using GVR.Properties;
using TMPro;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using Mesh = UnityEngine.Mesh;

namespace GVR.Lab.Handler
{
    public class NpcHandler : MonoBehaviour, IHandler
    {
        public TMP_Dropdown animationsDropdown;
        public GameObject bloodwynSlotGo;
        public BloodwynInstanceId bloodwynInstanceId;
        
        public enum BloodwynInstanceId
        {
            Deu = 6596
        }

        private NpcInstance bloodwynInstance;
        private string[] animations = {
            "T_LGUARD_2_STAND", "T_STAND_2_LGUARD", "T_LGUARD_SCRATCH", "T_LGUARD_STRETCH", "T_LGUARD_CHANGELEG",
            "T_HGUARD_2_STAND", "T_STAND_2_HGUARD", "T_HGUARD_LOOKAROUND"
        };

        public void Bootstrap()
        {
            animationsDropdown.options = animations.Select(item => new TMP_Dropdown.OptionData(item)).ToList();

            BootstrapBloodwyn();
        }

        private void BootstrapBloodwyn()
        {
            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.SetParent(bloodwynSlotGo);

            var npcSymbol = GameData.GothicVm.GetSymbolByIndex((int)bloodwynInstanceId);
            bloodwynInstance = GameData.GothicVm.AllocInstance<NpcInstance>(npcSymbol!);
            var properties = newNpc.GetComponent<NpcProperties>();
            LookupCache.NpcCache[bloodwynInstance.Index] = properties;
            properties.npcInstance = bloodwynInstance;

           GameData.GothicVm.InitInstance(bloodwynInstance);
            
            properties.Dialogs = GameData.Dialogs.Instances
                .Where(dialog => dialog.Npc == bloodwynInstance.Index)
                .OrderByDescending(dialog => dialog.Important)
                .ToList();
            newNpc.name = bloodwynInstance.GetName(NpcNameSlot.Slot0);
            GameData.GothicVm.GlobalSelf = bloodwynInstance;

            // Hero
            {
                // Need to be set for later usage (e.g. Bloodwyn checks your inventory if enough nuggets are carried)
                var heroGo = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
                heroGo.SetParent(bloodwynSlotGo);
                var heroInstance = GameData.GothicVm.InitInstance<NpcInstance>("hero");
                LookupCache.NpcCache[heroInstance.Index] = properties;
                GameData.GothicVm.GlobalOther = heroInstance;
            }

            var mdmName = "Hum_GRDM_ARMOR.asc";
            var mdhName = "Humans_Militia.mds";
            var body = new VmGothicExternals.ExtSetVisualBodyData()
            {
                Armor = 3643,
                Body = "hum_body_Naked0",
                BodyTexColor = 1,
                BodyTexNr = 0,
                Head = "Hum_Head_Bald",
                HeadTexNr = 18,
                TeethTexNr = 1
            };

            MeshObjectCreator.CreateNpc(newNpc.name, mdmName, mdhName, body, newNpc);

            // TODO - MorphMesh test only
            mmb = AssetCache.TryGetMmb("Hum_Head_Bald");
            head = GameObject.Find("BIP01 HEAD");
            mesh = head.GetComponent<MeshFilter>().mesh;
            anim = mmb.Animations.First(anim => anim.Name.EqualsIgnoreCase("VISEME"));
        }

        public void AnimationStartClick()
        {
            VmGothicExternals.AI_PlayAni(bloodwynInstance, animationsDropdown.options[animationsDropdown.value].text);
        }


        private IMorphMesh mmb;
        private GameObject head;
        private IMorphAnimation anim;
        private Mesh mesh;

        private float frame;
        private int lastFrame = -1;

        private void Update()
        {
            if (mmb == null)
                return;

            var vertices = mesh.vertices;
            var vertexMapping = AbstractMeshCreator.DebugBloodwynVertexMapping;

            frame += Time.deltaTime;

            // Do not show a frame twice.
            if (lastFrame == (int)frame)
                return;

            // It's a hack to show only one new frame each second.
            lastFrame = (int)frame;

            var frameIndexOffset = (int)frame * anim.Vertices.Count;

            foreach (var vertexId in anim.Vertices)
            {
                var vertexElementsFromMapping = vertexMapping[vertexId];
                var vertexValue = anim.Samples[vertexId + frameIndexOffset];

                foreach (var vertexMappingId in vertexElementsFromMapping)
                {
                    // Test: Just check if Morph mesh is working after all - Big head mode
                    // vertices[vertexMappingId] = vertices[vertexMappingId] * 1.1f;

                    // Test: Do the animations "additive" - Open mouth mode
                    // vertices[vertexMappingId] += vertexValue.ToUnityVector();

                    // Test: Set the head vertex to new MorphMesh value - Tiny head mode
                    vertices[vertexMappingId] = vertexValue.ToUnityVector();
                }
            }

            mesh.vertices = vertices;

            // This Vignette has only 16 frames.
            if (frame > 15)
            {
                Debug.Log("Last frame reached.");
                frame = 0;
            }
        }
    }
}
