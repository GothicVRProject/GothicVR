using GVR.Extensions;
using GVR.Manager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Player.Climb
{
    public class RayTeleportClimb : MonoBehaviour
    {
        [SerializeField] private XRInteractorLineVisual lineVisual;
        [SerializeField] private XRBaseInteractor interactor;
        [SerializeField] private GameObject player;

        [SerializeField] private Sprite sprite;

        private GameObject reticle;
        private Image reticleImage;

        private bool isHittingObject = false;
        private GameObject zsPos0GO;
        private GameObject zsPos1GO;
        private float hitTime;
        private float teleportDelay = 1f; // Adjust the delay duration as needed

        private string zsPos0 = "ZS_POS0";
        private string zsPos1 = "ZS_POS1";

        private void Start()
        {
            CreateReticle();
            interactor.selectEntered.AddListener(OnRaycastHit);
            interactor.selectExited.AddListener(OnRaycastExit);
        }

        private void OnRaycastHit(SelectEnterEventArgs args)
        {
            // Check if the interactable GameObject has the tag "Climbable"
            if (args.interactableObject != null &&
                args.interactableObject.transform.CompareTag(ConstantsManager.ClimbableTag))
            {
                // Show a message in the logs
                var hitObject = args.interactableObject.transform.gameObject;

                zsPos0GO = hitObject.FindChildRecursively(zsPos0);
                zsPos1GO = hitObject.FindChildRecursively(zsPos1);

                // Get the zs_pos0 and zs_pos1 positions
                var zsPos0Position = zsPos0GO.transform.position;
                var zsPos1Position = zsPos1GO.transform.position;

                Vector3 playerPosition = player.transform.position;

                float yDifferenceToZsPos0 = Mathf.Abs(playerPosition.y - zsPos0Position.y);
                float yDifferenceToZsPos1 = Mathf.Abs(playerPosition.y - zsPos1Position.y);

                reticle.SetActive(true);

                // Teleport the player to the closer zs_pos position based on y-level
                if (yDifferenceToZsPos0 < yDifferenceToZsPos1)
                {
                    reticle.SetParent(zsPos0GO, true, true);
                    reticleImage.rectTransform.localRotation = Quaternion.AngleAxis(0, Vector3.forward);
                }
                else
                {
                    reticle.SetParent(zsPos1GO, true, true);
                    reticleImage.rectTransform.localRotation = Quaternion.AngleAxis(180, Vector3.forward);
                }

                isHittingObject = true;

                // Record the hit time
                hitTime = Time.time;
            }
        }

        private void OnRaycastExit(SelectExitEventArgs args)
        {
            isHittingObject = false;
            reticle.transform.parent = null;
            reticleImage.fillAmount = 0;
            reticle.SetActive(false);
        }

        private void Update()
        {
            if (isHittingObject)
            {
                reticleImage.fillAmount = (Time.time - hitTime) / teleportDelay;
                // Check if the delay duration has passed since the hit
                if (Time.time - hitTime >= teleportDelay)
                {
                    PerformTeleport();
                }
            }
        }

        private void PerformTeleport()
        {
            // Get the player's position
            Vector3 playerPosition = player.transform.position;

            var zsPos0Position = zsPos0GO.transform.position;
            var zsPos1Position = zsPos1GO.transform.position;

            float yDifferenceToZsPos0 = Mathf.Abs(playerPosition.y - zsPos0Position.y);
            float yDifferenceToZsPos1 = Mathf.Abs(playerPosition.y - zsPos1Position.y);

            // Teleport the player to the closer zs_pos position based on y-level
            if (yDifferenceToZsPos0 < yDifferenceToZsPos1)
            {
                TeleportPlayer(zsPos1Position);
            }
            else
            {
                TeleportPlayer(zsPos0Position);
            }

            // Reset the state
            isHittingObject = false;

            // Deactivate the teleport ray
            if (interactor.gameObject.name.Contains("Teleport"))
                interactor.enabled = false;
        }

        private void TeleportPlayer(Vector3 targetPosition)
        {
            // Teleport the player to the target position
            player.transform.position = targetPosition;
        }

        private void CreateReticle()
        {
            reticle = new GameObject("Reticle");

            var canvas = new GameObject("Canvas");
            canvas.SetParent(reticle);
            canvas.AddComponent<Canvas>();
            var image = new GameObject("Image");
            image.SetParent(canvas);


            reticleImage = image.AddComponent<Image>();

            reticleImage.sprite = sprite;
            reticleImage.rectTransform.sizeDelta = new Vector2(1f, 1f);
            reticleImage.material = TextureManager.I.arrowmaterial;
            reticleImage.type = Image.Type.Filled;
            reticleImage.fillMethod = Image.FillMethod.Vertical;
            Destroy(reticle.GetComponent<Collider>());
            reticle.SetActive(false);
        }
    }
}
