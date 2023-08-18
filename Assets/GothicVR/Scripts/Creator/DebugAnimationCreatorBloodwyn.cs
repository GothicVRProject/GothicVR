using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Model;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace GVR.Creator
{
    public class DebugAnimationCreatorBloodwyn: SingletonBehaviour<DebugAnimationCreatorBloodwyn>
    {
        private const string worldName = "world.zen";
        
        public void Create()
        {
            if (!FeatureFlags.I.CreateExampleAnimation)
                return;

            CreateNaked();
            CreateCrawler();
            CreateShadow();
            CreateGuard();

            var mdmBroken = AssetCache.I.TryGetMdm("Hum_GRDM_ARMOR");
            var mdmWorking = AssetCache.I.TryGetMdm("Hum_STTS_ARMOR");
            var mdmWorkingCrawler = AssetCache.I.TryGetMdm("Hum_CRAWLER_ARMOR");
            var mdmWorkingNaked = AssetCache.I.TryGetMdm("Hum_Body_Naked0.ASC");
        }

        private void CreateNaked()
        {
            var name = "DebugBloodwyn";
            var mdhName = "HUMANS.mdh";
            var head = "Hum_Head_Bald"; // B - Hum_Head_Bald --- D - Hum_Head_Thief
            var armor = "hum_body_Naked0"; // B - Hum_GRDM_ARMOR --- D - Hum_STTS_ARMOR --- N - hum_body_Naked0 --- C - Hum_CRAWLER_ARMOR
            var variant = new VmGothicBridge.ExtSetVisualBodyData()
            {
                BodyTexNr = 0, // B=0, D=0
                BodyTexColor = 1, // B=1, D=2
                HeadTexNr = 18, // B=18, D=15
                TeethTexNr = 1, // B=1, D=4
            };

            var mdh = AssetCache.I.TryGetMdh(mdhName);
            var mmb = AssetCache.I.TryGetMmb(head);
            var mdm = AssetCache.I.TryGetMdm(armor);
            var obj = NpcMeshCreator.I.CreateNpc(name, mdm, mdh, mmb, variant, null);

            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(obj);

            obj.transform.localPosition = new(-35f, 10f, 0);

            
            var mdsName = "HUMANS.mds";
            var animationName = "t_Stand_2_Jump";
            var mds = AssetCache.I.TryGetMds(mdsName);

            try
            {
                // PlayAnimationBloodwyn(obj, mds, mdh, mdsName, animationName);
            }
            catch (Exception e)
            {
                var x = e.StackTrace;
            }
        }

        private void CreateCrawler()
        {
            var name = "DebugBloodwyn";
            var mdhName = "HUMANS.mdh";
            var head = "Hum_Head_Bald"; // B - Hum_Head_Bald --- D - Hum_Head_Thief
            var armor = "Hum_CRAWLER_ARMOR"; // B - Hum_GRDM_ARMOR --- D - Hum_STTS_ARMOR --- N - hum_body_Naked0 --- C - Hum_CRAWLER_ARMOR
            var variant = new VmGothicBridge.ExtSetVisualBodyData()
            {
                BodyTexNr = 0, // B=0, D=0
                BodyTexColor = 1, // B=1, D=2
                HeadTexNr = 18, // B=18, D=15
                TeethTexNr = 1, // B=1, D=4
            };

            var mdh = AssetCache.I.TryGetMdh(mdhName);
            var mmb = AssetCache.I.TryGetMmb(head);
            var mdm = AssetCache.I.TryGetMdm(armor);
            var obj = NpcMeshCreator.I.CreateNpc(name, mdm, mdh, mmb, variant, null);

            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(obj);

            obj.transform.localPosition = new(-36f, 10f, 0);

            
            var mdsName = "HUMANS.mds";
            var animationName = "t_Stand_2_Jump";
            var mds = AssetCache.I.TryGetMds(mdsName);

            try
            {
                // PlayAnimationBloodwyn(obj, mds, mdh, mdsName, animationName);
            }
            catch (Exception e)
            {
                var x = e.StackTrace;
            }
        }

        private void CreateShadow()
        {
            var name = "DebugBloodwyn";
            var mdhName = "HUMANS.mdh";
            var head = "Hum_Head_Bald"; // B - Hum_Head_Bald --- D - Hum_Head_Thief
            var armor = "Hum_STTS_ARMOR"; // B - Hum_GRDM_ARMOR --- D - Hum_STTS_ARMOR --- N - hum_body_Naked0 --- C - Hum_CRAWLER_ARMOR
            var variant = new VmGothicBridge.ExtSetVisualBodyData()
            {
                BodyTexNr = 0, // B=0, D=0
                BodyTexColor = 1, // B=1, D=2
                HeadTexNr = 18, // B=18, D=15
                TeethTexNr = 1, // B=1, D=4
            };

            var mdh = AssetCache.I.TryGetMdh(mdhName);
            var mmb = AssetCache.I.TryGetMmb(head);
            var mdm = AssetCache.I.TryGetMdm(armor);
            var obj = NpcMeshCreator.I.CreateNpc(name, mdm, mdh, mmb, variant, null);

            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(obj);

            obj.transform.localPosition = new(-37f, 10f, 0);

            
            var mdsName = "HUMANS.mds";
            var animationName = "t_Stand_2_Jump";
            var mds = AssetCache.I.TryGetMds(mdsName);

            try
            {
                // PlayAnimationBloodwyn(obj, mds, mdh, mdsName, animationName);
            }
            catch (Exception e)
            {
                var x = e.StackTrace;
            }
        }

        private void CreateGuard()
        {
            var name = "DebugBloodwyn";
            var mdhName = "HUMANS.mdh";
            var head = "Hum_Head_Bald"; // B - Hum_Head_Bald --- D - Hum_Head_Thief
            var armor = "Hum_GRDM_ARMOR"; // B - Hum_GRDM_ARMOR --- D - Hum_STTS_ARMOR --- N - hum_body_Naked0 --- C - Hum_CRAWLER_ARMOR
            var variant = new VmGothicBridge.ExtSetVisualBodyData()
            {
                BodyTexNr = 0, // B=0, D=0
                BodyTexColor = 1, // B=1, D=2
                HeadTexNr = 18, // B=18, D=15
                TeethTexNr = 1, // B=1, D=4
            };

            var mdh = AssetCache.I.TryGetMdh(mdhName);
            var mmb = AssetCache.I.TryGetMmb(head);
            var mdm = AssetCache.I.TryGetMdm(armor);
            var obj = NpcMeshCreator.I.CreateNpc(name, mdm, mdh, mmb, variant, null);

            SceneManager.GetSceneByName(worldName).GetRootGameObjects().Append(obj);

            obj.transform.localPosition = new(-38f, 10f, 0);

            
            var mdsName = "HUMANS.mds";
            var animationName = "t_Stand_2_Jump";
            var mds = AssetCache.I.TryGetMds(mdsName);

            try
            {
                // PlayAnimationBloodwyn(obj, mds, mdh, mdsName, animationName);
            }
            catch (Exception e)
            {
                var x = e.StackTrace;
            }
        }

        
        
        private void PlayAnimationBloodwyn(GameObject rootObj, PxModelScriptData mds, PxModelHierarchyData mdh, string mdsName, string animationName)
        {
            PxAnimationData[] animations = new PxAnimationData[mds.animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                var animName = mdsName.Replace(".MDS", $"-{mds.animations[i].name}.MAN", StringComparison.OrdinalIgnoreCase);
                animations[i] = PxAnimation.LoadFromVfs(GameData.I.VfsPtr, animName);
            }
            
            var animation = animations.First(i => i.name.ToUpper() == animationName.ToUpper());

            var animationComp = rootObj.gameObject.AddComponent<Animation>();
            var clip = new AnimationClip();
            clip.legacy = true;

            var curves = new Dictionary<string, List<AnimationCurve>>((int)animation.nodeCount);
            var boneNames = animation.node_indices.Select(nodeIndex => mdh.nodes[nodeIndex].name).ToArray();

            // Initialize array
            for (var boneId = 0; boneId < boneNames.Length; boneId++)
            {
                var boneName = boneNames[boneId];
                curves.Add(boneName, new List<AnimationCurve>(7));

                // Initialize 7 dimensions. (3x position, 4x rotation)
                curves[boneName].AddRange(Enumerable.Range(0, 7).Select(i => new AnimationCurve()).ToArray());
            }

            // Add KeyFrames from PxSamples
            for (var i = 0; i < animation.samples.Length; i++)
            {
                // We want to know what time it is for the animation.
                // Therefore we need to know fps multiplied with current sample. As there are nodeCount samples before a new time starts,
                // we need to add this to the calculation.
                var time = (1 / animation.fps) * (int)(i / animation.nodeCount);
                var sample = animation.samples[i];
                var boneId = i % animation.nodeCount;
                var boneName = boneNames[boneId];

                var boneList = curves[boneName];
                var uPosition = sample.position.ToUnityVector();

                // We add 6 properties for location and rotation.
                boneList[0].AddKey(time, uPosition.x);
                boneList[1].AddKey(time, uPosition.y);
                boneList[2].AddKey(time, uPosition.z);
                boneList[3].AddKey(time, -sample.rotation.w); // It's important to have this value with a -1. Otherwise animation is inversed.
                boneList[4].AddKey(time, sample.rotation.x);
                boneList[5].AddKey(time, sample.rotation.y);
                boneList[6].AddKey(time, sample.rotation.z);
            }

            foreach (var entry in curves)
            {
                var path = FindDeepChild(rootObj.transform, entry.Key, "");

                clip.SetCurve(path, typeof(Transform), "m_LocalPosition.x", entry.Value[0]);
                clip.SetCurve(path, typeof(Transform), "m_LocalPosition.y", entry.Value[1]);
                clip.SetCurve(path, typeof(Transform), "m_LocalPosition.z", entry.Value[2]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.w", entry.Value[3]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.x", entry.Value[4]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.y", entry.Value[5]);
                clip.SetCurve(path, typeof(Transform), "m_LocalRotation.z", entry.Value[6]);
            }

            clip.wrapMode = WrapMode.Loop;

            animationComp.AddClip(clip, "debug");
            animationComp.Play("debug");
        }
        string FindDeepChild(Transform parent, string name, string currentPath = "")
        {
            Transform result = parent.Find(name);

            if (result != null)
            {
                // The child object was found, return the current path
                if (currentPath != "")
                    return currentPath + "/" + name;
                else
                    return name;
            }
            else
            {
                // Search recursively in the children of the current object
                foreach (Transform child in parent)
                {
                    string childPath = currentPath + "/" + child.name;
                    string resultPath = FindDeepChild(child, name, childPath);

                    if (resultPath != null)
                    {
                        // The child object was found in a recursive call, return the result path
                        return resultPath.TrimStart('/');
                    }
                }

                // The child object was not found
                return null;
            }
        }
    }
}