using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTutorPrototype
{
    [RequireComponent(typeof(PhysicsTutorClient))]
    public class PhysicsTutorDemo : MonoBehaviour
    {
        string question = "质量为2 kg的物体受到6 N合力，如何判断加速度？先给我提示。";
        string observation = "我把质量和力直接相加了。", output = "等待真实接口回答。";
        int level = 1;
        Vector2 scroll;
        readonly List<TutorMessage> history = new List<TutorMessage>();
        PhysicsTutorClient client;
        void Awake() { client = GetComponent<PhysicsTutorClient>(); }
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, Mathf.Min(Screen.width - 40, 900), Screen.height - 40));
            GUILayout.Label("AI物理导师原型 · 有限范围演示 · 尚非专家教学验收");
            GUILayout.Label("接口（服务须运行，校园网通过 SSH 转发访问）");
            client.endpoint = GUILayout.TextField(client.endpoint);
            GUILayout.Label("问题"); question = GUILayout.TextArea(question, GUILayout.Height(65));
            GUILayout.Label("学生状态 / 当前观察"); observation = GUILayout.TextArea(observation, GUILayout.Height(45));
            level = GUILayout.Toolbar(level - 1, new[] { "1 概念方向", "2 关系与步骤", "3 详细指导" }) + 1;
            GUI.enabled = !client.Busy;
            if (GUILayout.Button("发送真实请求"))
            {
                var request = new TutorRequest { question = question, hint_level = level,
                    student_state = new StudentState { observation = observation }, history = history.ToArray() };
                output = "模型正在生成…";
                StartCoroutine(client.Ask(request, response => {
                    output = response.answer.reply + "\n\n状态: " + response.answer.status;
                    if (response.evidence != null) foreach (var card in response.evidence)
                        output += "\n\n" + card.id + " | " + card.title + "\n" + card.source_url;
                    history.Add(new TutorMessage { role = "user", content = request.question });
                    history.Add(new TutorMessage { role = "assistant", content = response.answer.reply });
                    // Bound the protocol history; new topics should use 'new conversation'.
                    while (history.Count > 10) history.RemoveAt(0);
                    Debug.Log("Physics tutor actual response: " + response.answer.reply);
                }, error => output = "调用失败（已保存证据）: " + error));
            }
            if (GUILayout.Button("新对话")) { history.Clear(); output = "已清空历史，请输入新问题。"; }
            GUI.enabled = true;
            GUILayout.Label("原始调用日志: " + client.EvidenceLogPath);
            scroll = GUILayout.BeginScrollView(scroll); GUILayout.Label(output); GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
