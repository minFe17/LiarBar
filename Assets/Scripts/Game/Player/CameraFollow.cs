using Photon.Pun;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private PhotonView _view;
    private Camera _camera;
    private Transform _head;
    private float smoothSpeed = 10f;

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

        Transform activeModel = FindActiveModel(transform.Find("StandCharacter"));

        _head = FindHead(activeModel);

        _camera.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (_head == null || !_view.IsMine) return;

        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _head.position, Time.deltaTime * smoothSpeed);
        _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, _head.rotation, Time.deltaTime * smoothSpeed);
    }

    private Transform FindActiveModel(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeInHierarchy)
                return child;
        }
        return null;
    }

    private Transform FindHead(Transform root)
    {
        string[] path = { "Root", "Hips", "Spine", "Spine1", "Neck", "Head" };
        Transform current = root;
        foreach (var part in path)
        {
            if (current == null) return null;
            current = current.Find(part);
        }
        return current;
    }
}
