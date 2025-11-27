using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseREST : MonoBehaviour
{
    string _apiKey = "AIzaSyDCWRKN8CkLnayZF9zSYlUlPdTrRPzZPYY";
    string _projectId = "LiarBar";

    FirestoreUser _firestoreUser;

    public Action OnLoginSuccess;

    public FirestoreUser User { get => _firestoreUser; }

    public void CreateAccount(string email, string password)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
        StartCoroutine(AuthRoutine(email, password, url));
    }

    public void SingIn(string email, string password)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
        StartCoroutine(AuthRoutine(email, password, url));
    }

    public void SaveUserData()
    {
        StartCoroutine(SaveUserDataRoutine());
    }

    IEnumerator AuthRoutine(string email, string password, string url)
    {
        // 계정 정보를 담은 객체
        AuthRequest payload = new AuthRequest(email, password);

        // REST API가 Json 형식으로 데이터를 받아 변환 필요
        string json = JsonUtility.ToJson(payload);

        // UnityWebRequest : Unity에서 HTTP 요청을 보내고 받을 때 사용하는 클래스
        // "POST" : 데이터를 서버에 보내는 요청
        // url로 POST 요청을 보내겠다라는 의미
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            // json 데이터를 전송
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            
            // 서버 응답 받음
            www.downloadHandler = new DownloadHandlerBuffer();

            // json임을 명시
            www.SetRequestHeader("Content-Type", "application/json");

            // UnityWebRequest로 만든 요청을 실제로 서버에 보내고 대기
            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Error: {www.error}");
            else
            {
                Debug.Log($"Response: {www.downloadHandler.text}");
                JObject data = JObject.Parse(www.downloadHandler.text);
                string uid = data["localId"].ToString();

                _firestoreUser = new FirestoreUser();
                _firestoreUser.SetField("uid", uid);

                OnLoginSuccess?.Invoke();
            }
        }
    }

    public IEnumerator SaveUserDataRoutine()
    {
        string uid = _firestoreUser.GetField<string>("uid");
        string url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/users/{uid}?key={_apiKey}";
        string json = _firestoreUser.ToJson();

        using (UnityWebRequest www = new UnityWebRequest(url, "PATCH"))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Error: {www.error}");
            else
                Debug.Log($"Response: {www.downloadHandler.text}");
        }
    }
}