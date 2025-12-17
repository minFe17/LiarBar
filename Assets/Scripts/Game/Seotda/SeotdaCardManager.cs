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

    private List<SeotdaData> _data;
    private Dictionary<string, GameObject> _cardDic = new Dictionary<string, GameObject>();
    private Queue<string> _cards = new Queue<string>();
    private List<GameObject> _myCards = new List<GameObject>();
    private List<SeotdaData?> _allCards = new List<SeotdaData?>();
    private bool _isStart = false;
    private int _winner = -1;

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
    private void Awake()
    {
        DataManager.Instance.LoadData();
        _data = DataManager.Instance.Data;
        SetDictionary();

    }
    private void Start()
    {
        SetCardGame(); //이거 GameManager같은걸로 빼서 관리하면될듯 이벤트이용해서 
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
            SplitCard();
        }
        if (Keyboard.current.f12Key.wasPressedThisFrame && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_ResetCards", RpcTarget.All);
        }
    }
    private void ClearSummitCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _allCards.Clear();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            _allCards.Add(null);
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

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
         {
              { "IsAlive", true }
         });

        if (!PhotonNetwork.IsMasterClient)
            return;   
        MixCards();
        ClearSummitCards();
        _winner = -1;
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
    private void GetWinner()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        int rank = 0;
        bool isDdang = false;
        bool isGwangDdang = false;
        int amhang = -1;
        int ddangjap = -1;
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++)
        {
            if (_allCards == null) continue;
            if (_allCards[i].Value.type == ESeotdaRuleType.Amhangusa)
                amhang = i;
            else if (_allCards[i].Value.type == ESeotdaRuleType.Ddangjapyee)
                ddangjap = i;
            else if (_allCards[i].Value.rank > rank)
            {
                rank = _allCards[i].Value.rank;
                _winner = i;
            }
            if (_allCards[i].Value.type == ESeotdaRuleType.Ddang)
                isDdang = true;
            if (_allCards[i].Value.type == ESeotdaRuleType.GwangDdang)
                isGwangDdang = true;
        }

        //if (rank == 4 || rank == 6)
        //살아있는사람끼리 재시작해야됨
        
        if (isGwangDdang && amhang >= 0 && rank != 1)
            _winner =  amhang;
        else if (isDdang && ddangjap >= 0 && rank != 1)
            _winner =  ddangjap;


        //위너 찾아서 보내자! rpc로 
        //++ 내가 뭐냈고, 몇등인지 다른애들 다 합쳐서 보여주자!
        //오브젝트 배열로 보내야됨
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
                    EventManager.Instance.Invoke("AddCard");
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

        _allCards[positionIndex] = FindData(_cardDic[c1].GetComponent<FlowerCard>(), _cardDic[c2].GetComponent<FlowerCard>());
        Debug.Log("제출");
    }
    [PunRPC]
    private void RPC_ResetCards()
    {
        SetCardGame();
    }
    [PunRPC]
    private void RPC_FindWinner()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GetWinner();
    }

    #endregion
}
//카드 나눠주기, 카드 순위확인해서 개인별로 등수매기기