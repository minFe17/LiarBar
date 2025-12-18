[System.Serializable]
public class SeotdaDataDTO
{
    public SeotdaDataDTO(ESeotdaRuleType type, int index)
    {
        this.type = type;
        this.index = index;
    }
    public ESeotdaRuleType type;
    public int index;
}