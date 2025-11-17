using UnityEngine;
using UnityEngine.UI;
using Utils;

public class NotifyUI : MonoBehaviour
{
    [SerializeField] Text _notifyText;

    void Awake()
    {
        SimpleSingleton<NotifyManager>.Instance.NotifyUI = this;
        gameObject.SetActive(false);
    }

    void HideNotify()
    {
        gameObject.SetActive(false);
    }

    public void Notify(string message)
    {
        gameObject.SetActive(true);
        _notifyText.text = message;
        Invoke("HideNotify", 1f);
    }
}
