using UnityEngine;
using System;

public class SeotdaBottleController : MonoBehaviour
{
    private const float SMOOTH_SPEED = 5;
    private int _curTurnIndex = -1;
    private SeotdaTurnManager _turn;
    private bool _isMove = false;
    private Quaternion _rotation;
    private Quaternion _baseRotation;

    private void Start()
    {
        _baseRotation = transform.rotation;
        _turn = GetComponentInParent<SeotdaTurnManager>();
        _curTurnIndex = _turn.Turn;
    }
    private void OnEnable()
    {
        //보틀 끄는건 작성했는데, 켜는거 작성안함
        EventManager.Instance.Subscribe("OnBottle", OnBottle);
        EventManager.Instance.Subscribe("OffBottle", OffBottle);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("OnBottle", (Action)OnBottle);
        EventManager.Instance.UnSubscribe("OffBottle",(Action) OffBottle);
    }

    private void Update()
    {
        if(_curTurnIndex!=_turn.Turn)
        {
            ChangeRotation();
        }
        if (!_isMove) return;
        MoveRotation();
    }
    private void MoveRotation()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _rotation,
            Time.deltaTime * SMOOTH_SPEED
        );
        if (Quaternion.Angle(transform.rotation, _rotation) < 0.001f)
        {
            transform.rotation = _rotation; 
            _isMove = false;
        }
    }
    private void ChangeRotation()
    {
        _curTurnIndex = _turn.Turn;
        float targetAngle = 90f * _turn.Turn;

        _rotation = _baseRotation * Quaternion.Euler(0, targetAngle, 0);

        Vector3 cross = Vector3.Cross(transform.forward, _rotation * Vector3.forward);

        if (cross.y > 0f)
        {
            _rotation = _baseRotation * Quaternion.Euler(0, targetAngle - 360f, 0);
        }

        _isMove = true;
    }
    private void OnBottle()
    {
        gameObject.SetActive(true);
    }
    private void OffBottle()
    {
        gameObject.SetActive(false);
    }
}
