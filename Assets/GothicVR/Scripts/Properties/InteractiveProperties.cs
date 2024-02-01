using ZenKit.Vobs;

namespace GVR.Properties
{
    public class InteractiveProperties : MovableProperties
    {
        public new InteractiveObject Properties;

        public override void SetData(IVirtualObject data)
        {
            if (data is InteractiveObject InteractiveData)
            {
                Properties = InteractiveData;
                base.Properties = InteractiveData;
            }

            base.SetData(data);
        }
    }
}