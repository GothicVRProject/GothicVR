using System;
using GVR.Util;
using UnityEngine;

namespace GVR.Debugging
{
    public class GameOptions : SingletonBehaviour<GameOptions>
    {
        [Header("__________UI Settings__________")]
        public bool uiFollowsPlayer;
    }

}