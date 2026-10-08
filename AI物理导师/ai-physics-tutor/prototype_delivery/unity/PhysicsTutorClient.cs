using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace PhysicsTutorPrototype
{
    public class PhysicsTutorClient : MonoBehaviour
    {
        public string endpoint = "http://127.0.0.1:18080/api/tutor";
        public bool Busy { get; private set; }
        public string EvidenceLogPath => Path.Combine(Application.persistentDataPath, "physics_tutor_calls.jsonl");

        public IEnumerator Ask(TutorRequest request, Action<TutorResponse> completed, Action<string> failed)
        {
            if (Busy) { failed?.Invoke("上一请求尚未完成"); yield break; }
            Busy = true;
            string payload = JsonUtility.ToJson(request);
            using (var http = new UnityWebRequest(endpoint, "POST"))
            {
                http.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
                http.downloadHandler = new DownloadHandlerBuffer();
                http.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                http.timeout = 120;
                yield return http.SendWebRequest();
                string raw = http.downloadHandler.text;
                // Keep the actual request and response, including failed calls.
                var record = new CallRecord { utc = DateTime.UtcNow.ToString("o"), request_json = payload,
                    response_json = raw, http_status = http.responseCode, transport_error = http.error };
                try { File.AppendAllText(EvidenceLogPath, JsonUtility.ToJson(record) + "\n", Encoding.UTF8); }
                catch (Exception e) { Debug.LogWarning("调用证据保存失败: " + e.Message); }
                Busy = false;
                if (http.result != UnityWebRequest.Result.Success) { failed?.Invoke(http.responseCode + ": " + raw + " " + http.error); yield break; }
                TutorResponse response = null;
                try { response = JsonUtility.FromJson<TutorResponse>(raw); }
                catch (Exception e) { failed?.Invoke("响应解析失败: " + e.Message); }
                if (response == null) yield break;
                if (!response.ok || response.answer == null) { failed?.Invoke(response.error ?? "回答失败"); yield break; }
                completed?.Invoke(response);
            }
        }
        [Serializable] private class CallRecord
        { public string utc, request_json, response_json, transport_error; public long http_status; }
    }
}
