using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.InputSystem;
using Utils;

public class SeotdaCardManager : MonoBehaviourPun
{
    [SerializeField]
    private FlowerCardDB _flowerCardDB;

    private List<SeotdaData> _data;
    private Dictionary<string, GameObject> _cardDic = new Dictionary<string, GameObject>();
    private Queue<string> _cards = new Queue<string>();
    private List<GameObject> _myCards = new List<GameObject>();

    public List<GameObject> MyCards
        { get { return _myCards; } }

    public SeotdaData? FindData(FlowerCard card1, FlowerCard card2)
    {
        List<SeotdaData> list = DataManager.Instance.MatchData(card1.Month, card2.Month);

        if(list.Count>0)
        {
            for(int i = 0; i<list.Count;i++)
            {
                if (list[i].condition == ESeotdaCondition.None)
                    return list[i];
                if (list[i].condition == DataManager.Instance.GetCondition(card1,card2))
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
        _myCards.Clear();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
         {
              { "IsAlive", true }
         });

        if (!PhotonNetwork.IsMasterClient)
            return;   
        MixCards();
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
                if (!isAlive) continue;

            string c1 = GetCardName();
            photonView.RPC("RPC_ReceiveCard", player, c1);
        }
    }

    [PunRPC]
    public void RPC_ReceiveCard(string c1)
    {
        _myCards.Add(_cardDic[c1]);
        SortAscending();
    }
}
//카드 나눠주기, 카드 순위확인해서 개인별로 등수매기기