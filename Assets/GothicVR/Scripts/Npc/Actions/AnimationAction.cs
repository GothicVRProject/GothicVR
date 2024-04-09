using UnityEngine;

namespace GVR.Npc.Actions
{
    public class AnimationAction
    {
            public AnimationAction(string string0 = null, int int0 = 0, int int1 = 0, uint uint0 = 0, float float0 = 0f, bool bool0 = false)
            {
                this.String0 = string0;
                this.Int0 = int0;
                this.Int1 = int1;
                this.Float0 = float0;
                this.Bool0 = bool0;
            }
            
            public readonly string String0;
            public readonly int Int0;
            public readonly int Int1;
            public readonly float Float0;
            public readonly bool Bool0;
    }
}
