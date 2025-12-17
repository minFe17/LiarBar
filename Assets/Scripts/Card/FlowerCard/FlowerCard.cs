using UnityEngine;

public class FlowerCard : MonoBehaviour
{
    [SerializeField]
    private EFlowerCardType _type;
    [SerializeField]
    private int _month;
    [SerializeField]
    private int _number;
    private string _name;

    public string Name
    {
        get { return _name; }
    }
    public int Month
    { get { return _month; } }
    public int Number
    { get { return _number; } }
    public EFlowerCardType Type
    { get { return _type; } }

    private void Awake()
    {
        _name = _month.ToString() + "_" + _number.ToString();
    }

}
