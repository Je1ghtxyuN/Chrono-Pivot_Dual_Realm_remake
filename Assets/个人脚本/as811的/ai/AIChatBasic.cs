using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class AIChatBasic : MonoBehaviour
{
    // 说明：
    // 1) 可在 Inspector 中填写 DeepSeek 相关参数。
    // 2) 可通过组件右上角 ContextMenu 触发一次发送测试。
    // 3) 也可由其他脚本调用 SendToDeepSeek(string) 进行请求。

    // ===== DeepSeek 接口配置 =====
    [Header("DeepSeek 接口设置")]
    [Tooltip("DeepSeek 的 API Key（Bearer Token）。")]
    [FormerlySerializedAs("apiKey")]
    [SerializeField] private string _apiKey = "";
    [Tooltip("聊天补全接口地址，通常保持默认即可。")]
    [FormerlySerializedAs("endpoint")]
    [SerializeField] private string _endpoint = "https://api.deepseek.com/chat/completions";
    [Tooltip("模型名称，例如 deepseek-chat。")]
    [FormerlySerializedAs("model")]
    [SerializeField] private string _model = "deepseek-chat";

    // ===== 采样参数（与服务端请求体字段对应） =====
    [Header("采样参数")]
    [Tooltip("采样温度，越高随机性越强。")]
    [FormerlySerializedAs("temperature")]
    [SerializeField] private float _temperature = 0.3f;
    [Tooltip("频率惩罚，降低重复用词倾向。")]
    [FormerlySerializedAs("frequencyPenalty")]
    [SerializeField] private float _frequencyPenalty = 1.3f;
    [Tooltip("存在惩罚，鼓励引入新内容。")]
    [FormerlySerializedAs("presencePenalty")]
    [SerializeField] private float _presencePenalty = 0.8f;
    [Tooltip("单次回复最大 token 数。")]
    [FormerlySerializedAs("maxTokens")]
    [SerializeField] private int _maxTokens = 2000;
    [Tooltip("是否启用流式输出（边生成边显示）。")]
    [FormerlySerializedAs("stream")]
    [SerializeField] private bool _stream = true;

    // ===== 输入/输出内容 =====
    [Header("输入与输出")]
    [Tooltip("系统提示词（可选），用于约束模型风格与行为。")]
    [TextArea(2, 6)]
    [FormerlySerializedAs("systemPrompt")]
    [SerializeField] private string _systemPrompt = "";
    [Tooltip("用户输入内容。ContextMenu 测试会直接发送该字段。")]
    [TextArea(3, 8)]
    [FormerlySerializedAs("inputText")]
    [SerializeField] private string _inputText = "";
    [Tooltip("模型输出内容（流式追加）。")]
    [TextArea(5, 20)]
    [FormerlySerializedAs("outputText")]
    [SerializeField] private string _outputText = "";
    [Tooltip("每次请求前是否清空输出框。")]
    [FormerlySerializedAs("clearOutputBeforeRequest")]
    [SerializeField] private bool _clearOutputBeforeRequest = true;

    // ===== 运行时状态 =====
    [Header("运行状态")]
    [Tooltip("当前是否有请求正在执行。")]
    [FormerlySerializedAs("isRequestRunning")]
    [SerializeField] private bool _isRequestRunning = false;

    // 当前运行中的请求协程引用（用于防重复发送、取消请求）
    private Coroutine _requestCoroutine;

    // 只读输出：供外部显示脚本（如 AITextVisiable）轮询读取。
    public string OutputText => _outputText;
    public bool IsRequestRunning => _isRequestRunning;

    // 右键菜单快速测试：直接发送 inputText。
    [ContextMenu("Send Input Text To DeepSeek")]
    public void SendInputText()
    {
        SendToDeepSeek(_inputText);
    }

    // 对外发送入口：校验参数后开启协程请求。
    public void SendToDeepSeek(string content)
    {
        if (_isRequestRunning)
        {
            Debug.LogWarning("AIChatBasic: 上一次请求尚未结束。");
            return;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            Debug.LogWarning("AIChatBasic: 输入内容为空。");
            return;
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            Debug.LogError("AIChatBasic: API Key 为空。请在 Inspector 中填写。\n");
            return;
        }

        _requestCoroutine = StartCoroutine(SendRoutine(content));
    }

    // 对外取消入口：中断当前协程并恢复状态。
    public void CancelRequest()
    {
        if (!_isRequestRunning || _requestCoroutine == null)
        {
            return;
        }

        StopCoroutine(_requestCoroutine);
        _requestCoroutine = null;
        _isRequestRunning = false;
        Debug.LogWarning("AIChatBasic: 请求已手动取消。");
    }

    // 核心请求协程：
    // - 构造 JSON 请求体
    // - 根据 stream 选择普通下载或 SSE 流式下载处理器
    // - 处理成功/失败并将结果写回 outputText
    private IEnumerator SendRoutine(string content)
    {
        _isRequestRunning = true;

        if (_clearOutputBeforeRequest)
        {
            _outputText = string.Empty;
        }

        ChatRequestBody body = BuildRequestBody(content);
        string json = JsonUtility.ToJson(body);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(_endpoint, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(payload);

            StreamingSseDownloadHandler streamingHandler = null;
            if (_stream)
            {
                streamingHandler = new StreamingSseDownloadHandler(OnStreamData);
                request.downloadHandler = streamingHandler;
            }
            else
            {
                request.downloadHandler = new DownloadHandlerBuffer();
            }

            request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            while (!op.isDone)
            {
                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                // 流式模式下错误文本可能已被 DownloadHandlerScript 累积。
                string errorBody = _stream
                    ? streamingHandler != null ? streamingHandler.AllText : string.Empty
                    : request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

                _outputText += $"\n[Error] {request.responseCode} - {request.error}\n{errorBody}";
                Debug.LogError($"AIChatBasic: DeepSeek API 调用失败: {request.responseCode} - {request.error}\n{errorBody}");
            }
            else if (!_stream)
            {
                // 非流式：完整 JSON 一次性解析。
                string responseText = request.downloadHandler.text;
                ChatResponseBody full = JsonUtility.FromJson<ChatResponseBody>(responseText);
                string modelOutput = TryExtractAssistantContent(full);

                if (!string.IsNullOrEmpty(modelOutput))
                {
                    _outputText += modelOutput;
                }
                else
                {
                    _outputText += "\n[Warn] 响应解析为空。";
                }
            }
        }

        _isRequestRunning = false;
        _requestCoroutine = null;
    }

    // 流式回调：每收到一个 delta 片段就追加到输出，实现“边生成边显示”。
    private void OnStreamData(string delta)
    {
        if (!string.IsNullOrEmpty(delta))
        {
            _outputText += delta;
        }
    }

    // 按当前 Inspector 参数拼装请求体。
    private ChatRequestBody BuildRequestBody(string userContent)
    {
        List<ChatMessage> messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(_systemPrompt))
        {
            messages.Add(new ChatMessage
            {
                role = "system",
                content = _systemPrompt
            });
        }

        messages.Add(new ChatMessage
        {
            role = "user",
            content = userContent
        });

        return new ChatRequestBody
        {
            model = _model,
            messages = messages.ToArray(),
            temperature = _temperature,
            frequency_penalty = _frequencyPenalty,
            presence_penalty = _presencePenalty,
            max_tokens = _maxTokens,
            stream = _stream
        };
    }

    // 非流式结果提取：从 choices[0].message.content 中读取最终文本。
    private string TryExtractAssistantContent(ChatResponseBody response)
    {
        if (response == null || response.choices == null || response.choices.Length == 0)
        {
            return string.Empty;
        }

        ChatChoice firstChoice = response.choices[0];
        if (firstChoice == null || firstChoice.message == null)
        {
            return string.Empty;
        }

        return firstChoice.message.content ?? string.Empty;
    }

    // ===== 请求/响应数据结构（用于 JsonUtility 序列化与反序列化） =====
    [Serializable]
    private class ChatRequestBody
    {
        public string model;
        public ChatMessage[] messages;
        public float temperature;
        public float frequency_penalty;
        public float presence_penalty;
        public int max_tokens;
        public bool stream;
    }

    [Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatResponseBody
    {
        public ChatChoice[] choices;
    }

    [Serializable]
    private class ChatChoice
    {
        public ChatMessage message;
        public ChatDelta delta;
    }

    [Serializable]
    private class ChatDelta
    {
        public string content;
    }

    // 自定义流式下载处理器：
    // DeepSeek stream=true 返回 SSE（Server-Sent Events）格式数据。
    // 本处理器逐块接收字节，按行切分 data: 前缀，再解析 JSON 的 delta.content。
    private class StreamingSseDownloadHandler : DownloadHandlerScript
    {
        private readonly Action<string> _onDelta;
        private readonly Decoder _utf8Decoder = Encoding.UTF8.GetDecoder();
        private readonly char[] _charBuffer = new char[4096];
        private readonly StringBuilder _lineBuffer = new StringBuilder();
        private readonly StringBuilder _allText = new StringBuilder();

        public string AllText => _allText.ToString();

        public StreamingSseDownloadHandler(Action<string> onDelta)
            : base(new byte[4096])
        {
            _onDelta = onDelta;
        }

        // 每收到一段字节流就尝试转码与解析。
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength <= 0)
            {
                return true;
            }

            int charCount = _utf8Decoder.GetChars(data, 0, dataLength, _charBuffer, 0, false);
            if (charCount <= 0)
            {
                return true;
            }

            string chunk = new string(_charBuffer, 0, charCount);
            _allText.Append(chunk);
            _lineBuffer.Append(chunk);
            ParseSseLines();
            return true;
        }

        // 下载结束时再补一次解析，避免末尾残留未处理。
        protected override void CompleteContent()
        {
            ParseSseLines();
            base.CompleteContent();
        }

        // 按换行切分 SSE 行。
        private void ParseSseLines()
        {
            while (true)
            {
                int newlineIndex = IndexOfNewLine(_lineBuffer);
                if (newlineIndex < 0)
                {
                    break;
                }

                string line = _lineBuffer.ToString(0, newlineIndex).TrimEnd('\r');
                _lineBuffer.Remove(0, newlineIndex + 1);
                ProcessSseLine(line);
            }
        }

        // 仅处理 data: 开头行；[DONE] 代表流结束。
        private void ProcessSseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:"))
            {
                return;
            }

            string payload = line.Substring(5).Trim();
            if (payload == "[DONE]")
            {
                return;
            }

            ChatResponseBody streamResponse;
            try
            {
                streamResponse = JsonUtility.FromJson<ChatResponseBody>(payload);
            }
            catch
            {
                return;
            }

            if (streamResponse == null || streamResponse.choices == null || streamResponse.choices.Length == 0)
            {
                return;
            }

            string delta = streamResponse.choices[0]?.delta?.content;
            if (!string.IsNullOrEmpty(delta))
            {
                _onDelta?.Invoke(delta);
            }
        }

        // 在 StringBuilder 中查找下一行换行符位置。
        private int IndexOfNewLine(StringBuilder builder)
        {
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] == '\n')
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
