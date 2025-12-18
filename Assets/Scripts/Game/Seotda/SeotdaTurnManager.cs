using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;


public class SeotdaTurnManager : MonoBehaviourPun
{
    //여기서 플레이어 alive상태 초기화해주면되겠다

    private Player[] _playerList = new Player[4];
    private int _curIndex = 0;
    private PhotonView _view;
    private bool _myTurn = false;


    public bool MyTurn
    {
        get { return _myTurn; }
    }    

    public int Turn
    {
        get { return _curIndex; }
    }
    public void Die()
    { 
        if((bool)MyPlayer.myPlayer.CustomProperties["IsAlive"] && _myTurn)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
         {
              { "IsAlive", false }
         });
            EventManager.Instance.Invoke("DiePlayer");
            //마스터 클라이언트한테 턴 넘긴거 보내기
            photonView.RPC("RPC_ChangeTurn", RpcTarget.MasterClient, _curIndex);

            Debug.Log("죽음");
        }
    }
    public void NextTurn()
    {
        if (_myTurn) 
        {
            //마스터 클라이언트한테 턴 넘긴거 보내기
            photonView.RPC("RPC_ChangeTurn", RpcTarget.MasterClient, _curIndex);
            Debug.Log("턴 넘어감");
        }
    }
    public void EndGame()
    {
        photonView.RPC("RPC_EndGame", RpcTarget.All);
    }
    public void StartGame()
    {
        photonView.RPC("RPC_Start", RpcTarget.All);
    }
    private void Start()
    {
        _view = GetComponent<PhotonView>();
        SetPlayerList();
        StartGame();
    }
    private void SetPlayerList()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            int index = (int)players[i].CustomProperties["PositionIndex"];
            _playerList[index] = players[i];
        }
    }
    [PunRPC]
    private void RPC_ChangeTurn(int index)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        AddTurn();

        while(true)
        {
            var props = _playerList[_curIndex].CustomProperties;

            if (!props.TryGetValue("IsAlive", out object aliveObj))
                break;
            if (!(bool)_playerList[_curIndex].CustomProperties["IsAlive"])
            {
                AddTurn();
                if (index == _curIndex) //게임 끝 추가해주기
                {
                    photonView.RPC("RPC_EndGame", RpcTarget.All); //이건 다죽었을때 기준임 All부분 빼주기
                }
            }
            else
                break;
        }
        foreach (Player player in _playerList)
        {
            photonView.RPC("RPC_UpdateTurn", player, _curIndex);
        }
    }
    [PunRPC]
    private void RPC_EndGame()
    {
        //이벤트 발생시켜주기
    }
    [PunRPC]
    private void RPC_Start()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
         {
              { "IsAlive", true }
         });
        _myTurn = false;
        if (!PhotonNetwork.IsMasterClient) return;
        foreach (Player player in _playerList)
        {
            photonView.RPC("RPC_UpdateTurn", player, MyPlayer.local.PositionIndex);
        }
        
    }
    [PunRPC]
    private void RPC_UpdateTurn(int index)
    {
        Debug.Log(MyPlayer.local.PositionIndex);
        if (index == MyPlayer.local.PositionIndex)
            _myTurn = true;
        else
            _myTurn = false;

        Debug.Log(index);
        Debug.Log(_myTurn);

        _curIndex = index;
    }
    private void AddTurn()
    {
        if (_curIndex == 3)
            _curIndex = 0;
        else _curIndex++;

        Debug.Log(_curIndex+ " 생존 ? "+ _playerList[_curIndex].CustomProperties["IsAlive"]);
    }
}
