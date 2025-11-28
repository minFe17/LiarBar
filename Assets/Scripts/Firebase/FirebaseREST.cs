using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseREST : MonoBehaviour
{
    string _apiKey = "AIzaSyDCWRKN8CkLnayZF9zSYlUlPdTrRPzZPYY";
    string _projectId = "liarbar-2bd7c";
    string _idToken;

    FirestoreUser _firestoreUser;

    public Action OnLoginSuccess;

    public FirestoreUser User { get => _firestoreUser; }

    void LoadUserData(Action onLoaded = null)
    {
        StartCoroutine(LoadUserDataRoutine(onLoaded));
    }

    public void CreateAccount(string email, string password)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
        StartCoroutine(AuthRoutine(email, password, url, true)); 
    }

    public void SingIn(string email, string password)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
        StartCoroutine(AuthRoutine(email, password, url, false)); 
    }

    public void SaveUserData()
    {
        StartCoroutine(SaveUserDataRoutine());
    }

    #region 코루틴
    IEnumerator AuthRoutine(string email, string password, string url, bool isSignUp = false)
    {
        // 계정 정보를 담은 객체
        AuthRequest payload = new AuthRequest(email, password);

        // REST API가 Json 형식으로 데이터를 받아 변환 필요
        string json = JsonConvert.SerializeObject(payload);

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

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Error: {www.error}");
            else
            {
                Debug.Log($"Response: {www.downloadHandler.text}");
                JObject data = JObject.Parse(www.downloadHandler.text);
                string uid = data["localId"].ToString();
                _idToken = data["idToken"].ToString();

                _firestoreUser = new FirestoreUser();
                _firestoreUser.SetField("uid", uid);

                if (isSignUp)
                {
                    // Firestore에 초기 데이터 저장
                    _firestoreUser.SetField("nickname", ""); // 기본값
                    StartCoroutine(SaveUserDataRoutine());   // 저장만 하고 LoadUserData는 생략
                }
                else
                {
                    // 로그인일 때만 Firestore에서 데이터 불러오기
                    LoadUserData(() => { OnLoginSuccess?.Invoke(); });
                }
            }
        }
    }

    public IEnumerator SaveUserDataRoutine()
    {
        string uid = _firestoreUser.GetField<string>("uid");
        string url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/users/{uid}";
        string json = _firestoreUser.ToJson();

        using (UnityWebRequest www = new UnityWebRequest(url, "PATCH"))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {_idToken}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Error: {www.error}");
            else
                Debug.Log($"Response: {www.downloadHandler.text}");
        }
    }

    IEnumerator LoadUserDataRoutine(Action onLoaded)
    {
        string uid = _firestoreUser.GetField<string>("uid");
        string url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/users/{uid}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", $"Bearer {_idToken}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"LoadUserData Error: {www.error}");
            }
            else
            {
                // Firestore 문서를 FirestoreUser로 재구성
                JObject json = JObject.Parse(www.downloadHandler.text);

                JObject fields = json["fields"] as JObject;

                if (fields != null)
                {
                    foreach (KeyValuePair<string, JToken> f in fields)
                    {
                        string key = f.Key;

                        // Firestore 타입별 처리
                        if (f.Value["stringValue"] != null)
                            _firestoreUser.SetField(key, (string)f.Value["stringValue"]);
                        else if (f.Value["integerValue"] != null)
                            _firestoreUser.SetField(key, (int)f.Value["integerValue"]);
                    }
                }
                onLoaded?.Invoke();
            }
        }
    }

    public IEnumerator CheckNicknameDuplicate(string nickname, Action<bool> onResult)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents:runQuery";

        // Firestore structuredQuery 생성
        var query = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = "users" } },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new { fieldPath = "nickname" },
                        op = "EQUAL",
                        value = new { stringValue = nickname }
                    }
                }
            }
        };

        string json = JsonConvert.SerializeObject(query);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {_idToken}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"CheckNicknameDuplicate Error: {www.error}");
                onResult(false); // 에러면 중복 체크 실패 처리
            }
            else
            {
                JArray response = JArray.Parse(www.downloadHandler.text);
                bool exists = false;

                foreach (JToken doc in response)
                {
                    if (doc["document"] != null)
                    {
                        exists = true; // 이미 존재
                        break;
                    }
                }

                onResult(!exists); // true면 사용 가능
            }
        }
    }
    #endregion
}