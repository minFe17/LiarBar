using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    private PhotonView _view;
    private Camera _camera;
    private Transform _head;
    private float smoothSpeed = 1.0f;
    private Vector3 _rotation = new Vector3(20,0,0);

    private void Start()
    {
        _view = GetComponent<PhotonView>();
        _camera = GetComponentInChildren<Camera>();

        if (!_view.IsMine)
        {
            if (_camera != null) _camera.gameObject.SetActive(false);
            return;
        }

        if (Camera.main != null)
            Camera.main.gameObject.SetActive(false);

        _head = FindHeadInActiveModel(transform.Find("StandCharacter"));
        _camera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_head == null || !_view.IsMine) return;
        

    }
    private void LateUpdate()
    {
        if (_head == null || !_view.IsMine) return;

        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _head.position, Time.deltaTime * smoothSpeed);
        //_camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, _head.rotation, Time.deltaTime * smoothSpeed);
        //회전 들어가면 너무 어지러워서 그냥 끔

    }

    private Transform FindHeadInActiveModel(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeInHierarchy) continue;

            Transform head = FindHead(child);
            if (head != null) return head;
        }
        return null;
    }

    private Transform FindHead(Transform root)
    {
        if (root == null || !root.gameObject.activeInHierarchy)
            return null;

        string[] path = { "Root", "Hips", "Spine", "Spine1", "Neck", "Head" };
        Transform current = root;

        foreach (var part in path)
        {
            if (current == null) return null;
            current = current.Find(part);
            if (current == null || !current.gameObject.activeInHierarchy)
                return null; // 비활성화된 중간 뼈대가 있으면 null
        }

        return current;
    }
}
