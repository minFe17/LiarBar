using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using UnityEngine.UI;
using Utils;

public class LiarBarUI : MonoBehaviour, IMediatorEvent
{
    [Header("Table Info")]
    [SerializeField] Text _cardText;
    [SerializeField] Image _cardImage;

    [Header("Card")]
    [SerializeField] RectTransform _cardParent;
    [SerializeField] List<LiarBarCardSlot> _cardSlots;

    [Header("Player Turn")]
    [SerializeField] GameObject _playerTurnUI;

    SpriteAtlas _cardAtlas;
    GamePlayer _gamePlayer;

    int _cardParentWidth = 1130;
    int _cardSize = 225;
    int _currentCardIndex = 0;

    bool _isReady;

    void Start()
    {
        LiarBarCardManager.Instance.OnSetTableAction += UpdateTargetCard;
        _cardAtlas = Resources.Load<SpriteAtlas>("SpriteAtlas/LiarBarCardAtlas");
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorEventType.InitHandCard, this);
    }

    void UpdateTargetCard()
    {
        _isReady = false;
        ELiarBarCardType targetCard = LiarBarCardManager.Instance.TargetCard;
        string cardStr = targetCard.ToString();
        _cardImage.sprite = _cardAtlas.GetSprite(cardStr);
        _cardText.text = $"{cardStr[0]} TABLE";
    }

    void InitCardUI()
    {
        _cardParent.sizeDelta = new Vector2(_cardParentWidth, _cardParent.sizeDelta.y);

        for (int i = 0; i < _gamePlayer.Cards.Count; i++)
        {
            ELiarBarCardType cardType = _gamePlayer.Cards[i];
            _cardSlots[i].Init(cardType, _cardAtlas.GetSprite(cardType.ToString()));
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

    #region Input System
    void OnShowCard(InputValue value)
    {
        //if (!_isReady)
        //    return;
        if (!value.isPressed)
            return;
        if (_gamePlayer.IsMyTurn)
            return;

        int remainingHand = _cardSlots.Count(slot => !slot.IsPlayed);
        if (remainingHand == 0)
            return;

        bool isActive = _cardParent.gameObject.activeSelf;
        _cardParent.gameObject.SetActive(!isActive);
    }

    void OnSwitchCard(InputValue value)
    {
        //if(!_gamePlayer.IsMyTurn)
        //    return;

        float val = value.Get<float>();
        if (Mathf.Approximately(val, 0f))
            return;

        _cardSlots[_currentCardIndex].SetOutline(false);

        int direction = (int)Mathf.Sign(val);
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

    void OnSelectCard(InputValue value)
    {
        //if (!_gamePlayer.IsMyTurn)
        //    return;
        if (!value.isPressed)
            return;

        LiarBarCardSlot currentSlot = _cardSlots[_currentCardIndex];
        if (currentSlot.IsPlayed)
            return;

        currentSlot.SetSelected(!currentSlot.IsSelected);
    }

    void OnThrowCard(InputValue value)
    {
        //if (!_gamePlayer.IsMyTurn)
        //    return;
        if (!value.isPressed)
            return;

        List<LiarBarCardSlot> selectedSlots = _cardSlots.Where(slot => slot.IsSelected).ToList();

        if (selectedSlots.Count == 0)
        {
            Debug.Log("선택한 카드가 없습니다!");
            return;
        }

        foreach (LiarBarCardSlot slot in selectedSlots)
        {
            _gamePlayer.PlayCard(slot.CardType);
            slot.SetPlayed(true);
        }

        int remainingHand = _cardSlots.Count(slot => !slot.IsPlayed);
        if (remainingHand != 0)
        {
            _cardParentWidth -= _cardSize * selectedSlots.Count;
            _cardParent.sizeDelta = new Vector2(_cardParentWidth, _cardParent.sizeDelta.y);
        }
        _cardParent.gameObject.SetActive(false);
        _playerTurnUI.SetActive(false);
    }

    void OnCallLiar(InputValue value)
    {
        if (!value.isPressed)
            return;

        // 낸 카드가 없으면(라운드 시작) return
        _gamePlayer.CallLiar();
    }
    #endregion

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