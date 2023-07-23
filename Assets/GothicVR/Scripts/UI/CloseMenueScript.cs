using UnityEngine;

public class CloseMenueScript : MonoBehaviour
{
    private Vector3 menuePosition;
    private Quaternion menueRotation;
    private GameObject menueParent;
    private void Start()
    {
        menuePosition = transform.localPosition;
        menueRotation = transform.localRotation;
        menueParent = transform.parent.gameObject;
        gameObject.SetActive(false);
    }

    public void CloseFunction()
    {
        transform.parent = menueParent.transform;
        transform.localRotation = menueRotation;
        transform.localPosition = menuePosition;
        gameObject.SetActive(false);
    }
}
