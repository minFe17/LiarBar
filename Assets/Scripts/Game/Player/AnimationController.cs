using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private Animator _animator;
    private PhotonView _view;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _view = GetComponentInParent<PhotonView>();
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
    }


}
