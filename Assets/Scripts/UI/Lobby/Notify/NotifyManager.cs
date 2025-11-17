using System.Collections.Generic;

public class NotifyManager
{
    // 싱글턴
    Dictionary<short, string> _errorCodeDict = new Dictionary<short, string>();
    NotifyUI _notifyUI;

    public NotifyUI NotifyUI { set => _notifyUI = value; }

    public void SetErrorCode()
    {
        _errorCodeDict[32757] = "방 가득 참";
        _errorCodeDict[32758] = "방 없음";
    }

    public void Notify(string message, short errorcode)
    {
        message += _errorCodeDict[errorcode];
        _notifyUI.Notify(message);
    }
}