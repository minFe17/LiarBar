using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class LiarBarTable : MonoBehaviourPun
{
    public static LiarBarTable Instance { get; private set; }

    [SerializeField] Transform _tableCenter;
    [SerializeField] LiarBarTurnUI _turnUI;

    LiarBarCardMemento _memento = new LiarBarCardMemento();
    GamePlayer _callLiarPlayer;
    int _index = 0;

    public LiarBarTurnUI TurnUI { get => _turnUI; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void OnTurnEvaluated(bool isTruth)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        TurnManager.Instance.BeginResolveLiar();

        if (isTruth)
        {
            // 라이어 선언한 사람이 틀렸으므로 포션 마심
            _callLiarPlayer.photonView.RPC("RPC_DoDrinkPotion", _callLiarPlayer.photonView.Owner);
        }
        else
        {
            // 실제로 라이어였으므로 마지막으로 카드 낸 사람이 포션 마심
            int index = TurnManager.Instance.CurrentPlayerIndex - 1;
            if (index < 0)
                index = TurnManager.Instance.Players.Count - 1;

            GamePlayer lastPlayer = TurnManager.Instance.Players[index];
            lastPlayer.photonView.RPC("RPC_DoDrinkPotion", lastPlayer.photonView.Owner);
        }
    }

    public void SavePlayedCards(List<LiarBarCard> cards)
    {
        int[] cardIds = cards.Select(c => c.photonView.ViewID).ToArray();
        photonView.RPC("RPC_SavePlayedCards", RpcTarget.MasterClient, cardIds);
    }

    public void CheckLiar(int viewId)
    {
        if (!PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_CheckLiar", RpcTarget.MasterClient, viewId);
        else
            RPC_CheckLiar(viewId);
    }

    public Vector3 GetCenterPosition()
    {
        Vector3 position = _tableCenter.position;
        position.y += (_index * 0.01f);
        _index++;
        photonView.RPC("RPC_AddIndex", RpcTarget.Others, _index);
        return position;
    }

    public void NewRound()
    {
        photonView.RPC("RPC_NewRound", RpcTarget.MasterClient);
    }

    IEnumerator NewRoundRoutine()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        // 카드 제거
        while (_memento.Cards.Count > 0)
        {
            LiarBarCard card = _memento.Cards.Pop();
            _memento.DestroyCard(card);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // 새 카드 분배 및 게임 재개
        LiarBarCardManager.Instance.SetTable();
        TurnManager.Instance.ContinueGame();
    }

    [PunRPC]
    public void RPC_AddIndex(int index)
    {
        _index = index;
    }

    [PunRPC]
    void RPC_SavePlayedCards(int[] cardIds)
    {
        List<LiarBarCard> cards = cardIds.Select(id => PhotonView.Find(id).GetComponent<LiarBarCard>()).ToList();
        _memento.Save(cards);
    }

    [PunRPC]
    void RPC_CheckLiar(int viewID)
    {
        _callLiarPlayer = TurnManager.Instance.Players.FirstOrDefault(p => p.ViewID == viewID);
        if (_callLiarPlayer == null)
            return;

        _memento.Restore(_tableCenter.position, (isTruth) => { OnTurnEvaluated(isTruth); });
        _index = 0;
    }

    [PunRPC]
    void RPC_NewRound()
    {
        StartCoroutine(NewRoundRoutine());
        _index = 0;
    }
}