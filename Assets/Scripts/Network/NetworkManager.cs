using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [Header("Beta teszt adatok")]
    public string editorTestUserId = "test_lazalaca_01";
    public string editorTestToken = "dummy_token_123";

    public void SendResultsMock(BattleResultData data)
    {
        //szép JSON generálása true paraméterrel
        string jsonPayload = JsonUtility.ToJson(data, true);

        Debug.Log("<color=cyan><b>[NetworkManager] Adatcsomag elõkészítve küldésre:</b></color>\n" + jsonPayload);
    }
}