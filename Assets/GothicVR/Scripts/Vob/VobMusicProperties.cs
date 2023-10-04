using System;
using PxCs.Data.Vob;
using UnityEngine;

namespace GothicVR.Vob
{
    public class VobMusicProperties : MonoBehaviour
    {
        // public PxVobZoneMusicData musicData;

        /// <summary>
        /// FIXME - In future, we could also mark PxVobZoneMusicData as [System.Serializable].
        /// FIXME - Then Unity will show its data without the need to reproduce data here.
        /// </summary>
        public void SetMusicData(PxVobZoneMusicData data)
        {
            isEnabled = data.enabled;
            priority = data.priority;
            ellipsoid = data.ellipsoid;
            reverb = data.reverb;
            volume = data.volume;
            loop = data.loop;
        }

        public bool isEnabled;
        public int priority;
        public bool ellipsoid;
        public float reverb;
        public float volume;
        public bool loop;
    }
}