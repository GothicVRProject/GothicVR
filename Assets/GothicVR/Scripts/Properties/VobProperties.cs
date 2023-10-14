using System.Linq;
using UnityEngine;

namespace GVR.Properties
{
    public class VobProperties : AbstractProperties
    {
        [field: SerializeField]
        public string visual { get; private set; }

        /// <summary>
        /// It's some hidden magic. Created based on PxVob.visual extracting the first part.
        /// Needed, because within Daedalus, there are functions requesting it. e.g. Wld_IsMobAvailable (self,"BED")
        /// </summary>
        [field: SerializeField]
        public string visualScheme { get; private set; }

        
        public void SetVisual(string visualName)
        {
            visual = visualName;
            visualScheme = visualName.Split('_').First(); // e.g. BED_1_OC.ASC => BED
        }
    }
}