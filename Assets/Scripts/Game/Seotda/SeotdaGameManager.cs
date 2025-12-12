using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SeotdaGameManager : MonoBehaviour
{
    // 턴 도는걸 여기서 관리해줘야되나?
    // 턴 돌고 뭐눌렀는지도 관리해줘야되는데 + 상금 쌓이는것도 여기서 관리해줘야됨 그럼 돈 있나없나 체크도해야됨
    // 아 누가 죽어서 게임 끝났는지 아닌지를 여기서 체크해야겠네
    // 그 연속 콜수가 살아있는 인원 -1 만큼이면 다음단계하면됨
    // 체크하고 카드매니저에 카드 다 서밋하고 발표까지 => 이거할때 카메라 다 수정해줘야되나
    // 타이머 흐르는거 체크해줘야됨
    // 이것도 rpc로 만들자

    private PhotonView _view;
    private bool _myTurn = false;

    private ESeotdaTurnMode _turnMode;
    private int _pot = 0;
    private int _callNum = 0;
    private int _curIndex;

    private GamePlayer _player;
    private Player _myPlayer;

    private List<GameObject> _myCards;
    private List<GameObject> _summitCards;

    private Vector3 _originScale = Vector3.zero;

    void Start()
    {
        _view = GetComponent<PhotonView>();
        _myPlayer = MyPlayer.myPlayer;
        _turnMode = ESeotdaTurnMode.PotMode;
        _myCards = GetComponent<SeotdaCardManager>().MyCards;
    }

    // Update is called once per frame
    void Update()
    {
        CheckTurn();
        if (_view.IsMine && _myTurn)
        {
            ClickButton();
        }
        
    }
    private void CheckTurn()
    {
        if(_originScale==Vector3.zero && _myCards.Count>0)
            _originScale = _myCards[0].transform.localScale;
        if (_myTurn || !_view.IsMine) return;
        if (TurnManager.Instance.Players[TurnManager.Instance.CurrentPlayerIndex].TurnIndex == (int)_myPlayer.CustomProperties["PositionIndex"])
        { 
            _myTurn = true;
            if(_player == null)
                _player = TurnManager.Instance.Players[TurnManager.Instance.CurrentPlayerIndex];
        }
    }
    private void ClickButton()
    {
        //타이머 다차면 걍 보내야겠다
        switch (_turnMode)
        {
            case ESeotdaTurnMode.PotMode:
                if(!PotMoney())
                    return;
                break;
            case ESeotdaTurnMode.SummitMode:
                if (!SummitCard())
                    return;
                break;
        }

        //타이머 초기화해주기
        TurnManager.Instance.EndTurn();    
    }
    private bool PotMoney()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            _callNum++;
            //돈 보내는거 알피씨로 보내기

        }
        else if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            //죽음처리하기(어케하지) 아! 애니메이션으로
            TurnManager.Instance.DiePlayer(_player);
        }
        else if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            //돈 더블로 보내는거 알피씨로 보내기
        }
        else if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            //돈 하프로 보내기
        }
        else 
            return false;

        return true;

    }
    private bool SummitCard()
    {
        
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            _myCards[_curIndex].transform.localScale = _originScale;
            if (_curIndex == 0)
                _curIndex = 2;
            else
                _curIndex--;
            _myCards[_curIndex].transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            _myCards[_curIndex].transform.localScale = _originScale;
            if (_curIndex == 2)
                _curIndex = 0;
            else
                _curIndex++;
            _myCards[_curIndex].transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
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
        for(int i = 0;i< _summitCards.Count;i++)
        {
            _summitCards[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        if (Keyboard.current.fKey.wasPressedThisFrame && _summitCards.Count == 2)
        {
            //서밋한 리스트 넘기기 

            _curIndex = 0;
            return true;
        }
        return false;
    }
}
