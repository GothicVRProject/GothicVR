using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Npc.Actions.AnimationActions
{
    public class OutputSvm : Output
    {
        private string preparedSvmFileName;
        
        // Overwriting this lookup as it let's us reuse the inherited Output class.
        protected override string outputName => preparedSvmFileName;
    
        public OutputSvm(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        {
        }

        public override void Start()
        {
            var svm = AssetCache.TryGetSvmData(Props.npcInstance.Voice);
            preparedSvmFileName = svm.GetAudioName(Action.String0);
            
            base.Start();
        }
    }
}
