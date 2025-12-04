using UnityEngine;
using UnityEngine.UI;
using Utils;

public class AuthUIBase : MonoBehaviour
{
    [SerializeField] InputField _emailInput;
    [SerializeField] InputField _passwordInput;

    protected string _email;
    protected string _password;
    protected FirebaseREST _firebaseRest;

    void Start()
    {
        _firebaseRest = MonoSingleton<FirebaseREST>.Instance;
    }

    public void OnEmailEndEdit(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _email = text;
            _passwordInput.Select();
        }
    }

    public void OnPasswordEndEdit(string text)
    {
        if (!string.IsNullOrEmpty(text))
            OnClickButton();
    }

    public virtual void OnClickButton()
    {
        _email = _emailInput.text;
        _password = _passwordInput.text;

        if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_password))
            return;
    }
}