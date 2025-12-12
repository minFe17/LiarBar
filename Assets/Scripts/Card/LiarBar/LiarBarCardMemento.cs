using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class LiarBarCardMemento : MonoBehaviour
{
    Stack<LiarBarCard> _cards = new Stack<LiarBarCard>();

    int _cardCount;

    public Stack<LiarBarCard> Cards { get => _cards; }

    public void Save(List<LiarBarCard> cards)
    {
        _cardCount = cards.Count;
        for (int i = 0; i < _cardCount; i++)
            _cards.Push(cards[i]);
    }

    public void Restore(Vector3 tablePosition, Action<bool> onAllFlipped)
    {
        float spacing = 0.2f;
        float zHeight = 0.2f;
        bool isTruth = IsTurnTruth();

        int flippedCount = 0;

        for (int i = 0; i < _cardCount; i++)
        {
            LiarBarCard card = _cards.Pop();
            float xOffset = (i - (_cardCount - 1) / 2f) * spacing;
            Vector3 cardPosition = tablePosition + new Vector3(xOffset, 0f, zHeight);

            // Flip 시작 (모든 카드 동시에)
            card.Flip(cardPosition, () =>
            {
                flippedCount++;
                if (flippedCount == _cardCount)
                    onAllFlipped?.Invoke(isTruth); // 모든 카드 뒤집힌 후 호출
            });
        }
    }

    public bool IsTurnTruth()
    {
        List<LiarBarCard> recentCards = _cards.Take(_cardCount).ToList();
        bool isTruth = recentCards.All(c => c.CardType == LiarBarCardManager.Instance.TargetCard || c.CardType == ELiarBarCardType.JokerCard);
        return isTruth;
    }

    public void DestroyCard(LiarBarCard card)
    {
        PhotonNetwork.Destroy(card.gameObject);
    }
}