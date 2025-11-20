using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "TicTacThrow/Api Config", order = 0)]
public class ApiConfig : ScriptableObject
{
    [SerializeField] private string baseUrl = "http://172.24.150.11:3000";

    public string BaseUrl => baseUrl;

    public void SetBaseUrl(string value)
    {
        baseUrl = value;
    }
}

