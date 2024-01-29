using System.Linq;
using GVR.Caches;
using GVR.Properties;
using UnityEngine.SceneManagement;
using ZenKit.Daedalus;

namespace GVR.Globals
{
    public static class Hero
    {
        public static void LoadHeroVM()
        {
            var hero = GameData.GothicVm.InitInstance<NpcInstance>("hero");
            GameData.GothicVm.GlobalHero = hero;

            var heroProperties = new NpcProperties();
            LookupCache.NpcCache[hero.Index] = heroProperties;
            
            GvrEvents.GeneralSceneLoaded.AddListener(SetHeroGameObject);
        }

        private static void SetHeroGameObject()
        {
            var generalScene = SceneManager.GetSceneByName(Constants.SceneGeneral);
            if (GameData.GothicVm.GlobalHero != null)
                LookupCache.NpcCache[GameData.GothicVm.GlobalHero.Index].go = generalScene.GetRootGameObjects()
                    .FirstOrDefault(go => go.name == "PlayerController")
                    ?.transform.Find("VRPlayer").gameObject;
        }
    }
}