using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class LiarBarCardMemento : MonoBehaviour
{
    Stack<LiarBarCard> _cards = new Stack<LiarBarCard>();

    int _cardCount;

    public void Save(List<LiarBarCard> cards)
    {
        _cardCount = cards.Count;
        for (int i = 0; i < _cardCount; i++)
            _cards.Push(cards[i]);
    }

    public bool Restore(Vector3 tablePosition)
    {
        float spacing = 0.2f;
        float zHeight = 0.2f;
        bool isTruth = IsTurnTruth();

        for (int i = 0; i < _cardCount; i++)
        {
            LiarBarCard card = _cards.Pop();

            // 가운데 기준으로 좌우 분배
            float xOffset = (i - (_cardCount - 1) / 2f) * spacing;
            Vector3 cardPosition = tablePosition + new Vector3(xOffset, 0f, zHeight);

            card.Flip(cardPosition);
        }
        return isTruth;
    }

    public void NewRound()
    {
        foreach (LiarBarCard card in _cards)
            PhotonNetwork.Destroy(card.gameObject);
        _cards.Clear();
    }

    public bool IsTurnTruth()
    {
        List<LiarBarCard> recentCards = _cards.Take(_cardCount).ToList();
        bool isTruth = recentCards.All(c => c.CardType == LiarBarCardManager.Instance.TargetCard || c.CardType == ELiarBarCardType.JokerCard);
        return isTruth;
    }
}