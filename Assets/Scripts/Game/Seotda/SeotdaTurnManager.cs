using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;


public class SeotdaTurnManager : MonoBehaviourPun
{
    //여기서 플레이어 alive상태 초기화해주면되겠다

    private List<Player> _playerList;
    private int _curIndex = 0;
    private PhotonView _view;
    private bool _myTurn = false;


    public bool MyTurn
    {
        get { return _myTurn; }
    }    

    public void Die()
    { 
        if(_view.Owner == MyPlayer.myPlayer && (bool)MyPlayer.myPlayer.CustomProperties["IsAlive"])
        {
            //마스터 클라이언트한테 턴 넘긴거 보내기

        }
    }
    public void NextTurn()
    {
        if (_view.Owner == MyPlayer.myPlayer) //이거보낼때 마이턴인 상황인지아닌지 판단해서 보내기
        {
            //마스터 클라이언트한테 턴 넘긴거 보내기

        }
    }

    private void Start()
    {
        _view = GetComponent<PhotonView>();
        SetPlayerList();
    }
    private void SetPlayerList()
    {
       // _playerList = PhotonNetwork.PlayerList;
       //  플레이어리스트 phositionindex 값으로 재정렬해야됨 오름차순으로.
    }
    private void RPC_ChangeTurn()
    {
        if (PhotonNetwork.IsMasterClient) return;
        AddTurn();

        while(true)
        {
            if (!(bool)PhotonNetwork.PlayerList[_curIndex].CustomProperties["IsAlive"])
            {
                AddTurn();
            }
            else
                break;
        }
        //RPC_~~로 각각  curIndex 보내주기 (하면서 마이턴 맞나아니나 수정)
    }
    private void RPC_UpdateTurn(int index)
    {
        if (index == (int)MyPlayer.myPlayer.CustomProperties["PositionIndex"])
            _myTurn = true;
        else
            _myTurn = false;

        _curIndex = index;
    }
    private void AddTurn()
    {
        if (_curIndex == 3)
            _curIndex = 0;
        else _curIndex++;
    }
}
