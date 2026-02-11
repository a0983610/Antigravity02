using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Antigravity02
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 請替換為您的 API Key
            string apiKey = "YOUR_GEMINI_API_KEY";

            // 本地功能測試 (當沒有設定 API Key 時)
            if (apiKey == "YOUR_GEMINI_API_KEY")
            {
                Console.WriteLine("--- 啟動本地工具測試模式 ---");
                var tools = new FileTools();

                Console.WriteLine("\n[測試 1: 列出檔案]");
                string list = tools.ListFiles();
                Console.WriteLine(list);

                Console.WriteLine("\n[測試 2: 讀取檔案 (test_file.txt)]");
                string content = tools.ReadFile("test_file.txt");
                Console.WriteLine("內容內容:\n" + content);

                Console.WriteLine("\n[測試 3: 儲存產出]");
                string saveResult = tools.SaveFileOutput("test_output.txt", "這是由本地測試產出的內容。時間: " + DateTime.Now);
                Console.WriteLine(saveResult);

                Console.WriteLine("\n[測試 4: 檢查產出資料夾]");
                Console.WriteLine(tools.ListFiles("AI_Outputs"));

                Console.WriteLine("\n--- 本地工具測試完成 ---");
                Console.WriteLine("請在 Program.cs 中設定有效的 Gemini API Key 以測試完整的 AI 功能。");
                Console.WriteLine("\n長按任意鍵結束...");
                if (!Console.IsInputRedirected) { Console.ReadKey(); }
                return;
            }

            var client = new GeminiClient(apiKey);
            var fileToolsImpl = new FileTools();
            var serializer = new JavaScriptSerializer();

            Console.WriteLine("=== AI 檔案存取工作助手 ===");

            // 1. 定義工具架構 (符合 Gemini API 規範)
            var listFilesTool = client.CreateFunctionDeclaration(
                "list_files",
                "列出指定資料夾路徑下的所有檔案與子資料夾。若不指定則列出目前的根目錄。",
                new
                {
                    type = "object",
                    properties = new
                    {
                        subPath = new { type = "string", description = "相對路徑名稱 (選填)" }
                    }
                }
            );

            var readFileTool = client.CreateFunctionDeclaration(
                "read_file",
                "讀取特定檔案的內容。支援 .txt, .docx, .cs 等格式。不可進行修改。",
                new
                {
                    type = "object",
                    properties = new
                    {
                        fileName = new { type = "string", description = "要讀取的完整檔名 (包含副檔名)" }
                    },
                    required = new[] { "fileName" }
                }
            );

            var saveOutputTool = client.CreateFunctionDeclaration(
                "save_ai_output",
                "將 AI 產出的內容儲存為本機文件檔案。",
                new
                {
                    type = "object",
                    properties = new
                    {
                        fileName = new { type = "string", description = "儲存的檔名 (例如: report.txt)" },
                        content = new { type = "string", description = "要寫入檔案的文字內容" },
                        subFolderName = new { type = "string", description = "子資料夾名稱，預設為 AI_Outputs" }
                    },
                    required = new[] { "fileName", "content" }
                }
            );

            var allTools = client.DefineTools(listFilesTool, readFileTool, saveOutputTool);

            // 2. 測試對話範例
            Console.WriteLine("\n請輸入您的需求 (例如：'幫我列出目前有哪些檔案' 或 '讀取 readme.txt 並總結後存成 summary.txt')：");
            string userPrompt = Console.ReadLine();

            try
            {
                Console.WriteLine("\n[正在發送請求至 Gemini...]");
                string rawJson = await client.GenerateContentAsync(userPrompt, allTools.ToList<object>());

                // 3. 處理 Function Calling 邏輯
                var data = serializer.Deserialize<Dictionary<string, object>>(rawJson);
                var candidates = data["candidates"] as System.Collections.ArrayList;
                var firstCandidate = candidates[0] as Dictionary<string, object>;
                var content = firstCandidate["content"] as Dictionary<string, object>;
                var parts = content["parts"] as System.Collections.ArrayList;

                foreach (Dictionary<string, object> part in parts)
                {
                    if (part.ContainsKey("functionCall"))
                    {
                        var call = part["functionCall"] as Dictionary<string, object>;
                        string funcName = call["name"].ToString();
                        var argsDict = call["args"] as Dictionary<string, object>;

                        Console.WriteLine($"\n>> AI 請求執行工具: {funcName}");

                        string result = "";
                        switch (funcName)
                        {
                            case "list_files":
                                string subPath = argsDict.ContainsKey("subPath") ? argsDict["subPath"].ToString() : "";
                                result = fileToolsImpl.ListFiles(subPath);
                                break;
                            case "read_file":
                                string fn = argsDict["fileName"].ToString();
                                result = fileToolsImpl.ReadFile(fn);
                                break;
                            case "save_ai_output":
                                string sFn = argsDict["fileName"].ToString();
                                string sContent = argsDict["content"].ToString();
                                string sFolder = argsDict.ContainsKey("subFolderName") ? argsDict["subFolderName"].ToString() : "AI_Outputs";
                                result = fileToolsImpl.SaveFileOutput(sFn, sContent, sFolder);
                                break;
                        }

                        Console.WriteLine($">> 執行結果: \n{result}");
                        
                        // 注意：在實際完整的對話流程中，您應該將此 result 回傳給 Gemini 以完成對話。
                        // 這裡示範到此，讓您能確認 local 端的工具運作是否正確。
                        Console.WriteLine("\n[工具執行完畢]");
                    }
                    else if (part.ContainsKey("text"))
                    {
                        Console.WriteLine($"\nGemini 回應: {part["text"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n發生錯誤: {ex.Message}");
            }

            Console.WriteLine("\n任務展示完成，按任意鍵結束...");
            if (!Console.IsInputRedirected) { Console.ReadKey(); }
        }
    }
}
