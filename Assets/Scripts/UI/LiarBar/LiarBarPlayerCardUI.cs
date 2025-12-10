using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class LiarBarPlayerCardUI : MonoBehaviour, IMediatorEvent
{
    [SerializeField] List<LiarBarCardSlot> _cardSlots;
    [SerializeField] GameObject _playerTurnUI;

    RectTransform _cardParent;
    GamePlayer _gamePlayer;

    int _cardParentWidth = 1130;
    int _cardSize = 225;
    int _currentCardIndex = 0;
    bool _isReady;

    public void Init()
    {
        _cardParent = GetComponent<RectTransform>();
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorEventType.InitHandCard, this);
    }

    void InitCardUI()
    {
        _cardParent.sizeDelta = new Vector2(_cardParentWidth, _cardParent.sizeDelta.y);

        for (int i = 0; i < _gamePlayer.Cards.Count; i++)
        {
            ELiarBarCardType cardType = _gamePlayer.Cards[i];
            _cardSlots[i].Init(cardType, LiarBarCardManager.Instance.GetCardSprite(cardType));
        }
        _isReady = true;
    }

    public void PlayerTurn()
    {
        _cardParent.gameObject.SetActive(true);
        _playerTurnUI.SetActive(true);
        _currentCardIndex = _cardSlots.FindIndex(slot => !slot.IsPlayed);
        if (_currentCardIndex >= 0)
            _cardSlots[_currentCardIndex].SetOutline(true);
    }

    public void ShowCard()
    {
        if (!_isReady)
            return;

        if (_gamePlayer.IsMyTurn)
            return;

        int remainingHand = _cardSlots.Count(slot => !slot.IsPlayed);
        if (remainingHand == 0)
            return;

        bool isActive = _cardParent.gameObject.activeSelf;
        _cardParent.gameObject.SetActive(!isActive);
    }

    public void SwitchCard(int direction)
    {
        if (!_gamePlayer.IsMyTurn)
            return;

        _cardSlots[_currentCardIndex].SetOutline(false);

        _currentCardIndex += direction;
        _currentCardIndex = Mathf.Clamp(_currentCardIndex, 0, _gamePlayer.Cards.Count - 1);

        while (_cardSlots[_currentCardIndex].IsPlayed)
        {
            int nextIndex = _currentCardIndex + direction;
            if (nextIndex < 0 || nextIndex >= _cardSlots.Count)
                break;
            _currentCardIndex = nextIndex;
        }

        _cardSlots[_currentCardIndex].SetOutline(true);
    }

    public void SelectCard()
    {
        if (!_gamePlayer.IsMyTurn)
            return;

        LiarBarCardSlot currentSlot = _cardSlots[_currentCardIndex];
        if (currentSlot.IsPlayed)
            return;

        currentSlot.SetSelected(!currentSlot.IsSelected);
    }

    public void ThrowCard()
    {
        if (!_gamePlayer.IsMyTurn)
            return;

        List<LiarBarCardSlot> selectedSlots = _cardSlots.Where(slot => slot.IsSelected).ToList();

        if (selectedSlots.Count == 0)
        {
            Debug.Log("선택한 카드가 없습니다!");
            return;
        }

        List<ELiarBarCardType> playedCards = selectedSlots.Select(slot => slot.CardType).ToList();
        _gamePlayer.PlayCard(playedCards);

        foreach (LiarBarCardSlot slot in selectedSlots)
            slot.SetPlayed(true);

        int remainingHand = _cardSlots.Count(slot => !slot.IsPlayed);
        if (remainingHand != 0)
        {
            _cardParentWidth -= _cardSize * selectedSlots.Count;
            _cardParent.sizeDelta = new Vector2(_cardParentWidth, _cardParent.sizeDelta.y);
        }
        _cardParent.gameObject.SetActive(false);
        _playerTurnUI.SetActive(false);
    }

    public void CallLiar()
    {
        _gamePlayer.CallLiar();
        _isReady = false;
    }

    #region Interface
    void IMediatorEvent.HandleEvent(object data)
    {
        GamePlayer player = data as GamePlayer;
        if (player == null)
            return;

        if (!player.photonView.IsMine)
            return;

        if (_gamePlayer == null)
        {
            _gamePlayer = player;
            _gamePlayer.OnStartTurn += PlayerTurn;
        }
        InitCardUI();
    }
    #endregion
}
