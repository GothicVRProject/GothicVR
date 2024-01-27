using System.Collections.Generic;

namespace GVR.Manager.Settings
{
    [System.Serializable]
    public class GameSettings
    {
        public string GothicIPath;
        public string GothicILanguage;
        public string LogLevel;

        public string GothicMenuFontPath;
        public string GothicSubtitleFontPath;

        public Dictionary<string, Dictionary<string, string>> GothicINISettings = new();
    }
}