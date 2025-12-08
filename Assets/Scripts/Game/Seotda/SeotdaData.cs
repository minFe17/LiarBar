using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SeotdaData
{
    public ESeotdaRuleType type;
    public string name;
    public int rank;
    public ESeotdaCondition condition;
    public List<CardPair> cards;
}

[System.Serializable]
public struct CardPair
{
    public int first;
    public int second;
}