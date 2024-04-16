namespace GVR.Npc.Actions
{
    public interface IAnimationCallbacks
    {
        public void AnimationCallback(string pxEventTagDataParam);
        public void AnimationSfxCallback(string pxEventSfxDataParam);
        public void AnimationMorphCallback(string pxEventMorphDataParam);
        public void AnimationEndCallback();
    }
}
