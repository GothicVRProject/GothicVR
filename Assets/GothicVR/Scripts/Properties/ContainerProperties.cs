using ZenKit.Vobs;

namespace GVR.Properties
{
    public class ContainerProperties : InteractiveProperties
    {
        public new Container Properties;

        public override void SetData(IVirtualObject data)
        {
            if (data is Container containerData)
            {
                Properties = containerData;
                base.Properties = containerData;
            }

            base.SetData(data);
        }
    }
}