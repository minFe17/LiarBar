public class NotifyManager
{
    // ╫л╠шео
    NotifyUI _notifyUI;

    public NotifyUI NotifyUI { set => _notifyUI = value; }

    public void Notify(string message)
    {
        _notifyUI.Notify(message);
    }
}