using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.U2D;
using TMPro;

public class SeotdaRuleUI : MonoBehaviour
{
    [SerializeField]
    private Image _firstImage;
    [SerializeField] 
    private Image _secondImage;
    [SerializeField]
    private int _ruleNumber;
    [SerializeField]
    private TextMeshProUGUI _ruleText;


    private SeotdaCardManager _cardManager;
    private List<GameObject> _myCards;
    private SpriteAtlas _spriteAtlas;
    private bool _isSetting = false;
    private void Start()
    {
        FindCards();
        ObjectOff();

    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe("AddCard", UpdateRule);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("AddCard", (Action)UpdateRule);
    }
    private void Update()
    {
        
    }
    private void UpdateRule()
    {
        if (_myCards.Count < 2 || _isSetting) return;
        if(_myCards.Count < 3&& _ruleNumber != 1 ) return;

        switch (_ruleNumber)
        {
            case 1:
                ChangeCardAndRule(0, 1);
                break;
            case 2:
                ChangeCardAndRule(0, 2);
                break;
            case 3:
                ChangeCardAndRule(1, 2);
                break;
        }
    }

    private void Reset()
    {
        ObjectOff();
        _isSetting = false;
    }
    private void ChangeCardAndRule(int index1, int index2)
    {
        ObjectOn();

        //이미지 바꿔주기
        string name = _myCards[index1].name.Split('(')[0].Trim();
        Sprite sprite = _spriteAtlas.GetSprite(name);
        _firstImage.sprite = sprite;
        name = _myCards[index2].name.Split('(')[0].Trim();
        sprite = _spriteAtlas.GetSprite(name);
        _secondImage.sprite = sprite;



        //이름 알려주는 코드도 넣으면 끝!
        FlowerCard card1 = _myCards[index1].GetComponent<FlowerCard>();
        FlowerCard card2 = _myCards[index2].GetComponent<FlowerCard>();


        SeotdaData? data = _cardManager.FindData(card1, card2);

        if (data != null)
        {
            _ruleText.text = data.Value.name;
        }
        else
        {

            Debug.Log("데이터 타입 안넘어옴 오류");
        }


        _isSetting = true;
    }
    private void FindCards()
    {
        _cardManager = GetComponentInParent<SeotdaCardManager>();
        _myCards = _cardManager.MyCards;
        _spriteAtlas = Resources.Load<SpriteAtlas>("SpriteAtlas/SeotdaCardAtlas");
    }

    private void ObjectOff()
    {
        _firstImage.gameObject.SetActive(false);
        _secondImage.gameObject.SetActive(false);
        _ruleText.gameObject.SetActive(false);
    }
    private void ObjectOn()
    {
        _firstImage.gameObject.SetActive(true);
        _secondImage.gameObject.SetActive(true);
        _ruleText.gameObject.SetActive(true);
    }
    
}
