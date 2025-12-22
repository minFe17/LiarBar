using UnityEngine;
using System;

public class SeotdaBottleController : MonoBehaviour
{
    private const float SMOOTH_SPEED = 5;
    private int _curTurnIndex = -1;
    private SeotdaTurnManager _turn;
    private bool _isMove = false;
    private Quaternion _rotation;

    private void Start()
    {
        OnBottle();
        _turn = GetComponentInParent<SeotdaTurnManager>();
        //_curTurnIndex = _turn.Turn;
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
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
        if (Quaternion.Angle(transform.rotation, _rotation) < 0.01f)
        {
            transform.rotation = _rotation; 
            _isMove = false;
        }
    }
    private void ChangeRotation()
    {
        _curTurnIndex = _turn.Turn;
        Vector3[] rotations = {new Vector3(0, 90, 90), new Vector3(0,360,90), new Vector3(0,270,90), new Vector3(0,180,90)};

        _rotation = Quaternion.Euler(rotations[_curTurnIndex]);

        // Vector3 cross = Vector3.Cross(transform.forward, _rotation * Vector3.forward);
        // 
        // if (cross.y > 0f)
        // {
        //     _rotation =  Quaternion.Euler(new Vector3(0, rotations[_curTurnIndex].y - 360f, rotations[_curTurnIndex].z));
        // }

        Debug.Log(transform.rotation.z);
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
