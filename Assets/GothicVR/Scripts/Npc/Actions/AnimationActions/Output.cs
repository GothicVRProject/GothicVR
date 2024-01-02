using System;
using GVR.Caches;
using GVR.Creator.Sounds;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class Output: AbstractAnimationAction
    {
        private float audioPlaySeconds;

        public Output(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var soundData = AssetCache.TryGetSound(Action.String0);
            var audioClip = SoundCreator.ToAudioClip(soundData);
            audioPlaySeconds = audioClip.length;

            // Hero
            if (Action.Int0 == 0)
            {
                // FIXME - Play sound file on Hero's AudioSource - Use global lookup for Hero's voice
                GameObject.Find("HeroVoice").GetComponent<AudioSource>().PlayOneShot(audioClip);
                // FIXME - Show subtitles somewhere next to Hero (== ourself/main camera)
            }
            // NPC
            else
            {
                Props.npcSound.PlayOneShot(audioClip);
                // FIXME - Show subtitles above NPC
            }
        }

        public override bool IsFinished()
        {
            audioPlaySeconds -= Time.deltaTime;

            return audioPlaySeconds <= 0f;
        }
    }
}
