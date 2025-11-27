using NUnit.Framework;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.InputSystem;

public class PlayerModelController : MonoBehaviour
{

    List<GameObject> _models = new List<GameObject>();
    PhotonView _view;
    int _index = 0;
    List<Vector3> _positions = new List<Vector3>();
    private void Start()
    {
        _view = GetComponentInParent<PhotonView>();
        FindModel();
        SetModelToIndex();
    }
    private void Update()
    {
        
    }

    private void SetModelToIndex()
    {
        int selectedIndex = 0;

        // photonView 기준으로 내 캐릭터인지 남 캐릭터인지 확인
        Player targetPlayer = _view.IsMine ? PhotonNetwork.LocalPlayer : _view.Owner;

        if (targetPlayer != null && targetPlayer.CustomProperties.ContainsKey("SelectedCharacterIndex"))
        {
            object idxObj = targetPlayer.CustomProperties["SelectedCharacterIndex"];
            if (idxObj is int idx)
                selectedIndex = idx;
        }
        if (_view.IsMine)
        {

        }
        
        ActiveSelectedModel(selectedIndex);
    }
    private void ActiveSelectedModel(int index)
    {
        _models[_index].SetActive(false);
        _models[index].SetActive(true);
        _index = index;
    }
    private void FindModel()
    {
        // transform은 이 스크립트가 붙은 오브젝트 기준
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in transform)
        {
            if (child.gameObject.name.ToLower().Contains("npc"))
            {
                _models.Add(child.gameObject);
            }
        }
    }
}
