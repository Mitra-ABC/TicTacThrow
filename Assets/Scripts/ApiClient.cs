using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [SerializeField] private ApiConfig apiConfig;
    [SerializeField] private string baseUrlOverride;
    [SerializeField] private float requestTimeoutSeconds = 10f;

    public string BaseUrl
    {
        get
        {
            if (!string.IsNullOrEmpty(baseUrlOverride))
            {
                return baseUrlOverride.TrimEnd('/');
            }

            if (apiConfig != null && !string.IsNullOrEmpty(apiConfig.BaseUrl))
            {
                return apiConfig.BaseUrl.TrimEnd('/');
            }

            return "http://localhost:3000";
        }
    }

    public void SetBaseUrl(string url)
    {
        baseUrlOverride = url;
    }

    public IEnumerator CreatePlayer(string nickname, Action<PlayerResponse> onSuccess, Action<string> onError)
    {
        var payload = new CreatePlayerRequest { nickname = nickname };
        var json = JsonUtility.ToJson(payload);

        yield return SendRequest("/api/players", UnityWebRequest.kHttpVerbPOST, json,
            response =>
            {
                var data = ApiResponseParser.ParsePlayerResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator CreateRoom(int playerId, Action<CreateRoomResponse> onSuccess, Action<string> onError)
    {
        var payload = new CreateRoomRequest { playerId = playerId };
        var json = JsonUtility.ToJson(payload);

        yield return SendRequest("/api/rooms", UnityWebRequest.kHttpVerbPOST, json,
            response =>
            {
                var data = ApiResponseParser.ParseCreateRoomResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator JoinRoom(int roomId, int playerId, Action<JoinRoomResponse> onSuccess, Action<string> onError)
    {
        var payload = new JoinRoomRequest { playerId = playerId };
        var json = JsonUtility.ToJson(payload);
        var endpoint = $"/api/rooms/{roomId}/join";

        yield return SendRequest(endpoint, UnityWebRequest.kHttpVerbPOST, json,
            response =>
            {
                var data = ApiResponseParser.ParseJoinRoomResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator GetRoom(int roomId, Action<RoomStateResponse> onSuccess, Action<string> onError)
    {
        var endpoint = $"/api/rooms/{roomId}";

        yield return SendRequest(endpoint, UnityWebRequest.kHttpVerbGET, null,
            response =>
            {
                var data = ApiResponseParser.ParseRoomStateResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    public IEnumerator PlayMove(int roomId, int playerId, int cellIndex, Action<MoveResponse> onSuccess, Action<string> onError)
    {
        var payload = new MoveRequest { playerId = playerId, cellIndex = cellIndex };
        var json = JsonUtility.ToJson(payload);
        var endpoint = $"/api/rooms/{roomId}/moves";

        yield return SendRequest(endpoint, UnityWebRequest.kHttpVerbPOST, json,
            response =>
            {
                var data = ApiResponseParser.ParseMoveResponse(response);
                onSuccess?.Invoke(data);
            },
            onError);
    }

    private IEnumerator SendRequest(string endpoint, string method, string jsonBody, Action<string> onSuccess, Action<string> onError)
    {
        var url = $"{BaseUrl}{endpoint}";
        Debug.Log($"[ApiClient] Sending {method} {url} bodyLength={(jsonBody?.Length ?? 0)}");
        using (var request = new UnityWebRequest(url, method))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(jsonBody))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            if (requestTimeoutSeconds > 0f)
            {
                request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                var message = !string.IsNullOrEmpty(request.error)
                    ? request.error
                    : "Unknown network error";
                if (request.responseCode != 0)
                {
                    message = $"{request.responseCode}: {message}";
                }
                Debug.LogWarning($"[ApiClient] Request FAILED {method} {endpoint} - {message}");
                onError?.Invoke(message);
                yield break;
            }

            var responseText = request.downloadHandler.text;
            Debug.Log($"[ApiClient] Response {method} {endpoint} code={request.responseCode} length={(responseText?.Length ?? 0)}");
            if (string.IsNullOrEmpty(responseText))
            {
                Debug.LogWarning($"[ApiClient] Empty response from server for {endpoint}");
                onError?.Invoke("Empty response from server.");
                yield break;
            }

            try
            {
                Debug.Log($"[ApiClient] Parsing response for {endpoint}");
                onSuccess?.Invoke(responseText);
                Debug.Log($"[ApiClient] Successfully parsed response for {endpoint}");
            }
            catch (Exception ex)
            {
                LogResponseDiagnostics(endpoint, responseText);
                Debug.LogError($"JSON parse error at '{endpoint}': {ex.Message}\nResponse: {responseText}");
                onError?.Invoke($"JSON parse error: {ex.Message}");
            }
        }
    }

    private void LogResponseDiagnostics(string endpoint, string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
        {
            Debug.LogWarning($"Response diagnostics ({endpoint}): empty body");
            return;
        }

        var builder = new StringBuilder();
        builder.Append($"Response diagnostics ({endpoint}): length={responseText.Length}, preview=");

        var charsToInspect = Math.Min(32, responseText.Length);
        for (int i = 0; i < charsToInspect; i++)
        {
            var c = responseText[i];
            builder.AppendFormat("\\u{0:X4}", (int)c);
            if (i < charsToInspect - 1)
            {
                builder.Append(' ');
            }
        }

        Debug.LogWarning(builder.ToString());
    }
}

