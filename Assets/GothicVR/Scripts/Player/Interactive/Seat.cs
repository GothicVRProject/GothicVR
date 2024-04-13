using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GVR.Player.Camera;
using GVR.Globals;

namespace GVR.Player.Interactive
{
    public class Seat : MonoBehaviour
    {
        //this will enable player to sit on benches/chairs etc
        private Vector3 posOffset = new Vector3(0,-0.75f,0.9f);
        private Vector3 eulerOffset = new Vector3(0,180,0);
        private const float cameraFadeDuration = 0.15f;
        private const float sittingCooldown = 0.5f;

        private XRGrabInteractable interactable;
        private bool isPlayerSeated;
        private bool isNpcSeated; //when NPC sits down this should be changed to true and when NPC stands up change it back to false
        private List<Transform> snapPoints = new List<Transform>();
        private Transform currentSnapPoint;

        private Vector3 cachedPos, cachedEulers;
        private GameObject cachedLocomotion;
        private CameraFade cachedCameraFade;
        private bool canPlayerSit = true;

        private void Start()
        {
            gameObject.layer = Constants.InteractiveLayer;
            //get snap points
            GetSnapPoints();
            //get interactable
            interactable = GetComponent<XRGrabInteractable>();
            //add ToggleSitting listener to SelectEntered
            interactable.selectEntered.AddListener(ToggleSitting);
        }
        private void GetSnapPoints()
        {
            //find children that contain "ZS_POS" in object name and add to list of snap points
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if(child.name.ToLower().Contains("zs_pos"))
                {
                    //add snap point
                    snapPoints.Add(child);
                }
            }
        }
        public void ToggleSitting(SelectEnterEventArgs args)
        {
            if (isNpcSeated || !canPlayerSit) return; // stop player from sitting if an NPC is already sitting there or if its cooling down
            //get player object
            canPlayerSit = false; //disable this function to cooldown
            GameObject player = args.interactorObject.transform.GetComponentInParent<XROrigin>().gameObject;
            //handle sitting/standing
            isPlayerSeated = !isPlayerSeated;
            if (isPlayerSeated) StartCoroutine(SitDown(player));
            else StartCoroutine(StandUp(player));
            Invoke("EnableSitting", sittingCooldown);
        }

        private void EnableSitting()
        {
            canPlayerSit = true;
        }

        private IEnumerator SitDown(GameObject player)
        {
            //cache player pos and eulers
            cachedPos = player.transform.position;
            cachedEulers = player.transform.eulerAngles;

            //lock input
            cachedLocomotion = player.GetComponentInChildren<LocomotionSystem>().gameObject;
            cachedLocomotion.SetActive(false);

            //get snap point
            currentSnapPoint = GetNearestSnapPoint(player.transform.position);

            //fade camera out
            cachedCameraFade = player.GetComponentInChildren<CameraFade>();
            cachedCameraFade.Fade(cameraFadeDuration, 1);
            yield return new WaitForSeconds(cameraFadeDuration);

            //set position and rotation
            player.transform.position = currentSnapPoint.position + currentSnapPoint.TransformDirection(posOffset);
            player.transform.eulerAngles = currentSnapPoint.eulerAngles + eulerOffset;

            //fade camera in
            cachedCameraFade.Fade(cameraFadeDuration, 0);
        }
        private IEnumerator StandUp(GameObject player)
        {
            //fade camera out
            cachedCameraFade.Fade(cameraFadeDuration, 1);
            yield return new WaitForSeconds(cameraFadeDuration);
            //move player forward
            player.transform.position += player.transform.TransformDirection(new Vector3(0, 0.5f, 1f));
           // player.transform.eulerAngles = cachedEulers;
            //unlock move an turn input
            cachedLocomotion.SetActive(true);
            //fade camera in
            cachedCameraFade.Fade(cameraFadeDuration, 0);
            //clear cache
            cachedPos = Vector3.zero;
            cachedEulers = Vector3.zero;
            cachedLocomotion = null;
            cachedCameraFade = null;
        }
        private Transform GetNearestSnapPoint(Vector3 pos)
        {
            // Find the closest Snap Point for player to sit.
            return snapPoints
                .OrderBy(point => Vector3.Distance(pos, point.position))
                .First();
        }
    }
}
