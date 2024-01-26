using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using GVR.Caches;

public class ControllerManager : MonoBehaviour
{
    public GameObject raycastLeft;
    public GameObject raycastRight;
    public GameObject directLeft;
    public GameObject directRight;
    public GameObject MenuGameObject;
    public GameObject MapObject;
    public float maprollspeed;
    public float maprolloffset;

    private Animator maproll;
    AudioSource mapaudio;
    AudioClip scrollsound;

    private InputAction leftPrimaryButtonAction;
    private InputAction leftSecondaryButtonAction;

    private InputAction rightPrimaryButtonAction;
    private InputAction rightSecondaryButtonAction;

    private void Awake()
    {
        leftPrimaryButtonAction = new InputAction("primaryButton", binding: "<XRController>{LeftHand}/primaryButton");
        leftSecondaryButtonAction = new InputAction("secondaryButton", binding: "<XRController>{LeftHand}/secondaryButton");

        leftPrimaryButtonAction.started += ctx => ShowRayCasts();
        leftPrimaryButtonAction.canceled += ctx => HideRayCasts();

        leftPrimaryButtonAction.Enable();
        leftSecondaryButtonAction.Enable();

        rightPrimaryButtonAction = new InputAction("primaryButton", binding: "<XRController>{RightHand}/primaryButton");
        rightSecondaryButtonAction = new InputAction("secondaryButton", binding: "<XRController>{RightHand}/secondaryButton");

        rightPrimaryButtonAction.started += ctx => ShowMap();
        rightSecondaryButtonAction.started += ctx => ShowMainMenu();

        rightPrimaryButtonAction.Enable();
        rightSecondaryButtonAction.Enable();

        maproll = MapObject.gameObject.GetComponent<Animator>();
        mapaudio = MapObject.gameObject.GetComponent<AudioSource>();
        scrollsound = GVR.GothicVR.Scripts.Manager.VobHelper.GetSoundClip("SCROLLROLL.WAV");
    }

    private void OnDestroy()
    {
        leftPrimaryButtonAction.Disable();
        leftSecondaryButtonAction.Disable();

        rightPrimaryButtonAction.Disable();
        rightSecondaryButtonAction.Disable();
    }

    public void ShowRayCasts()
    {
        raycastLeft.SetActive(true);
        raycastRight.SetActive(true);
        directLeft.SetActive(false);
        directRight.SetActive(false);
    }

    public void HideRayCasts()
    {
        raycastLeft.SetActive(false);
        raycastRight.SetActive(false);
        directLeft.SetActive(true);
        directRight.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (!MenuGameObject.activeSelf)
            MenuGameObject.SetActive(true);
        else
            MenuGameObject.SetActive(false);
    }

    public void ShowMap()
    {
        if (!MapObject.activeSelf)
            StartCoroutine(UnrollMap());
        else
            StartCoroutine(RollupMap());
    }

    public IEnumerator UnrollMap()
    {
        MapObject.SetActive(true);
        maproll.enabled = true;
        maproll.speed = maprollspeed;
        maproll.Play("Unroll",- 1,0.0f);
        mapaudio.PlayOneShot(scrollsound);
        yield return new WaitForSeconds((maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length/ maprollspeed)* (maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length - maprolloffset) / maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        maproll.speed = 0f;
    }
    public IEnumerator RollupMap()
    {
        maproll.speed = maprollspeed;
        maproll.Play("Roll",- 1, (1-(maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length-maprolloffset)/ maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length));
        mapaudio.PlayOneShot(scrollsound);
        yield return new WaitForSeconds((maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length / maprollspeed) * (maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length - maprolloffset) / maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        maproll.speed = 0f;
        MapObject.SetActive(false);
    }

    public void ShowInventory()
    {
    }
}