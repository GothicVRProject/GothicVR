using GVR.ZenjectTest.Main;
using Zenject;

namespace GVR.ZenjectTest.HVR
{
    public class HVRInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInjectable>().To<HVRInjectable>().AsSingle();
        }
    }
}
