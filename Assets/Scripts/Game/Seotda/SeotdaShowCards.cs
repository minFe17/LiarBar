using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SeotdaShowCards : MonoBehaviour
{
    [SerializeField]
    private FlowerCardDB db;

    private const float SPACE_VALUE = 0.1f;

    private Transform _playerLeftHand;
    private List<GameObject> _myCards;
    private int _cardCount = 0;

    private Vector3 _originalScale = new Vector3(0.05f, 0.05f, 0.05f);

    private void Start()
    {
        FindCards();
        FindPlayerLeftHand();
    }

    private void Update()
    {
        if (_cardCount != _myCards.Count)
             UpdateCards();

        if(Keyboard.current.pKey.wasPressedThisFrame)
        {
            GameObject card = Instantiate(db.FlowerCardPrefabs[0]);
            _myCards.Add(card);
        }
    }
    private void UpdateCards()
    {
        //상하좌우 포지션 먼저 맞춰주기 (로테이션도)
        //카드 증가하는것도 상-> x감소 하->x증가 좌->z감소 우 ->z증가 이렇게 맞춰줘야됨.
        Vector3 position = _playerLeftHand.position + new Vector3(SPACE_VALUE*(_cardCount+1), SPACE_VALUE, 0);
        _myCards[_cardCount].gameObject.transform.localScale = _originalScale;
        _myCards[_cardCount].gameObject.transform.position = position;

        Debug.Log(_myCards[_cardCount].gameObject.transform.position);

        _cardCount = _myCards.Count;
    }
    private void FindPlayerLeftHand()
    {
        _playerLeftHand = MyPlayer.local.LeftHand;
    }

    private void FindCards()
    {
        SeotdaCardManager manager = GetComponentInParent<SeotdaCardManager>();
        _myCards = manager.MyCards;
        _cardCount = _myCards.Count;
    }
}
