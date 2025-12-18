using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using ExitGames.Client.Photon;

public class SeotdaGameManager : MonoBehaviourPun
{
    //랭크 비교해서 가장 높은 애한테 상금 보내주기

    //상금 낼수있게 확인해서 걸기
    //타이머 체크 하기 (시간안에 아무것도 안누르면 자동 콜) => 돈 확인도해야됨

    // 턴 돌고 뭐눌렀는지도 관리해줘야되는데 + 상금 쌓이는것도 여기서 관리해줘야됨 그럼 돈 있나없나 체크도해야됨
    // 체크하고 카드매니저에 카드 다 서밋하고 발표까지 => 이거할때 카메라 다 수정해줘야되나
    // 타이머 흐르는거 체크해줘야됨

    private ESeotdaTurnMode _turnMode = ESeotdaTurnMode.PotMode;
    private int _pot = 0;
    private int _stake = 10;
    private int _callNum = 0;
    private int _curPlayer = 0;
    private int _curIndex = 0; //카드인덱스임
    private int _summitNum = 0;

    private bool _isUIOff = true;

    private bool _isAllIn = false;

    private SeotdaTurnManager _turn;
    private List<GameObject> _myCards;
    private List<GameObject> _summitCards = new List<GameObject>();


    private Vector3 _originScale = Vector3.zero;

    void Start()
    {
        _turn = GetComponent<SeotdaTurnManager>();
        _turnMode = ESeotdaTurnMode.PotMode;
        _myCards = GetComponent<SeotdaCardManager>().MyCards;
        _curPlayer = PhotonNetwork.PlayerList.Length;
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe("DiePlayer", DiePlayer);
        EventManager.Instance.Subscribe("EndTime", EndTime);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("DiePlayer", (Action)DiePlayer);
        EventManager.Instance.UnSubscribe("EndTime", (Action)EndTime);
    }
    private void Update()
    {
        if (_originScale == Vector3.zero && _myCards.Count > 0)
            _originScale = _myCards[0].transform.localScale;

        UpdateTurnMode();

        if (_turn.MyTurn && _isUIOff)
            OnUI();
        else if (!_turn.MyTurn && !_isUIOff)
            OffUI();

        if (_turn.MyTurn)
        {
            ClickButton();
        }

    }

    private void ResetGameManager()
    {
        ESeotdaTurnMode _turnMode = ESeotdaTurnMode.PotMode;
        _pot = 0;
        _stake = 10;
        _callNum = 0;
        _curPlayer = 0;
        _curIndex = 0; //카드인덱스임
        _summitNum = 0;

        _isUIOff = true;
        _isAllIn = false;

        _summitCards.Clear();

    }
    private void OnUI()
    {
        switch (_turnMode)
        {
            case ESeotdaTurnMode.PotMode:
                EventManager.Instance.Invoke("OnMoneyUI");
                break;
            case ESeotdaTurnMode.SummitMode:
                EventManager.Instance.Invoke("OnSummitUI");
                break;
            case ESeotdaTurnMode.EndGameMode:
                break;
        }
        _isUIOff = false;
        Debug.Log("켜짐");
    }
    private void OffUI()
    {
        if (_isUIOff) return;
        EventManager.Instance.Invoke("OffTurnUI");
        _isUIOff = true;
        Debug.Log("꺼짐");
    }
    private void DiePlayer()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        //알피씨로 보내기

        _curPlayer--;
        Debug.Log("플레이어 수 : " + _curPlayer);
        photonView.RPC("RPC_PlayerNumUpdate", RpcTarget.All, _curPlayer); // =>이거 어떻게해줄지 고민하기
    }

    private void UpdateTurnMode()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_callNum == _curPlayer)
        {
            photonView.RPC("RPC_UpdatePot", RpcTarget.All, _stake, _pot, 0, false);// 콜넘버 0으로 초기화
            _turnMode = ESeotdaTurnMode.SummitMode;
            photonView.RPC("RPC_UpdateTurnMode", RpcTarget.All, _turnMode);
        }
    }
    private void ClickButton()
    {
        //타이머 다차면 걍 보내야겠다
        switch (_turnMode)
        {
            case ESeotdaTurnMode.PotMode:

                PotMoney();
                break;
            case ESeotdaTurnMode.SummitMode:
                SummitCard();
                break;
        }

        //타이머 초기화해주기

    }
    private void PotMoney()
    {
        //돈 있나없나 체크
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            photonView.RPC("RPC_ChangeStake", RpcTarget.MasterClient, 0);
            Debug.Log("콜함");

        }
        else if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            _turn.Die();
            return;
        }
        else if (Keyboard.current.bKey.wasPressedThisFrame && !_isAllIn)
        {
            photonView.RPC("RPC_ChangeStake", RpcTarget.MasterClient, 2);
        }
        else if (Keyboard.current.hKey.wasPressedThisFrame && !_isAllIn)
        {
            photonView.RPC("RPC_ChangeStake", RpcTarget.MasterClient, 1);
        }
        else if (Keyboard.current.hKey.wasPressedThisFrame && !_isAllIn)
        {
            photonView.RPC("RPC_ChangeStake", RpcTarget.MasterClient, 3);
        }
        else
            return;

        _turn.NextTurn();
    }
    private void SummitCard()
    {

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            _myCards[_curIndex].transform.localScale = _originScale;
            if (_curIndex == 0)
                _curIndex = 2;
            else
                _curIndex--;
            _myCards[_curIndex].transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            _myCards[_curIndex].transform.localScale = _originScale;
            if (_curIndex == 2)
                _curIndex = 0;
            else
                _curIndex++;
            _myCards[_curIndex].transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            bool isIn = false;
            for (int i = 0; i < _summitCards.Count; i++)
            {
                if (_summitCards[i] == _myCards[_curIndex])
                {
                    isIn = true;
                    break;
                }
            }
            if (isIn)
            {
                _summitCards.Remove(_myCards[_curIndex]);
                _myCards[_curIndex].transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            }
            else
                _summitCards.Add(_myCards[_curIndex]);

        }
        for (int i = 0; i < _summitCards.Count; i++)
        {
            _summitCards[i].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
        if (Keyboard.current.fKey.wasPressedThisFrame && _summitCards.Count == 2)
        {
            //서밋한 리스트 넘기기 
            //카드 들고있는거에서 빼줄까 좀 고민되네
            photonView.RPC("RPC_SummitCard", RpcTarget.MasterClient, MyPlayer.local.PositionIndex, _summitCards[0].GetComponent<FlowerCard>().Name,
                _summitCards[1].GetComponent<FlowerCard>().Name);

            _curIndex = 0;
            CheckEndGame();
        }
    }

    private void EndTime()
    {
        switch (_turnMode)
        {
            case ESeotdaTurnMode.PotMode:
                AutoPotMoney();
                break;
            case ESeotdaTurnMode.SummitMode:
                AutoSummit();
                break;
        }
    }
    private void AutoPotMoney()
    {
        _turn.Die();
    }
    private void AutoSummit()
    {
        _summitCards.Add(_myCards[0]);
        _summitCards.Add(_myCards[1]);

        //카드넘기기 마스터 클라이언트에서 해주기~
        _summitNum++;
        CheckEndGame();
    }
    private void CheckEndGame()
    {
        photonView.RPC("RPC_UpdateSummitNum", RpcTarget.All, _summitNum + 1);

        if (_summitNum == _curPlayer)
        {//게임멈추기
            photonView.RPC("RPC_CheckEndGame", RpcTarget.MasterClient);
        }

        else
        {
            _turn.NextTurn();
        }

    }

    #region RPC
    [PunRPC]
    private void RPC_PlayerNumUpdate(int num)
    {
        _curPlayer = num;
        Debug.Log("플레이어 수 :" + num);
    }
    [PunRPC]
    private void RPC_UpdateTurnMode(ESeotdaTurnMode turnMode)
    {
        _turnMode = turnMode;
        Debug.Log("모드 : " + turnMode);
    }
    [PunRPC]
    private void RPC_UpdateSummitNum(int num)
    {
        _summitNum = num;
        Debug.Log("서밋 수 : " + num);
    }
    [PunRPC]
    private void RPC_UpdatePot(int stake, int pot, int call, bool isAllIn)
    {
        _stake = stake;
        Debug.Log("판돈 : " + _stake);
        _pot = pot;
        Debug.Log("모인 돈 :" + _pot);
        _callNum = call;
        Debug.Log("콜 수 :" + _callNum);
        _isAllIn = isAllIn;
    }
    [PunRPC]
    private void RPC_ChangeStake(int num)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        switch (num)
        {
            case 0:
                _callNum++;
                break;
            case 1: // 하프
                _stake = _stake + (int)(_stake * 0.5f);
                _callNum = 1;
                break;
            case 2: //더블
                _stake *= 2;
                _callNum = 1;
                break;
            case 3: //올인
                //_stake = mymoney; 데이터 가져와야됨
                _isAllIn = true;
                break;
        }

        photonView.RPC("RPC_UpdatePot", RpcTarget.All, _stake, _pot + _stake, _callNum, _isAllIn);
    }
    [PunRPC]
    private void RPC_CheckEndGame()
    {
        int winner;
        List<(SeotdaData data, int index)> list;
        GetComponent<SeotdaCardManager>().GetWinner(out winner, out list);

        Debug.Log("게임 종료");
        Debug.Log("위너 : " + winner);
        Debug.Log("리스트 데이터: " + list[0].data.name);
        Debug.Log("리스트 인덱스: " + list[0].index);

        int[] types = new int[list.Count];
        int[] indexes = new int[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            types[i] = (int)list[i].data.type;
            indexes[i] = list[i].index;
        }

        // RPC 호출
        photonView.RPC("RPC_OnResultPanel", RpcTarget.MasterClient, types, indexes);
    }
    [PunRPC]
    private void RPC_ResetGameManager()
    {
        ResetGameManager();
        _originScale = Vector3.zero;
    }

    #endregion
}
