using ZenKit.Vobs;

namespace GVR.Properties
{
    public class MovableProperties : VobProperties
    {
        public new MovableObject Properties;

        public override void SetData(IVirtualObject data)
        {
            if (data is MovableObject movableData)
            {
                Properties = movableData;
                base.Properties = movableData;
            }

            base.SetData(data);
        }
    }
}