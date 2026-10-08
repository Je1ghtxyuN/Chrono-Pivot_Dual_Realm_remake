using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace PhysicsTutorPrototype
{
    // UnityWebRequest and evidence logging adapted from the supplied tutor client.
    public sealed class PhysicsTutorClient : MonoBehaviour
    {
        public string endpoint = "";
        public bool Busy { get; private set; }
        public string EvidenceLogPath => Path.Combine(Application.persistentDataPath, "physics_tutor_calls.jsonl");
        UnityWebRequest active;

        public IEnumerator Ask(TutorRequest request, Action<TutorResponse> completed, Action<string> failed)
        {
            if (Busy) { failed?.Invoke("上一请求尚未完成"); yield break; }
            if (string.IsNullOrWhiteSpace(endpoint))
            { failed?.Invoke("尚未配置模型服务。语音和文字输入可继续调试。"); yield break; }
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            { failed?.Invoke("请填写有效的 HTTP / HTTPS 导师接口地址"); yield break; }
            Busy = true;
            try
            {
                string payload = JsonUtility.ToJson(request);
                using (var http = new UnityWebRequest(endpoint, "POST"))
                {
                    active = http;
                    http.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
                    http.downloadHandler = new DownloadHandlerBuffer();
                    http.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                    http.timeout = 120;
                    yield return http.SendWebRequest();
                    string raw = http.downloadHandler.text;
                    var record = new CallRecord { utc = DateTime.UtcNow.ToString("o"), request_json = payload,
                        response_json = raw, http_status = http.responseCode, transport_error = http.error };
                    try { File.AppendAllText(EvidenceLogPath, JsonUtility.ToJson(record) + "\n", Encoding.UTF8); }
                    catch (Exception e) { Debug.LogWarning("调用证据保存失败: " + e.Message); }
                    if (http.result != UnityWebRequest.Result.Success)
                    { failed?.Invoke(http.responseCode == 0 ? "无法连接导师服务，请确认本机导师已启动并检查服务地址。" : "HTTP " + http.responseCode + ": " + raw); yield break; }
                    TutorResponse response = null;
                    try { response = JsonUtility.FromJson<TutorResponse>(raw); }
                    catch (Exception e) { failed?.Invoke("响应解析失败: " + e.Message); yield break; }
                    if (response == null) { failed?.Invoke("导师返回了空响应"); yield break; }
                    if (!response.ok || response.answer == null || string.IsNullOrWhiteSpace(response.answer.reply))
                    { failed?.Invoke(response.error ?? "导师未返回有效回答"); yield break; }
                    completed?.Invoke(response);
                }
            }
            finally { active = null; Busy = false; }
        }
        void OnDisable() { if (active != null) active.Abort(); }
        void OnDestroy() { if (active != null) active.Abort(); Busy = false; }
        [Serializable] sealed class CallRecord
        { public string utc, request_json, response_json, transport_error; public long http_status; }
    }
}
