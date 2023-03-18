using UnityEngine;

namespace UZVR.Demo
{
    public class TestPlayer : MonoBehaviour
    {
        float speed = 5.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                transform.Translate(Vector3.forward * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                transform.Translate(-1 * Vector3.forward * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                transform.Rotate(0, -1, 0);
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                transform.Rotate(0, 1, 0);
        }
    }
}