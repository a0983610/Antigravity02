using System.Collections.Generic;
using System.Threading.Tasks;
using Antigravity02.AIClient;
using Antigravity02.UI;

namespace Antigravity02.Agents
{
    /// <summary>
    /// 萬能 Agent：整合多個功能模組，具備全方位的工具箱
    /// </summary>
    public class UniversalAgent : BaseAgent
    {
        private readonly List<IAgentModule> _modules = new List<IAgentModule>();

        public UniversalAgent(string apiKey, string smartModel, string fastModel) : base(apiKey, smartModel, fastModel)
        {
            // 在此註冊所有模組
            // 只有 Smart 和 Fast 為不同模型時，才啟用摘要功能（避免浪費相同模型的 API 呼叫）
            bool hasDifferentFastModel = SmartClient.ModelName != FastClient.ModelName;
            RegisterModule(new FileModule(hasDifferentFastModel ? FastClient : null));
            RegisterModule(new HttpModule());
            RegisterModule(new AIControlModule(this.SetModelMode, () => this.IsSmartMode));
            RegisterModule(new MultiAgentModule(apiKey, smartModel));
            // 未來可以輕鬆加入更多模組，例如：
            // RegisterModule(new WebSearchModule());
            // RegisterModule(new DatabaseModule());
            
            SystemInstruction = "你是一個高效能的自動化主控 AI，負責調度各種工具與專家來協助使用者。你可以操作檔案、發送 HTTP 請求，或使用 'consult_expert' 諮詢特定領域的 AI 專家來獲得深度建議。請專業且準確地回應。";
            
            InitializeToolDeclarations();
        }

        public void RegisterModule(IAgentModule module)
        {
            _modules.Add(module);
        }

        private void InitializeToolDeclarations()
        {
            var allDeclarations = new List<object>();
            foreach (var module in _modules)
            {
                allDeclarations.AddRange(module.GetToolDeclarations(Client));
            }

            if (allDeclarations.Count > 0)
            {
                ToolDeclarations = new List<object>(Client.DefineTools(allDeclarations.ToArray()));
            }
        }

        /// <summary>
        /// 模型模式切換時，重新初始化工具宣告以匹配新模型
        /// </summary>
        protected override void OnModelModeChanged()
        {
            InitializeToolDeclarations();
        }

        protected override async Task<string> ProcessToolCallAsync(string funcName, Dictionary<string, object> args, IAgentUI ui)
        {
            foreach (var module in _modules)
            {
                string result = await module.TryHandleToolCallAsync(funcName, args, ui);
                if (result != null)
                {
                    return result;
                }
            }

            return "Error: Unknown tool.";
        }
    }
}
