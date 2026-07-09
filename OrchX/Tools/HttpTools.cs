using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OrchX.Tools
{
    /// <summary>
    /// 底層 HTTP 工具類別，負責處理實際的網路通訊
    /// </summary>
    public class HttpTools
    {
        // 明確設定逾時，不依賴隱含預設；進行中的請求可由 CancellationToken 中斷
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };


        public async Task<string> GetAsync(string url, string headersJson = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddHeaders(request, headersJson);
                    using (var response = await _httpClient.SendAsync(request, cancellationToken))
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return $"Status: {response.StatusCode}\nContent: {content}";
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // 使用者中斷需上拋終止本輪，不可包裝成工具結果字串
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"HttpTools(Get) Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> PostAsync(string url, string body, string contentType = "application/json", string headersJson = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    // 先設定 Content 再加 headers，內容類 header (如 Content-Type) 才有 fallback 目標
                    request.Content = new StringContent(body, Encoding.UTF8, contentType);
                    AddHeaders(request, headersJson);

                    using (var response = await _httpClient.SendAsync(request, cancellationToken))
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return $"Status: {response.StatusCode}\nContent: {content}";
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // 使用者中斷需上拋終止本輪，不可包裝成工具結果字串
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"HttpTools(Post) Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private void AddHeaders(HttpRequestMessage request, string headersJson)
        {
            if (string.IsNullOrEmpty(headersJson)) return;

            try
            {
                var headerDict = JsonTools.Deserialize<Dictionary<string, string>>(headersJson);
                if (headerDict != null)
                {
                    foreach (var kvp in headerDict)
                    {
                        // 請求層加不進去的內容類 header (如 Content-Type)，改加到 Content.Headers，皆失敗才記 log
                        if (!request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value) &&
                            !(request.Content?.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value) ?? false))
                        {
                            UsageLogger.LogError($"HttpTools: 無法加入 header '{kvp.Key}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"HttpTools Header Parse Error: {ex.Message}");
            }
        }
    }
}
