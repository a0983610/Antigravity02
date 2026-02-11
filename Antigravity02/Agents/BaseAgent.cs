using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Antigravity02.AIClient;
using Antigravity02.Tools;

namespace Antigravity02.Agents
{
    /// <summary>
    /// 所有 AI Agent 的基底類別，實作核心的 Function Calling 循環
    /// </summary>
    public abstract class BaseAgent
    {
        protected readonly IAIClient SmartClient;
        protected readonly IAIClient FastClient;
        private bool _useSmartModel = false; // 預設使用快速模型

        protected IAIClient Client => _useSmartModel ? SmartClient : FastClient;
        public bool IsSmartMode => _useSmartModel;
        
        public void SetModelMode(string mode)
        {
            bool wasSmart = _useSmartModel;
            if (mode?.ToLower() == "fast")
            {
                _useSmartModel = false;
            }
            else
            {
                _useSmartModel = true;
            }

            // 模式有變更時，通知子類別重新初始化工具宣告
            if (wasSmart != _useSmartModel)
            {
                OnModelModeChanged();
            }
        }

        /// <summary>
        /// 當模型模式切換時觸發，子類別可覆寫此方法以更新工具宣告等
        /// </summary>
        protected virtual void OnModelModeChanged() { }

        protected string SystemInstruction { get; set; }
        protected readonly JavaScriptSerializer Serializer;

        protected List<object> ToolDeclarations;
        protected List<object> ChatHistory; // 新增：保存完整對話紀錄

        protected BaseAgent(string apiKey, string smartModel, string fastModel)
        {
            SmartClient = new GeminiClient(apiKey, smartModel);
            FastClient = new GeminiClient(apiKey, fastModel);
            Serializer = new JavaScriptSerializer();
            ToolDeclarations = new List<object>();
            ChatHistory = new List<object>(); // 新增：初始化對話紀錄
        }

        /// <summary>
        /// 核心執行方法：接收指令並透過 UI 回饋進度
        /// </summary>
        public async Task ExecuteAsync(string userPrompt, IAgentUI ui)
        {
            // 將新的使用者訊息加入歷史紀錄
            ChatHistory.Add(new { role = "user", parts = new[] { new { text = userPrompt } } });

            bool continueLoop = true;
            int currentIteration = 0;
            const int maxIterations = 10;

            while (continueLoop && currentIteration < maxIterations)
            {
                currentIteration++;
                // 每次 iteration 開始前，Client 目前的模型即為此次使用的模型
                string currentModelName = Client.ModelName;
                ui.ReportThinking(currentIteration, currentModelName);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    var request = new GenerateContentRequest
                    {
                        Contents = ChatHistory,
                        Tools = ToolDeclarations,
                        SystemInstruction = SystemInstruction
                    };

                    string rawJson = await Client.GenerateContentAsync(request);
                    sw.Stop();

                    var data = Serializer.Deserialize<Dictionary<string, object>>(rawJson);

                    // 解析 Token 使用量 (從 usageMetadata 獲取)
                    int promptTokens = 0, candidateTokens = 0, totalTokens = 0;
                    if (data.ContainsKey("usageMetadata"))
                    {
                        var usage = data["usageMetadata"] as Dictionary<string, object>;
                        if (usage != null)
                        {
                            if (usage.ContainsKey("promptTokenCount"))
                                promptTokens = Convert.ToInt32(usage["promptTokenCount"]);
                            if (usage.ContainsKey("candidatesTokenCount"))
                                candidateTokens = Convert.ToInt32(usage["candidatesTokenCount"]);
                            if (usage.ContainsKey("totalTokenCount"))
                                totalTokens = Convert.ToInt32(usage["totalTokenCount"]);
                        }
                    }

                    // 紀錄 Log
                    UsageLogger.LogApiUsage(currentModelName, sw.ElapsedMilliseconds, promptTokens, candidateTokens, totalTokens);

                    var candidates = data["candidates"] as System.Collections.ArrayList;
                    if (candidates == null || candidates.Count == 0) break;

                    var modelContent = (candidates[0] as Dictionary<string, object>)["content"] as Dictionary<string, object>;
                    var parts = modelContent["parts"] as System.Collections.ArrayList;

                    ChatHistory.Add(modelContent);

                    bool hasFunctionCall = false;
                    var toolResponseParts = new List<object>();

                    foreach (Dictionary<string, object> part in parts)
                    {
                        if (part.ContainsKey("text"))
                        {
                            ui.ReportTextResponse(part["text"].ToString(), currentModelName);
                        }

                        if (part.ContainsKey("functionCall"))
                        {
                            hasFunctionCall = true;
                            var call = part["functionCall"] as Dictionary<string, object>;
                            string funcName = call["name"].ToString();
                            var argsDict = (call["args"] as Dictionary<string, object>) ?? new Dictionary<string, object>();

                            ui.ReportToolCall(funcName, Serializer.Serialize(argsDict));

                            // 執行具體的工具邏輯 (由子類別實作)
                            string result = await ProcessToolCallAsync(funcName, argsDict);
                            UsageLogger.LogAction(funcName, result); // 紀錄行動
                            ui.ReportToolResult(result);

                            toolResponseParts.Add(new
                            {
                                functionResponse = new
                                {
                                    name = funcName,
                                    response = new { content = result }
                                }
                            });
                        }
                    }

                    if (hasFunctionCall)
                    {
                        ChatHistory.Add(new { role = "function", parts = toolResponseParts });
                    }
                    else
                    {
                        continueLoop = false;
                    }
                }
                catch (Exception ex)
                {
                    UsageLogger.LogError($"Agent Error: {ex.Message}");
                    ui.ReportError(ex.Message);
                    break;
                }

                // 檢查是否達到上限並詢問是否繼續
                if (continueLoop && currentIteration >= maxIterations)
                {
                    bool shouldContinue = await ui.PromptContinueAsync($"已達到單次最大執行次數 ({maxIterations})，任務尚未完成。");
                    if (shouldContinue)
                    {
                        currentIteration = 0; // 重置計數，再給它一次循環
                    }
                    else
                    {
                        ui.ReportError("任務已被使用者中斷。");
                        continueLoop = false;
                    }
                }
            }
        }

        /// <summary>
        /// 子類別必須實作此方法來處理特定的工具呼叫
        /// </summary>
        protected abstract Task<string> ProcessToolCallAsync(string funcName, Dictionary<string, object> args);

        public void SaveChatHistory(string filePath)
        {
            try
            {
                string json = Serializer.Serialize(ChatHistory);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to save chat history: {ex.Message}");
            }
        }

        public void LoadChatHistory(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var history = Serializer.Deserialize<List<object>>(json);
                    if (history != null)
                    {
                        ChatHistory = history;
                    }
                }
                else
                {
                    Console.WriteLine($"[Error] File not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to load chat history: {ex.Message}");
            }
        }
    }
}
