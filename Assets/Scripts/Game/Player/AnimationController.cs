using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private const float MAX_CAMERA_YAW = 60f;
    private const float MAX_CAMERA_PITCH = 30f;
    private Animator _animator;
    private PhotonView _view;
    private CameraFollow _camera;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _view = GetComponentInParent<PhotonView>();
        _camera = GetComponentInParent<CameraFollow>();
    }
    private void Update()
    {
        if (!_view.IsMine) return;
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            _animator.SetInteger("Status", 1);
        }
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            _animator.SetInteger("Status", 0);
        }
        if(Keyboard.current.f1Key.wasPressedThisFrame)
        {
            _animator.SetBool("HeadMove", true);
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            _animator.SetBool("HeadMove", false);
        }

        _animator.SetFloat("LookX", _camera.NormalizeValue.x); 
        _animator.SetFloat("LookY", _camera.NormalizeValue.y); 
    }


}
