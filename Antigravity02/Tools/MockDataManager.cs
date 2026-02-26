using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Antigravity02.Tools
{
    /// <summary>
    /// 集中管理所有 AI 模型的 Mock Data 讀取與建立邏輯。
    /// 每個 provider 各自維護獨立的計數器與訊息狀態，互不干擾。
    /// </summary>
    public class MockDataManager
    {
        // 每個 provider 各自維護獨立的計數器與「是否已顯示首次訊息」狀態
        private static readonly Dictionary<string, int> _mockCounters = new Dictionary<string, int>();
        private static readonly Dictionary<string, bool> _mockMessageShown = new Dictionary<string, bool>();

        /// <summary>
        /// 取得指定 provider 的模擬回應資料。
        /// providerName 會被正規化為小寫，以確保檔案命名一致（例如 "gemini_mock_response_0001.json"）。
        /// </summary>
        public static string GetMockResponse(string providerName = "gemini")
        {
            // 正規化為小寫，確保與既有檔案命名一致
            string normalizedName = providerName.ToLower();

            // 初始化該 provider 的狀態（若尚未存在）
            if (!_mockCounters.ContainsKey(normalizedName))
            {
                _mockCounters[normalizedName] = 1;
                _mockMessageShown[normalizedName] = false;
            }

            int counter = _mockCounters[normalizedName];
            string basePath = Environment.CurrentDirectory;
            string mockFileName = $"{normalizedName}_mock_response_{counter:D4}.json";
            string mockFilePath = Path.Combine(basePath, "MockData", mockFileName);

            if (File.Exists(mockFilePath))
            {
                if (!_mockMessageShown[normalizedName])
                {
                    Console.WriteLine($"\n[{providerName}Client] 尚未設定 API KEY，讀取模擬回應資料 ({mockFilePath})...");
                    _mockMessageShown[normalizedName] = true;
                }
                else
                {
                    Console.WriteLine($"\n[{providerName}Client] 讀取模擬回應資料 ({mockFilePath})...");
                }

                string mockFileContent = File.ReadAllText(mockFilePath);
                _mockCounters[normalizedName] = counter + 1;
                return mockFileContent;
            }
            else
            {
                Console.WriteLine($"\n[System] 找不到模擬回應檔案，正在自動建立空白檔案: {mockFilePath}");
                
                var directory = Path.GetDirectoryName(mockFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string defaultMockContent = GetDefaultMockContent(normalizedName);
                File.WriteAllText(mockFilePath, defaultMockContent, Encoding.UTF8);
                
                throw new Exception($"尚未設定 API KEY，且找不到模擬回應檔案。\n系統已自動於路徑建立空白檔案：{mockFilePath}\n請在該檔案中的 'text' 欄位(或對應欄位)填入您想測試的回應內容後再試一次。");
            }
        }

        private static string GetDefaultMockContent(string providerName)
        {
            if (providerName == "gemini")
            {
                return @"{
  ""candidates"": [
    {
      ""content"": {
        ""parts"": [
          {
            ""text"": ""請在這裡填寫你想測試的回應內容。\n支援多行與 Markdown 格式。\n例如：\n\n這是一個測試回應。""
          }
        ],
        ""role"": ""model""
      },
      ""finishReason"": ""STOP""
    }
  ]
}";
            }
            
            return @"{
  ""mock_response"": ""請在這裡填寫你想測試的回應內容。""
}";
        }
    }
}
