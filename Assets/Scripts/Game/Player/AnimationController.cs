using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private const float CORRECTION_VALUE = 0.5f;
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


        _animator.SetFloat("LookX", _camera.NormalizeValue.x); 
        _animator.SetFloat("LookY", _camera.NormalizeValue.y + CORRECTION_VALUE); 
    }


}
