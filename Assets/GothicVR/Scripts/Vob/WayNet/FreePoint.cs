namespace GVR.Vob.WayNet
{
    [System.Serializable]
    public class FreePoint : WayNetPoint
    {
        public override bool IsFreePoint()
        {
            return true;
        }
    }
}
