using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Sounds;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GVR.Npc.Actions.AnimationActions
{
    public class Output: AbstractAnimationAction
    {
        private float audioPlaySeconds;

        private int speakerId => Action.Int0;
        protected virtual string outputName => Action.String0;
        
        public Output(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var soundData = AssetCache.TryGetSound(outputName);
            var audioClip = SoundCreator.ToAudioClip(soundData);
            audioPlaySeconds = audioClip.length;

            // Hero
            if (speakerId == 0)
            {
                // If NPC talked before, we stop it immediately (As some audio samples are shorter than the actual animation)
                AnimationCreator.StopAnimation(NpcGo);

               NpcHelper.GetHeroGameObject().GetComponent<AudioSource>().PlayOneShot(audioClip);
                // FIXME - Show subtitles somewhere next to Hero (== ourself/main camera)
            }
            // NPC
            else
            {
                var gestureCount = GetDialogGestureCount();
                var randomId = Random.Range(1, gestureCount+1);

                AnimationCreator.PlayAnimation(Props.mdsNames, $"T_DIALOGGESTURE_{randomId:00}", NpcGo);
                AnimationCreator.PlayHeadMorphAnimation(Props, HeadMorph.HeadMorphType.Viseme);

                Props.npcSound.PlayOneShot(audioClip);

                // FIXME - Show subtitles above NPC
            }
        }

        /// <summary>
        /// Gothic1 and Gothic 2 have different amount of Gestures. As we cached all animation names, we iterate through them once and return its number.
        /// </summary>
        private int GetDialogGestureCount()
        {
            if (GameData.Dialogs.GestureCount == 0)
            {
                // FIXME - We might need to check overlayMds and baseMds
                // FIXME - We might need to save amount of gestures based on mds names (if they differ for e.g. humans and orcs)
                var mds = AssetCache.TryGetMds(Props.baseMdsName);

                GameData.Dialogs.GestureCount = mds.Animations
                    .Count(anim => anim.Name.StartsWithIgnoreCase("T_DIALOGGESTURE_"));
            }

            return GameData.Dialogs.GestureCount;
        }

        public override bool IsFinished()
        {
            audioPlaySeconds -= Time.deltaTime;

            if (audioPlaySeconds <= 0f)
            {
                // NPC
                if (speakerId != 0)
                    AnimationCreator.StopHeadMorphAnimation(Props, HeadMorph.HeadMorphType.Viseme);

                return true;
            }
            else
                return false;
        }
    }
}
