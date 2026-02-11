using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Antigravity02.Agents;
using Antigravity02.Tools;

namespace Antigravity02
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 確保環境變量檔案存在
            EnsureEnvFileExists();

            // --- 配置獲取 ---
            string apiKey = GetApiKey();
            string smartModelRaw = GetConfig("GEMINI_SMART_MODEL") ?? GetConfig("GEMINI_MODEL");
            string fastModelRaw = GetConfig("GEMINI_FAST_MODEL") ?? GetConfig("GEMINI_MODEL");
            
            bool noModelConfigured = string.IsNullOrEmpty(smartModelRaw) && string.IsNullOrEmpty(fastModelRaw);

            // 使用者指定預設為 gemini-2.5-flash
            string smartModel = smartModelRaw ?? "gemini-2.5-flash";
            string fastModel = fastModelRaw ?? "gemini-2.5-flash";
            // ----------------

            Console.WriteLine("=== AI Automation Assistant ===");

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("\n[Error] API Key is required to start the AI Agent.");
            }
            else
            {
                if (noModelConfigured)
                {
                    // 不顯示在介面，而是默默更新 .env 檔案
                    await UpdateEnvWithModelListAsync(apiKey);
                }

                var agent = new UniversalAgent(apiKey, smartModel, fastModel);
                
                // 顯示目前使用的模型
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[Config] Smart Model: {smartModel}");
                Console.WriteLine($"[Config] Fast Model : {fastModel}");
                Console.ResetColor();

                var ui = new ConsoleUI();

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("\nUser: ");
                    Console.ResetColor();
                    
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input)) continue;

                    // --- 指令處理 ---
                    if (CommandManager.TryHandleCommand(input, agent, out bool shouldExit))
                    {
                        if (shouldExit) break;
                        continue;
                    }
                    // -----------------------

                    try
                    {
                        await agent.ExecuteAsync(input, ui);
                    }
                    catch (Exception ex)
                    {
                        UsageLogger.LogError($"System Error: {ex.Message}");
                        ui.ReportError(ex.Message);
                    }
                }
            }

            Console.WriteLine("\nProgram finished.");
            if (!Console.IsInputRedirected) { Console.ReadKey(); }
        }

        static async Task UpdateEnvWithModelListAsync(string apiKey)
        {
            const string envFileName = ".env";
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, envFileName);

            if (!File.Exists(envPath)) return;

            try
            {
                string envContent = File.ReadAllText(envPath);
                // 如果已經有模型列表註解，就不重複查詢寫入
                if (envContent.Contains("# --- 自動查詢可用模型列表 ---")) return;

                var tempClient = new Antigravity02.AIClient.GeminiClient(apiKey);
                string json = await tempClient.ListModelsAsync();

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var data = serializer.Deserialize<Dictionary<string, object>>(json);
                
                var modelDocs = new System.Text.StringBuilder();
                modelDocs.AppendLine("\n# --- 自動查詢可用模型列表 ---");

                if (data.ContainsKey("models"))
                {
                    var models = data["models"] as System.Collections.ArrayList;
                    foreach (Dictionary<string, object> model in models)
                    {
                        string name = model["name"].ToString().Replace("models/", "");
                        string displayName = model["displayName"].ToString();
                        
                        var methods = model["supportedGenerationMethods"] as System.Collections.ArrayList;
                        if (methods != null && methods.Contains("generateContent"))
                        {
                            modelDocs.AppendLine($"# {name,-25} : {displayName}");
                        }
                    }
                }
                modelDocs.AppendLine("# ------------------------------");

                // 插入到模型設置區塊之前
                if (envContent.Contains("GEMINI_MODEL="))
                {
                    envContent = envContent.Replace("GEMINI_MODEL=", modelDocs.ToString() + "GEMINI_MODEL=");
                }
                else
                {
                    envContent += modelDocs.ToString();
                }

                File.WriteAllText(envPath, envContent);
                Console.WriteLine($"[System] 已自動查詢可用模型列表，並寫入 {envFileName} 供參考。");
            }
            catch
            {
                // 靜默失敗，不影響主程式運行
            }
        }

        static string GetApiKey() => GetConfig("GEMINI_API_KEY") ?? PromptForApiKey();
        
        static string GetConfig(string keyName)
        {
            const string envFileName = ".env";

            // 1. 優先從系統環境變量讀取
            string value = Environment.GetEnvironmentVariable(keyName);
            if (!string.IsNullOrEmpty(value)) return value;

            // 2. 嘗試從本地 .env 檔案讀取
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, envFileName);
            if (File.Exists(envPath))
            {
                var lines = File.ReadAllLines(envPath);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue; // 跳過註解與空行
                    string prefix = keyName + "=";
                    if (trimmed.StartsWith(prefix))
                    {
                        string result = trimmed.Substring(prefix.Length).Trim().Trim('\'', '"');
                        return string.IsNullOrEmpty(result) ? null : result; // 空值視為未設定
                    }
                }
            }
            return null;
        }

        static void EnsureEnvFileExists()
        {
            const string envFileName = ".env";
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, envFileName);

            if (!File.Exists(envPath))
            {
                string content = "# Gemini API Key (必填)\n" +
                                 "GEMINI_API_KEY=\n\n" +
                                 "# 推理模型設置 (選填，沒填會使用預設值)\n" +
                                 "# 也可以只設 GEMINI_MODEL 讓所有模組共用\n" +
                                 "GEMINI_MODEL=\n" +
                                 "GEMINI_SMART_MODEL=\n" +
                                 "GEMINI_FAST_MODEL=\n";
                
                try
                {
                    File.WriteAllText(envPath, content);
                    Console.WriteLine("[System] 已自動創建 .env 配置文件，請填入 API Key 後重新啟動。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[System Error] 無法創建 .env 檔案: {ex.Message}");
                }
            }
        }

        static string PromptForApiKey()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[API Key Not Found]");
            Console.WriteLine("Please set environment variable 'GEMINI_API_KEY' or create '.env' file.");
            Console.Write("Or enter API Key now: ");
            Console.ResetColor();

            return Console.ReadLine()?.Trim();
        }
    }

    public class ConsoleUI : IAgentUI
    {
        public void ReportThinking(int iteration, string modelName)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[Thinking Iteration {iteration} ({modelName})] ...");
            Console.ResetColor();
        }

        public void ReportToolCall(string toolName, string args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Action: {toolName}");
            Console.ResetColor();
        }

        public void ReportToolResult(string resultSummary)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            string text = resultSummary ?? "(no result)";
            string summary = text.Length > 100 ? text.Substring(0, 100) + "..." : text;
            Console.WriteLine($"Result: {summary}");
            Console.ResetColor();
        }

        public void ReportTextResponse(string text, string modelName)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\nAI ({modelName}): {text}");
            Console.ResetColor();
        }

        public void ReportError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {message}");
            Console.ResetColor();
        }

        public Task<bool> PromptContinueAsync(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n[PROMPT] {message} (Y/N): ");
            Console.ResetColor();
            string input = Console.ReadLine()?.Trim().ToLower();
            return Task.FromResult(input == "y" || input == "yes");
        }
    }
}
