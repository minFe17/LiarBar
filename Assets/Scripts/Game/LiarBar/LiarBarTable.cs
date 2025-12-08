using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class LiarBarTable : MonoBehaviourPun
{
    public static LiarBarTable Instance { get; private set; }

    [SerializeField] Transform _tableCenter;

    LiarBarCardMemento _memento = new LiarBarCardMemento();

    int _index = 0;

    public void SavePlayedCards(List<LiarBarCard> cards) => _memento.Save(cards);
    public void NewRound() => _index = 0;

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

    public void CheckLiar()
    {
        _memento.Restore();
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
}