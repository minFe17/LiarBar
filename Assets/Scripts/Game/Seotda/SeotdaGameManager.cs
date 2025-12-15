using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using ExitGames.Client.Photon;

public class SeotdaGameManager : MonoBehaviourPun
{
    // 마스터클라이언트이용해서 중앙집중형으로 바꾸기 
    // 카드 제출한거 없애고, 나머지 다 제출하면 카메라 한명씩 비추면서 보여주기(연출)

    //랭크 비교해서 가장 높은 애한테 상금 보내주기

    //상금 낼수있게 확인해서 걸기

    //타이머 체크 하기 (시간안에 아무것도 안누르면 자동 콜) => 돈 확인도해야됨

    // 턴 돌고 뭐눌렀는지도 관리해줘야되는데 + 상금 쌓이는것도 여기서 관리해줘야됨 그럼 돈 있나없나 체크도해야됨
    // 체크하고 카드매니저에 카드 다 서밋하고 발표까지 => 이거할때 카메라 다 수정해줘야되나
    // 타이머 흐르는거 체크해줘야됨


    private PhotonView _view;

    private ESeotdaTurnMode _turnMode = ESeotdaTurnMode.PotMode;
    private int _pot = 0;
    private int _callNum = 0;
    private int _curPlayer = 0;
    private int _curIndex = 0; //카드인덱스임

    private SeotdaTurnManager _turn;
    private Player _myPlayer;

    private List<GameObject> _myCards;
    private List<GameObject> _summitCards = new List<GameObject>();

    private Vector3 _originScale = Vector3.zero;

    void Start()
    {
        _turn = GetComponent<SeotdaTurnManager>();
        _view = GetComponent<PhotonView>();
        _myPlayer = MyPlayer.myPlayer;
        _turnMode = ESeotdaTurnMode.PotMode;
        _myCards = GetComponent<SeotdaCardManager>().MyCards;
        _curPlayer = PhotonNetwork.PlayerList.Length;
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe("DiePlayer", DiePlayer);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("DiePlayer", (Action)DiePlayer);
    }
    private void Update()
    {
        if (_originScale == Vector3.zero && _myCards.Count > 0)
            _originScale = _myCards[0].transform.localScale;

        UpdateTurnMode();

        if (_turn.MyTurn)
        {
            ClickButton();
        }

    }
    private void DiePlayer()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        //알피씨로 보내기
        _curPlayer--;
        Debug.Log("플레이어 수 : " + _curPlayer);
        photonView.RPC("RPC_PlayerNumUpdate", RpcTarget.All, _curPlayer);
    }

    private void UpdateTurnMode()
    {
         if (_callNum == _curPlayer)
         {
             photonView.RPC("RPC_CallNumUpdate", RpcTarget.All, 0);// 콜넘버 0으로 초기화
             _turnMode = ESeotdaTurnMode.SummitMode;
             photonView.RPC("RPC_UpdateTurnMode",RpcTarget.All, _turnMode);
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
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            _callNum++;
            Debug.Log("콜함");
            //돈 보내는거 알피씨로 보내기
        }
        else if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            //죽음처리하기(어케하지) 아! 애니메이션으로
            _turn.Die();
            return;
        }
        else if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            //돈 더블로 보내는거 알피씨로 보내기
            _callNum = 1;

        }
        else if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            //돈 하프로 보내기
            _callNum = 1;
        }
        else
            return;
        photonView.RPC("RPC_CallNumUpdate", RpcTarget.All, _callNum);
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
                _summitCards.Remove(_myCards[_curIndex]);
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
            List<GameObject> list = _summitCards;
            _curIndex = 0;
            _turn.NextTurn();
        }
    }
    [PunRPC]
    private void RPC_PlayerNumUpdate(int num)
    {
        _curPlayer = num;
        Debug.Log("플레이어 수 :" + num);
    }
    [PunRPC]
    private void RPC_CallNumUpdate(int num)
    {
        _callNum = num;
        Debug.Log("콜 수 : " + num);
    }
    [PunRPC]
    private void RPC_UpdateTurnMode(ESeotdaTurnMode turnMode)
    {
        _turnMode = turnMode;
        Debug.Log("모드 : " + turnMode);
    }
}
