using System;

namespace Antigravity02.Config
{
    public static class AgentConfig
    {
        public static string GetSystemInstruction()
        {
            return "你是一個高效能的自動化主控 AI，負責調度各種工具與專家來協助使用者。你可以操作檔案、發送 HTTP 請求，或使用 'consult_expert' 諮詢特定領域的 AI 專家來獲得深度建議。請專業且準確地回應。";
        }
    }
}
