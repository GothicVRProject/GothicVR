using GVR.Morph;

namespace GVR.Vob
{
    // Currently no difference from AbstractMorphAnimation. But you never know. ;-)
    public class VobAnimateMorph : AbstractMorphAnimation
    {
        public void StartAnimation(string morphMeshName)
        {
            StartAnimation(morphMeshName, null);
        }
    }
}
