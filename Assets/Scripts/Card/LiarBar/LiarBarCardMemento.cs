using System.Collections.Generic;
using UnityEngine;

public class LiarBarCardMemento : MonoBehaviour
{
    List<LiarBarCard> _currentTurnCards = new List<LiarBarCard>();

    public void Save(List<LiarBarCard> cards)
    {
        ClearTurn();
        _currentTurnCards.AddRange(cards);
    }

    public void Restore()
    {
        //foreach (LiarBarCard card in _currentTurnCards)
        //    card.
        ClearTurn();
    }

    private void ClearTurn()
    {
        _currentTurnCards.Clear();
    }
}