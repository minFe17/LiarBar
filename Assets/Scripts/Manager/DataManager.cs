using UnityEngine;
using Utils;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
public class DataManager : SimpleSingleton<DataManager>
{
    private List<SeotdaData> _data;


    public List<SeotdaData> Data
    {
        get { return _data; } 
    }
    public SeotdaData? FindDataToType(ESeotdaRuleType type)
    {
        for(int i=0;i<Data.Count; i++)
        {
            if (_data[i].type == type) return _data[i];
        }

        return null;
    }
    public SeotdaData? GetKeut()
    {
        for(int i=0;i<_data.Count;i++)
        {
            if (_data[i].type != ESeotdaRuleType.Keut) continue;
            return _data[i];
        }
        return null;
    }
    public ESeotdaCondition GetCondition(FlowerCard card1, FlowerCard card2)
    {
        ESeotdaCondition condition = new ESeotdaCondition();

        if (card1.Type == EFlowerCardType.Gwang && card2.Type == EFlowerCardType.Gwang)
            condition = ESeotdaCondition.Gwang;
        else if (card1.Month == card2.Month)
            condition = ESeotdaCondition.Same;
        else if ((card1.Type == EFlowerCardType.Gwang || card2.Type == EFlowerCardType.Gwang) && (card1.Type == EFlowerCardType.Drawing || card2.Type == EFlowerCardType.Drawing || card1.Type == EFlowerCardType.Gookjin || card2.Type == EFlowerCardType.Gookjin))
            condition = ESeotdaCondition.GwangAndDrawing;
        else if (((card1.Type == EFlowerCardType.Drawing || card1.Type == EFlowerCardType.Gookjin) && (card2.Type == EFlowerCardType.Drawing || card2.Type == EFlowerCardType.Gookjin)))
            condition = ESeotdaCondition.Drawing;
        else
            condition = ESeotdaCondition.None;


            return condition;
    }
    public List<SeotdaData> MatchData(int month)
    {
        List<SeotdaData> list = new List<SeotdaData>();

        for(int i=0;i< _data.Count;i++)
        {
            for(int j = 0; j < _data[i].cards.Count;j++)
            {
                if (_data[i].cards[j].first != month && _data[i].cards[j].second != month) continue;

                list.Add(_data[i]);
            }
        }

        return list;
    }
    public List<SeotdaData> MatchData(int month1, int month2)
    {
        List<SeotdaData> list = new List<SeotdaData>();

        for (int i = 0; i < _data.Count; i++)
        {
            for (int j = 0; j < _data[i].cards.Count; j++)
            {
                if (_data[i].cards[j].first != month1 && _data[i].cards[j].second != month1) continue;
                if (_data[i].cards[j].first !=month2 && _data[i].cards[j].second !=month2) continue;

                list.Add(_data[i]);
            }
        }

        return list;

    }
  
    public void LoadData()
    {
        LoadSeotdaData();
    }
    private void LoadSeotdaData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Seotda/SeotdaRules");
        _data = JsonConvert.DeserializeObject<List<SeotdaData>>(jsonFile.text);
    }
}
