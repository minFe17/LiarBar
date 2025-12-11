using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.U2D;

public class SeotdaRuleUI : MonoBehaviour
{
    [SerializeField]
    private Image _firstImage;
    [SerializeField] 
    private Image _secondImage;
    [SerializeField]
    private int _ruleNumber;

    private List<GameObject> _myCards;
    private SpriteAtlas _spriteAtlas;
    private void Start()
    {
        FindCards();
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
        if (_myCards.Count < 2) return;
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
    private void ChangeCardAndRule(int index1, int index2)
    {
        string firstName = _myCards[index1].name.Split('(')[0].Trim();
        Sprite sprite = _spriteAtlas.GetSprite(firstName);
        _firstImage.sprite = sprite;

        string secondName = _myCards[index2].name.Split('(')[0].Trim();
        sprite = _spriteAtlas.GetSprite(secondName);
        _secondImage.sprite = sprite;

        //이름 알려주는 코드도 넣으면 끝!
    }
    private void FindCards()
    {
        _myCards = GetComponentInParent<SeotdaCardManager>().MyCards;
        _spriteAtlas = Resources.Load<SpriteAtlas>("SpriteAtlas/SeotdaCardAtlas");
    }


    
}
