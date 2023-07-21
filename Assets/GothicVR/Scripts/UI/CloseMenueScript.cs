using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseMenueScript : MonoBehaviour
{
    private Vector3 menuePosition;
    private Quaternion menueRotation;
    private GameObject menueParent;
    private void Start()
    {
        menuePosition = gameObject.transform.localPosition;
        menueRotation = gameObject.transform.localRotation;
        menueParent = gameObject.transform.parent.gameObject;
        gameObject.SetActive(false);
    }
    public void closeFunction()
    {
        gameObject.transform.parent = menueParent.transform;
        gameObject.transform.localRotation = menueRotation;
        gameObject.transform.localPosition = menuePosition;
        gameObject.SetActive(false);
    }
}
