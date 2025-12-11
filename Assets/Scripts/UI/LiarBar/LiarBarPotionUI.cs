using UnityEngine;
using UnityEngine.UI;
using Utils;

public class LiarBarPotionUI : MonoBehaviour, IMediatorEvent
{
    [SerializeField] Text _potiontext;
    int _potionCount;

    public void Init()
    {
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorEventType.DrinkPotion, this);
        _potionCount = 6;
        _potiontext.text = $"LEFT {_potionCount}";
    }

    void IMediatorEvent.HandleEvent(object data)
    {
        _potionCount--;
        _potiontext.text = $"LEFT {_potionCount}";
    }
}