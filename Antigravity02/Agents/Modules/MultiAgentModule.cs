using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Antigravity02.AIClient;
using Antigravity02.Tools;
using Antigravity02.UI;

namespace Antigravity02.Agents
{
    /// <summary>
    /// 專家 Session：維護單一專家的角色設定與對話歷史
    /// </summary>
    internal class ExpertSession
    {
        public string Role { get; set; }
        public List<object> History { get; set; } = new List<object>();
    }

    /// <summary>
    /// 多重 AI 代理模組：允許主 Agent 創建並諮詢其他特定角色的 AI 專家
    /// 支援多輪對話，每位專家擁有獨立的記憶與上下文
    /// </summary>
    public class MultiAgentModule : IAgentModule
    {
        private readonly IAIClient _client;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        /// <summary>
        /// 以 expert_name 為 Key 管理多個專家 Session
        /// </summary>
        private readonly Dictionary<string, ExpertSession> _sessions = new Dictionary<string, ExpertSession>(StringComparer.OrdinalIgnoreCase);

        public MultiAgentModule(string apiKey, string modelName)
        {
            _client = new GeminiClient(apiKey, modelName);
        }

        public IEnumerable<object> GetToolDeclarations(IAIClient client)
        {
            yield return client.CreateFunctionDeclaration(
                "consult_expert",
                "諮詢一個特定領域的 AI 專家，支援多輪對話。使用相同的 expert_name 可延續先前的對話。\n" +
                "首次建立專家時必須提供 role，之後追問只需 expert_name 和 question。\n" +
                "目前活躍的專家列表會在回應中附帶提示。",
                new
                {
                    type = "object",
                    properties = new
                    {
                        expert_name = new { type = "string", description = "專家的識別名稱 (例如 'security_expert', 'arch_expert')，用於多輪對話時識別同一位專家" },
                        question = new { type = "string", description = "要問專家的具體問題或任務內容" },
                        role = new { type = "string", description = "專家的角色設定與專業背景 (System Instruction)。首次建立專家時必填，後續追問可省略" }
                    },
                    required = new[] { "expert_name", "question" }
                }
            );

            yield return client.CreateFunctionDeclaration(
                "list_experts",
                "列出目前所有活躍中的專家 Session，包含名稱、角色設定、對話輪數。",
                new
                {
                    type = "object",
                    properties = new { }
                }
            );

            yield return client.CreateFunctionDeclaration(
                "dismiss_expert",
                "結束某位專家的 Session，釋放其對話歷史。",
                new
                {
                    type = "object",
                    properties = new
                    {
                        expert_name = new { type = "string", description = "要結束的專家識別名稱" }
                    },
                    required = new[] { "expert_name" }
                }
            );
        }

        public async Task<string> TryHandleToolCallAsync(string funcName, Dictionary<string, object> args, IAgentUI ui)
        {
            switch (funcName)
            {
                case "consult_expert":
                    string expertName = args.ContainsKey("expert_name") ? args["expert_name"].ToString() : "default";
                    string question = args.ContainsKey("question") ? args["question"].ToString() : "";
                    string role = args.ContainsKey("role") ? args["role"].ToString() : null;
                    return await ConsultExpertAsync(expertName, question, role, ui);

                case "list_experts":
                    return ListExperts();

                case "dismiss_expert":
                    string dismissName = args.ContainsKey("expert_name") ? args["expert_name"].ToString() : "";
                    return DismissExpert(dismissName);

                default:
                    return null;
            }
        }

        private async Task<string> ConsultExpertAsync(string expertName, string question, string role, IAgentUI ui)
        {
            ExpertSession session = null;
            bool isNewSession = false;
            bool historyAdded = false;

            try
            {
                // 取得或建立 Session

                if (_sessions.ContainsKey(expertName))
                {
                    session = _sessions[expertName];
                    // 如果有提供新的 role，更新它 (允許動態調整專家角色)
                    if (!string.IsNullOrEmpty(role))
                    {
                        session.Role = role;
                    }
                }
                else
                {
                    // 建立新 Session
                    if (string.IsNullOrEmpty(role))
                    {
                        return $"[System Error]: 建立新專家 '{expertName}' 時必須提供 'role' 設定（專業背景與指導原則）。";
                    }
                    session = new ExpertSession { Role = role };
                    _sessions[expertName] = session;
                    isNewSession = true;
                }

                // --- UI: 顯示諮詢開始 ---
                int currentTurn = (session.History.Count / 2) + 1;
                if (isNewSession)
                {
                    ui.ReportInfo($"\n[Expert: {expertName}] 建立新專家 Session");
                    ui.ReportInfo($"[Expert: {expertName}] 角色: {Truncate(session.Role, 80)}");
                }
                else
                {
                    ui.ReportInfo($"\n[Expert: {expertName}] 第 {currentTurn} 輪對話");
                }
                ui.ReportInfo($"[Expert: {expertName}] 提問: {Truncate(question, 120)}");
                ui.ReportInfo($"[Expert: {expertName}] 等待回應中...");

                // 將使用者問題加入此專家的對話歷史
                session.History.Add(new { role = "user", parts = new[] { new { text = question } } });
                historyAdded = true;

                // 建立 API 請求 (使用完整的對話歷史)
                var request = new GenerateContentRequest
                {
                    SystemInstruction = session.Role,
                    Contents = session.History,
                    Tools = null // 專家暫時不給予額外工具
                };

                string responseJson = await _client.GenerateContentAsync(request);

                // 解析回應
                var data = _serializer.Deserialize<Dictionary<string, object>>(responseJson);
                var candidates = data["candidates"] as System.Collections.ArrayList;

                if (candidates != null && candidates.Count > 0)
                {
                    var candidate = candidates[0] as Dictionary<string, object>;
                    var modelContent = candidate["content"] as Dictionary<string, object>;
                    var parts = modelContent["parts"] as System.Collections.ArrayList;

                    // 將模型回應加入對話歷史 (保持多輪對話)
                    session.History.Add(modelContent);

                    if (parts != null)
                    {
                        // 收集所有文字回應
                        var textParts = new System.Text.StringBuilder();
                        foreach (Dictionary<string, object> part in parts)
                        {
                            if (part.ContainsKey("text"))
                            {
                                textParts.AppendLine(part["text"].ToString());
                            }
                        }

                        if (textParts.Length > 0)
                        {
                            string responseText = textParts.ToString().TrimEnd();
                            int turnCount = session.History.Count / 2;

                            // --- UI: 顯示專家回應 ---
                            ui.ReportInfo($"\n[Expert: {expertName}] 回應 (第 {turnCount} 輪)：");
                            ui.ReportInfo(responseText);

                            string sessionInfo = isNewSession
                                ? $" (新建專家 Session，角色: {Truncate(session.Role, 50)})"
                                : $" (第 {turnCount} 輪對話)";

                            return $"[專家 {expertName} 回應]{sessionInfo}：\n{responseText}";
                        }
                    }
                }

                // 回應失敗時，移除剛加入的 user 訊息
                if (session.History.Count > 0)
                {
                    session.History.RemoveAt(session.History.Count - 1);
                }
                return $"[System]: 專家 {expertName} 沒有回應。";
            }
            catch (Exception ex)
            {
                // 異常時回滾：移除已加入的 user 訊息，避免殘留
                if (historyAdded && session != null && session.History.Count > 0)
                {
                    session.History.RemoveAt(session.History.Count - 1);
                }
                UsageLogger.LogError($"ConsultExpert({expertName}) Error: {ex.Message}");
                return $"[System Error] 諮詢專家 {expertName} 時發生錯誤: {ex.Message}";
            }
        }

        private string ListExperts()
        {
            if (_sessions.Count == 0)
            {
                return "目前沒有活躍的專家 Session。";
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"目前有 {_sessions.Count} 位活躍的專家：");
            sb.AppendLine();

            foreach (var kvp in _sessions)
            {
                int turns = kvp.Value.History.Count / 2;
                string rolePreview = Truncate(kvp.Value.Role, 60);
                sb.AppendLine($"  [{kvp.Key}] 對話輪數: {turns} | 角色: {rolePreview}");
            }

            return sb.ToString().TrimEnd();
        }

        private string DismissExpert(string expertName)
        {
            if (string.IsNullOrEmpty(expertName))
            {
                return "[System]: 請指定要結束的專家名稱。";
            }

            if (_sessions.ContainsKey(expertName))
            {
                int turns = _sessions[expertName].History.Count / 2;
                _sessions.Remove(expertName);
                return $"已結束專家 {expertName} 的 Session（共進行了 {turns} 輪對話）。";
            }

            return $"[System]: 找不到名為 {expertName} 的專家。";
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
        }
    }
}
