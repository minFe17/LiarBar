using NUnit.Framework;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class PlayerPosition : MonoBehaviour
{
    PhotonView _view;
    List<Vector3> _positions;
    Transform _transform;

    private void Start()
    {
        _view = GetComponent<PhotonView>();
        _transform = GetComponent<Transform>();
        _transform.localPosition = Vector3.one;
    }

    private void Update()
    {
        
    }
}
