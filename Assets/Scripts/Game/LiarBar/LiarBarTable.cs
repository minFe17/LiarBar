using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class LiarBarTable : MonoBehaviourPun
{
    public static LiarBarTable Instance { get; private set; }

    [SerializeField] Transform _tableCenter;

    LiarBarCardMemento _memento = new LiarBarCardMemento();

    int _index = 0;

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
        if (isTruth)
            Debug.Log("모두 초록: 라이어 말한 사람 포션 적용");
        else
            Debug.Log("빨간 카드 있음: 카드 낸 사람 포션 적용");
        //photonView.RPC("RPC_ApplyTurnResult", RpcTarget.All, isTruth);
    }

    public void SavePlayedCards(List<LiarBarCard> cards)
    {
        int[] cardIds = cards.Select(c => c.photonView.ViewID).ToArray();
        photonView.RPC("RPC_SavePlayedCards", RpcTarget.MasterClient, cardIds);
    }

    public void CheckLiar()
    {
        if (!PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_CheckLiar", RpcTarget.MasterClient);
        else
            RPC_CheckLiar();
    }

    public Vector3 GetCenterPosition()
    {
        Vector3 position = _tableCenter.position;
        position.y += (_index * 0.01f);
        _index++;
        photonView.RPC("RPC_AddIndex", RpcTarget.Others, _index);
        return position;
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
    public void RPC_CheckLiar()
    {
        bool isTruth = _memento.Restore(_tableCenter.position);
        OnTurnEvaluated(isTruth);
        _index = 0;
    }

    //[PunRPC]
    //public void RPC_ApplyTurnResult(bool isTruth)
    //{

    //}
}