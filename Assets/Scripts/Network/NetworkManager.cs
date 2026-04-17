using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class NetworkManager : MonoBehaviour
{
    [Header("Szerver beallitasok")]
    public string serverUrl = "https://majdittlesz.com";

    [Header("Teszt adatok")]
    public string editorTestUserId = "test_lazalaca_01";
    public string editorTestToken = "token_1";

    public void SendResults(BattleResultData data)
    {
        StartCoroutine(PostRequestCoroutine(serverUrl + "/api/score", JsonUtility.ToJson(data)));
    }

    private IEnumerator PostRequestCoroutine(string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Hiba a kuldes soran: " + request.error);
            }
            else
            {
                Debug.Log("Szerver valasza: " + request.downloadHandler.text);
            }
        }
    }
}