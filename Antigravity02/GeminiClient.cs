using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Antigravity02
{
    public class GeminiClient
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private readonly JavaScriptSerializer _serializer;

        public GeminiClient(string apiKey, string model = "gemini-2.5-flash")
        {
            _apiKey = apiKey;
            _model = model;
            _httpClient = new HttpClient();
            _serializer = new JavaScriptSerializer();
        }

        public async Task<string> GenerateContentAsync(string prompt, List<object> tools = null)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                tools = tools
            };

            var json = _serializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API Error: {response.StatusCode}\n{responseJson}");
            }

            return responseJson;
        }

        /// <summary>
        /// 簡化版文字生成，直接回傳第一個 Content 的文字
        /// </summary>
        public async Task<string> AskAsync(string prompt)
        {
            var rawJson = await GenerateContentAsync(prompt);
            var data = _serializer.Deserialize<Dictionary<string, object>>(rawJson);
            
            try
            {
                var candidates = data["candidates"] as System.Collections.ArrayList;
                var firstCandidate = candidates[0] as Dictionary<string, object>;
                var content = firstCandidate["content"] as Dictionary<string, object>;
                var parts = content["parts"] as System.Collections.ArrayList;
                var firstPart = parts[0] as Dictionary<string, object>;
                return firstPart["text"].ToString();
            }
            catch (Exception ex)
            {
                return $"解析錯誤: {ex.Message}\n原始資料: {rawJson}";
            }
        }

        /// <summary>
        /// 輔助方法：定義多個工具
        /// </summary>
        public object[] DefineTools(params object[] functionDeclarations)
        {
            return new[]
            {
                new
                {
                    function_declarations = functionDeclarations
                }
            };
        }

        /// <summary>
        /// 建立單個 Function Declaration 物件
        /// </summary>
        public object CreateFunctionDeclaration(string name, string description, object parameters)
        {
            return new
            {
                name = name,
                description = description,
                parameters = parameters
            };
        }
    }
}
