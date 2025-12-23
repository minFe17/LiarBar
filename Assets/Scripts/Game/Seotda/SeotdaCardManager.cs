using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.InputSystem;
using Utils;
using UnityEngine.UI;
using Unity.VisualScripting;

public class SeotdaCardManager : MonoBehaviourPun
{
    [SerializeField]
    private FlowerCardDB _flowerCardDB;

    private Dictionary<string, GameObject> _cardDic = new Dictionary<string, GameObject>();
    private Queue<string> _cards = new Queue<string>();
    private List<GameObject> _myCards = new List<GameObject>();
    private List<(SeotdaData? data, int month1, int month2)> _allCards = new List<(SeotdaData? data, int month1, int month2)> ();
    private List<SeotdaData> _data; //들고있을필요있나?
    private bool _isStart = false;

    public List<GameObject> MyCards
        { get { return _myCards; } }

    public SeotdaData? FindData(FlowerCard card1, FlowerCard card2)
    {
        List<SeotdaData> list = DataManager.Instance.MatchData(card1.Month, card2.Month);

        
        if(list.Count>0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].condition != ESeotdaCondition.None &&
                    list[i].condition == DataManager.Instance.GetCondition(card1, card2))
                    return list[i];
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].condition == ESeotdaCondition.None)
                    return list[i];
            }

        }
        if ((card1.Month + card2.Month) % 10 == 0)
        {
            return DataManager.Instance.Mangtong;
        }

        return DataManager.Instance.Keut;
    }
    public int FindMonth(int index)
    {
        int month = _allCards[index].month1+ _allCards[index].month2;
        if (_allCards[index].data.Value.type == ESeotdaRuleType.Keut)
            return month - 10;
        else if (_allCards[index].data.Value.type == ESeotdaRuleType.Ddang)
            return (int)(month * 0.5f);

        return -1;
    }
    public void GetWinner(out int winner, out List<(SeotdaData data, int index)>list)
    {
        list = new List<(SeotdaData data, int index)>();
        winner = 0;
        if (!PhotonNetwork.IsMasterClient) return;

        bool isDdang = false;
        bool isGwangDdang = false;
        int amhang = -1;
        int ddangjap = -1;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (_allCards[i].data == null) continue;
            list.Add((_allCards[i].data.Value, i)); //i는 포지션인덱스임
            SortingList(list);
            if (_allCards[i].data.Value.type == ESeotdaRuleType.Amhangusa)
                amhang = i;
            else if (_allCards[i].data.Value.type == ESeotdaRuleType.Ddangjapyee)
                ddangjap = i;
            if (_allCards[i].data.Value.type == ESeotdaRuleType.Ddang)
                isDdang = true;
            if (_allCards[i].data.Value.type == ESeotdaRuleType.GwangDdang)
                isGwangDdang = true;
        }



        if (isGwangDdang && amhang >= 0 && list[0].data.rank != 1)
            winner = amhang;
        else if (isDdang && ddangjap >= 0 && list[0].data.rank < 3)
            winner = ddangjap;
        else if (list[0].data.rank == 4 || list[0].data.rank == 6)
        {
            winner = -1;
            //재시작 rpc 시작하기
        }


        //++ 내가 뭐냈고, 몇등인지 다른애들 다 합쳐서 보여주자!
        //오브젝트 배열로 보내야됨
    }

    private void Awake()
    {
        DataManager.Instance.LoadData();
        _data = DataManager.Instance.Data;
        SetDictionary();

    }
    private void Start()
    {
        //SetCardGame(); //이거 GameManager같은걸로 빼서 관리하면될듯 이벤트이용해서 
        _isStart = true;
        //SplitCard();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe("SetSeotdaGame", SetCardGame);
        EventManager.Instance.Subscribe("SplitCard", SplitCard);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("SetSeotdaGame", (Action)SetCardGame);
        EventManager.Instance.UnSubscribe("SplitCard", (Action)SplitCard);
    }

    private void Update()
    {
        
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            EventManager.Instance.Invoke("OnSplit");
            //SplitCard();
        }
        if (Keyboard.current.f12Key.wasPressedThisFrame && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_ResetCards", RpcTarget.All);
        }
    }
    private void SortingList(List<(SeotdaData data, int index)> list)
    {
        list.Sort((a, b) =>
        {
            int rankCompare = a.data.rank.CompareTo(b.data.rank);
            if (rankCompare != 0)
                return rankCompare;

            // rank가 같을 때만 여기로 옴
            int aScore = CalcScore(a.data,a.index);
            int bScore = CalcScore(b.data,b.index);

            return bScore.CompareTo(aScore); // 높은 값이 앞으로 오게
        });
    }
    private int CalcScore(SeotdaData data, int index)
    {
        int month = _allCards[index].month1 + _allCards[index].month2;
        switch (data.type)
        {
            case ESeotdaRuleType.Ddang:
                return (int)(month * 0.5f);

            case ESeotdaRuleType.Keut:
                return month<10 ? month : month -10;
        }
        //둘 재대결하게해야됨
        return -1;

    }
    private void ClearSummitCards()
    {
        _allCards.Clear();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            _allCards.Add((null,0,0));
        }
    }
    private void SetDictionary()
    {
        foreach(GameObject card in _flowerCardDB.FlowerCardPrefabs)
        {
            _cardDic[card.name] = card;
        }
    }
    private void SetCardGame()
    {
        if(_isStart)
        {
            EventManager.Instance.Invoke("ResetShowCards");
            EventManager.Instance.Invoke("ResetRuleUI");
        }

        _myCards.Clear();


        if (!PhotonNetwork.IsMasterClient)
            return;   
        MixCards();
        ClearSummitCards();
    }

    private void MixCards()
    {
        _cards.Clear();

        for (int i=1;i<=10;i++)
        {
            _cards.Enqueue(i + "_1");
            _cards.Enqueue(i + "_2");
        }

        _cards =  Shuffle(_cards);
    }
    private Queue<string> Shuffle(Queue<string> queue)
    {
        List<string> list = new List<string>(queue);

        // Fisher-Yates Shuffle
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }

        return new Queue<string>(list);
    }

    private string GetCardName()
    {
        return _cards.Dequeue();
    }
   
    private void SortAscending()
    {
        // _myCards.Sort((a, b) =>
        // a.GetComponent<FlowerCard>().Month.CompareTo(
        // b.GetComponent<FlowerCard>().Month));
        // => 소팅하면, 카드 프리팹불러올때 버그발생함

        for (int i=0;i<_myCards.Count;i++)
        {
            Debug.Log(_myCards[i].name);
        }
        EventManager.Instance.Invoke("AddCard");
    }

    private void SplitCard()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log("split");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            object obj = player.CustomProperties["IsAlive"];
            if (obj is bool isAlive)
                if (!isAlive)
                {
                    //EventManager.Instance.Invoke("AddCard");
                    continue;
                }

            string c1 = GetCardName();
            photonView.RPC("RPC_ReceiveCard", player, c1);
        }
    }

    #region RPC
    [PunRPC]
    public void RPC_ReceiveCard(string c1)
    {
        _myCards.Add(_cardDic[c1]);
        SortAscending();
    }

    [PunRPC]
    private void RPC_SummitCard(int positionIndex, string c1, string c2)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _allCards[positionIndex] = (FindData(_cardDic[c1].GetComponent<FlowerCard>(), _cardDic[c2].GetComponent<FlowerCard>()),
            _cardDic[c1].GetComponent<FlowerCard>().Month, _cardDic[c2].GetComponent<FlowerCard>().Month);
        Debug.Log("제출");
    }
    [PunRPC]
    private void RPC_ResetCards()
    {
        SetCardGame();
    }


    #endregion
}
//카드 나눠주기, 카드 순위확인해서 개인별로 등수매기기