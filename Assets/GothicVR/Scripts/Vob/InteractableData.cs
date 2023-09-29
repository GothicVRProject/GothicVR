using System.Linq;
using UnityEngine;

namespace GothicVR.Vob
{
    public class InteractableData : MonoBehaviour
    {
        public string visual { get; private set; }
        public string visualScheme { get; private set; }

        
        public void SetVisual(string visualName)
        {
            visual = visualName;
            visualScheme = visualName.Split('_').First(); // e.g. BED_1_OC.ASC => BED
        }
        
    }
}