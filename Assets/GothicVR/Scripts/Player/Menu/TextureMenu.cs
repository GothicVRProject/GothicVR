using GVR.Manager;
using UnityEngine;

namespace GVR.Player.Menu
{
    public class TextureMenu : MonoBehaviour
    {
        [SerializeField] private GameObject MainMenuImageBackground;
        [SerializeField] private GameObject MainMenuBackground;
        [SerializeField] private GameObject MainMenuText;
        private void Start()
        {
            SetMaterials();
        }

        public void SetMaterials()
        {
            var mmib = MainMenuImageBackground.GetComponent<MeshRenderer>();
            var mmb = MainMenuBackground.GetComponent<MeshRenderer>();
            var mmt = MainMenuText.GetComponent<MeshRenderer>();

            mmib.material = TextureManager.I.MainMenuImageBackgroundMaterial;
            mmb.material = TextureManager.I.MainMenuBackgroundMaterial;
            mmt.material = TextureManager.I.MainMenuTextImageMaterial;
        }
    }

}