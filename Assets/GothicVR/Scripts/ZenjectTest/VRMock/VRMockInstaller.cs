using System;
using GVR.ZenjectTest.Main;
using Zenject;

namespace GVR.ZenjectTest.VRMock
{
    public class VRMockInstaller : MonoInstaller
    {
        private const string HVR_BOOTSTRAPPER = "GVR.ZenjectTest.HVR.HVRInstaller";

        public override void InstallBindings()
        {
            // Cross-check, if HVR is set up, then break.
            if (Type.GetType(HVR_BOOTSTRAPPER) != null)
                return;

            Container.Bind<IInjectable>().To<VRMockInjectable>().AsSingle();
        }
    }
}
