using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GVR.Data;
using GVR.Extensions;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZenKit.Daedalus;

public class ControllerManager : SingletonBehaviour<ControllerManager>
{
    public GameObject raycastLeft;
    public GameObject raycastRight;
    public GameObject directLeft;
    public GameObject directRight;
    public GameObject MenuGameObject;
    public GameObject dialogGameObject;
    public List<GameObject> dialogItems;
    private InputAction leftPrimaryButtonAction;
    private InputAction leftSecondaryButtonAction;

    private InputAction rightPrimaryButtonAction;
    private InputAction rightSecondaryButtonAction;

    public GameObject MapObject;
    public float maprollspeed;
    public float maprolloffset;
    private Animator maproll;
    AudioSource mapaudio;
    AudioClip scrollsound;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        maproll = MapObject.gameObject.GetComponent<Animator>();
        mapaudio = MapObject.gameObject.GetComponent<AudioSource>();
        scrollsound = VobHelper.GetSoundClip("SCROLLROLL.WAV");
        MapObject.SetActive(false);
        maproll.enabled = false;

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
    }

    private void OnDestroy()
    {
        leftPrimaryButtonAction?.Disable();
        leftSecondaryButtonAction?.Disable();

        rightPrimaryButtonAction?.Disable();
        rightSecondaryButtonAction?.Disable();
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
        maproll.Play("Unroll", -1, 0.0f);
        mapaudio.PlayOneShot(scrollsound);
        yield return new WaitForSeconds((maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length / maprollspeed) * (maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length - maprolloffset) / maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        maproll.speed = 0f;
    }
    public IEnumerator RollupMap()
    {
        maproll.speed = maprollspeed;
        maproll.Play("Roll", -1, (1 - (maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length - maprolloffset) / maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length));
        mapaudio.PlayOneShot(scrollsound);
        yield return new WaitForSeconds((maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length / maprollspeed) * (maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length - maprolloffset) / maproll.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        maproll.speed = 0f;
        MapObject.SetActive(false);
    }

    public void ShowDialog()
    {
        dialogGameObject.SetActive(true);
    }

    public void HideDialog()
    {
        dialogGameObject.SetActive(false);
    }

    public void FillDialog(int npcInstanceIndex, List<DialogOption> dialogOptions)
    {
        CreateAdditionalDialogOptions(dialogOptions.Count);
        ClearDialogOptions();

        // G1 handles DialogOptions added via (Info_AddChoice()) in reverse order.
        dialogOptions.Reverse();
        for (var i = 0; i < dialogOptions.Count; i++)
        {
            var dialogItem = dialogItems[i];
            var dialogOption = dialogOptions[i];

            dialogItem.GetComponent<Button>().onClick.AddListener(
                () => OnDialogClick(npcInstanceIndex, dialogOption.Function, false));
            dialogItem.FindChildRecursively("Label").GetComponent<TMP_Text>().text = dialogOption.Text;
        }
    }

    public void FillDialog(int npcInstanceIndex, List<InfoInstance> dialogOptions)
    {
        CreateAdditionalDialogOptions(dialogOptions.Count);
        ClearDialogOptions();

        for (var i = 0; i < dialogOptions.Count; i++)
        {
            var dialogItem = dialogItems[i];
            var dialogOption = dialogOptions[i];

            dialogItem.GetComponent<Button>().onClick.AddListener(
                () => OnDialogClick(npcInstanceIndex, dialogOption.Information, true));
            dialogItem.FindChildRecursively("Label").GetComponent<TMP_Text>().text = dialogOption.Description;
        }
    }

    /// <summary>
    /// We won't know the maximum amount of element from the start of the game.
    /// Therefore we start with one entry only and create more if needed now.
    /// </summary>
    private void CreateAdditionalDialogOptions(int currentItemsNeeded)
    {
        var newItemsToCreate = currentItemsNeeded - dialogItems.Count;

        if (newItemsToCreate <= 0)
            return;

        var lastItem = dialogItems.Last();
        for (var i = 0; i < newItemsToCreate; i++)
        {
            var newItem = Instantiate(lastItem, lastItem.transform.parent, false);
            dialogItems.Add(newItem);

            newItem.name = $"Item{dialogItems.Count-1:00}";
            // FIXME - We need to handle this kind of UI magic more transparent somewhere else...
            newItem.transform.localPosition += new Vector3(0, -50 * (dialogItems.Count - 1), 0);
        }
    }

    private void ClearDialogOptions()
    {
        foreach (var item in dialogItems)
        {
            item.GetComponent<Button>().onClick.RemoveAllListeners();
            item.FindChildRecursively("Label").GetComponent<TMP_Text>().text = "";
        }
    }

    private void OnDialogClick(int npcInstanceIndex, int dialogId, bool isMainDialog)
    {
        DialogHelper.SelectionClicked(npcInstanceIndex, dialogId, isMainDialog);
    }
}
