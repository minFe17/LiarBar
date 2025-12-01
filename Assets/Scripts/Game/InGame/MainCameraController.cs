using UnityEngine;
using UnityEngine.InputSystem; 

public class MainCameraController : MonoBehaviour
{
    private const int CAMERA_SPEED = 50;
    private Transform _transform;
    private Vector3 _rotation = new Vector3();

    private void Start()
    {
        _transform = GetComponent<Transform>();
    }

    private void Update()
    {
        _rotation = _transform.eulerAngles;

        // Q 누르는 동안
        if (Keyboard.current.qKey.isPressed)
        {
            _rotation.y -= CAMERA_SPEED * Time.deltaTime;
            _transform.eulerAngles = _rotation;
        }
        // E 누르는 동안
       if (Keyboard.current.eKey.isPressed)
        {
            _rotation.y += CAMERA_SPEED * Time.deltaTime;
            _transform.eulerAngles = _rotation;
        }
    }
}
