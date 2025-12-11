using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    private const float MAX_PITCH = 30f;
    private const float MAX_YAW = 60f;

    private PhotonView _view;
    private Camera _camera;
    private Transform _head;
    private float _smoothSpeed = 1.0f;

    private ECameraMode _mode;

    private float _sens = 0.2f;
    private float _pitch = 0f;
    private float _yaw = 0f;
    private float _startYaw = 0f;

    private Vector2 _normalizeValue;

    public Vector2 NormalizeValue
    {
        get { return _normalizeValue; }
    }


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

        _startYaw = transform.eulerAngles.y;
        _yaw = _startYaw;

        _mode = ECameraMode.PlayerViewPoint;
    }

    private void Update()
    {
        if (!_view.IsMine) return;

        if(_head == null)
            _head = FindHeadInActiveModel(transform.Find("StandCharacter"));

        switch (_mode)
        {
            case ECameraMode.PlayerViewPoint:
                SetCameraRotation();
                break;
            case ECameraMode.PlayerDiwViewPoint:
                //이거 씬에서 카메라 테이블에달아놓은거 찾아줘야됨
                break;
            case ECameraMode.WinnerViewPoint:
                //이기면 카메라 각도 틀어줘야됨
                break;
        }
    }

    private void LateUpdate()
    {
        if (_head == null || !_view.IsMine || _mode!= ECameraMode.PlayerViewPoint) return;

        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _head.position, Time.deltaTime * _smoothSpeed);

        //_camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, Quaternion.Euler(_pitch, _yaw, 0f), Time.deltaTime * 30);
    }

    private void SetCameraRotation()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        float mouseX = delta.x * _sens;
        float mouseY = delta.y * _sens;

        _yaw += mouseX;
        _pitch -= mouseY;

        _pitch = Mathf.Clamp(_pitch, -MAX_PITCH, MAX_PITCH); // 상하 제한
        _yaw = Mathf.Clamp(_yaw, _startYaw - MAX_YAW, _startYaw + MAX_YAW); // 좌우 제한
        _camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        SetNormalize();

    }
    private void SetNormalize()
    {
        _normalizeValue.x =  Mathf.Clamp((_yaw - _startYaw) / MAX_YAW, -1f, 1f);
        _normalizeValue.y = -Mathf.Clamp(_pitch / MAX_PITCH, -1f, 1f);
    }
    private Transform FindHeadInActiveModel(Transform parent)
    {
        // 이거 MyPlayer에 하면 그 로테이션 셋팅안된상태로 적용돼서 카메라 회전제대로 안잡힌다.
        // 이부분 수정할 것. 
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
                return null;
        }

        return current;
    }


}
