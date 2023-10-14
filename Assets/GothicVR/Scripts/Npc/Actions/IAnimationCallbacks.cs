namespace GVR.Npc.Actions
{
    public interface IAnimationCallbacks
    {
        public void AnimationCallback(string pxEventTagDataParam);
        public void AnimationSfxCallback(string pxEventSfxDataParam);
        public void AnimationEndCallback();
    }
}