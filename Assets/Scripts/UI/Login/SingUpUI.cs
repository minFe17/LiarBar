using UnityEngine;

public class SingUpUI : AuthUIBase
{
    [Header("Login UI")]
    [SerializeField] GameObject _loginUI;

    public override void OnClickButton()
    {
        base.OnClickButton();
        _firebaseRest.CreateAccount(_email, _password);

        _loginUI.SetActive(true);
        gameObject.SetActive(false);
    }
}