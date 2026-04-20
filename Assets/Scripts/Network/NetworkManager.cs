using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class NetworkManager : MonoBehaviour
{
    [Header("Szerver Beállítások")]
    public string serverUrl = "https://bmedutower.space";

    [Header("Teszt Adatok")]
    public string editorTestUserId = "test_moodle_diak_01";
    public string editorTestToken = "dummy_token_123";

    private string activeUserId;
    private string activeToken;

    private void Awake()
    {
        string url = Application.absoluteURL;

        if (!string.IsNullOrEmpty(url) && url.Contains("userid=") && url.Contains("token="))
        {
            activeUserId = GetParam(url, "userid");
            activeToken = GetParam(url, "token");
        }
        else
        {
            activeUserId = editorTestUserId;
            activeToken = editorTestToken;
        }
    }

    private string GetParam(string url, string param)
    {
        try
        {
            string prefix = param + "=";
            int start = url.IndexOf(prefix) + prefix.Length;
            int end = url.IndexOf("&", start);
            return end == -1 ? url.Substring(start) : url.Substring(start, end - start);
        }
        catch
        {
            return param == "userid" ? editorTestUserId : editorTestToken;
        }
    }

    public string GetActiveUserId() => activeUserId;
    public string GetActiveToken() => activeToken;

    public void SendResults(BattleResultData data)
    {
        string jsonPayload = JsonUtility.ToJson(data);

        // EZT A SORT ADD HOZZÁ:
        Debug.Log($"SendResults meghívva! Cél URL: {serverUrl}/api/score | Küldött JSON: {jsonPayload}");

        StartCoroutine(PostRequestCoroutine(serverUrl + "/api/score", jsonPayload));
    }

    private IEnumerator PostRequestCoroutine(string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("UnityWebRequest elküldve a szerver felé...");

            yield return request.SendWebRequest();

            Debug.Log($"Szerver válaszkód: {request.responseCode}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Hálózati hiba: {request.error}");
            }
            else
            {
                Debug.Log($"Szerver üzenete: {request.downloadHandler.text}");
            }
        }
    }
}