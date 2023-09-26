namespace GVR.Npc.Actions
{
    public interface IAnimationCallbacks
    {
        public void AnimationCallback(string pxEventTagDataParam);
        public void AnimationEndCallback();
    }
}