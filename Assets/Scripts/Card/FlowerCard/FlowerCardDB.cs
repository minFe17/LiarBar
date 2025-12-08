using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FlowerCardDB", menuName = "FlowerCard/FlowerCardDB")]
public class FlowerCardDB : ScriptableObject
{
    [SerializeField]
    private List<GameObject> _flowerCardPrefabs;

    public List<GameObject> FlowerCardPrefabs
    {
        get { return _flowerCardPrefabs; }
    }
}