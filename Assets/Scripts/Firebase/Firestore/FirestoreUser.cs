using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class FirestoreUser
{
    [JsonProperty("fields")]
    private Dictionary<string, object> Fields { get; set; }

    public FirestoreUser()
    {
        Fields = new Dictionary<string, object>();
    }

    // 값 저장
    public void SetField<T>(string key, T value)
    {
        object firestoreValue = ConvertToFirestoreValue(value);
        Fields[key] = firestoreValue;
    }

    private object ConvertToFirestoreValue<T>(T value)
    {
        if (value is string s)
            return new Dictionary<string, string> { { "stringValue", s } };
        if (value is int i)
            return new Dictionary<string, string> { { "integerValue", i.ToString() } };
        return null;
    }

    // 값 가져오기
    public T GetField<T>(string key)
    {
        if (!Fields.ContainsKey(key))
            return default;

        var valueDict = Fields[key] as Dictionary<string, object>;
        if (valueDict == null)
            return default;

        if (typeof(T) == typeof(string) && valueDict.ContainsKey("stringValue"))
            return (T)(object)valueDict["stringValue"].ToString();

        if (typeof(T) == typeof(int) && valueDict.ContainsKey("integerValue") && int.TryParse(valueDict["integerValue"].ToString(), out int intValue))
            return (T)(object)intValue;

        return default;
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static FirestoreUser FromJson(string json)
    {
        return JsonConvert.DeserializeObject<FirestoreUser>(json);
    }
}
