using UnityEngine;
using Utils;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
public class DataManager : SimpleSingleton<DataManager>
{
    private List<SeotdaData> _data;


   public List<SeotdaData> Data
    {
        get { return _data; } 
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
