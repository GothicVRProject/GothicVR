using System;
using GVR.Adapter.Controls;
using Zenject;

namespace GVR.Flat
{
    public class FlatInstaller : MonoInstaller
    {
        private const string HVR_BOOTSTRAPPER = "GVR.HVR.HVRInstaller";

        public override void InstallBindings()
        {
            // Cross-check, if HVR is set up, then break.
            if (Type.GetType(HVR_BOOTSTRAPPER) != null)
                return;

            Container.Bind<IDemoInjectable>().To<FlatDemoInjectable>().AsSingle();
        }
    }
}
