using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OrchX.AIClient;
using OrchX.Tools;
using OrchX.UI;

namespace OrchX.Agents
{
    /// <summary>
    /// 提供模組的基礎實作，包含參數檢核等共用功能。
    /// </summary>
    public abstract class BaseAgentModule : IAgentModule
    {
        private Dictionary<string, string[]> _requiredArgsCache;

        /// <summary>
        /// 獲取此模組提供的所有工具宣告，並在第一次呼叫時自動建立參數檢核快取。
        /// 子類別必須實作 <see cref="BuildToolDeclarations"/> 來提供宣告。
        /// </summary>
        public IEnumerable<object> GetToolDeclarations(IAIClient client)
        {
            // 必須先具體化為 List，因為 BuildToolDeclarations 使用 yield return，
            // 產生的 IEnumerable 只能遍歷一次。若不具體化，快取 foreach 會消耗掉迭代器，
            // 導致最終回傳給外部呼叫者的結果為空。
            var decls = new List<object>(BuildToolDeclarations(client));
            if (_requiredArgsCache == null)
            {
                _requiredArgsCache = new Dictionary<string, string[]>();
                foreach (var decl in decls)
                {
                    try
                    {
                        var json = JsonTools.Serialize(decl);
                        var dict = JsonTools.Deserialize<Dictionary<string, object>>(json);
                        
                        if (dict != null && dict.ContainsKey("name"))
                        {
                            string funcName = dict["name"]?.ToString();
                            if (!string.IsNullOrEmpty(funcName))
                            {
                                var requiredList = new List<string>();
                                if (dict.ContainsKey("parameters") && dict["parameters"] is Dictionary<string, object> parameters)
                                {
                                    if (parameters.ContainsKey("required") && parameters["required"] is System.Collections.ArrayList reqList)
                                    {
                                        foreach(var r in reqList)
                                        {
                                            requiredList.Add(r?.ToString());
                                        }
                                    }
                                }
                                _requiredArgsCache[funcName] = requiredList.ToArray();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore parsing errors for individual declarations
                        UsageLogger.LogError($"[BaseAgentModule] Failed to cache required args: {ex.Message}");
                    }
                }
            }
            return decls;
        }

        /// <summary>
        /// 子類別實作此方法來定義工具宣告。
        /// </summary>
        protected abstract IEnumerable<object> BuildToolDeclarations(IAIClient client);

        public abstract Task<string> TryHandleToolCallAsync(string funcName, Dictionary<string, object> args, IAgentUI ui, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 檢查工具呼叫是否包含所有的必要參數，且不為空值。
        /// 若缺少必要參數，會嘗試比對常見的 AI 幻覺別名 (Aliases) 進行自動修復。
        /// </summary>
        protected bool ValidateRequiredArgs(Dictionary<string, object> args, string[] requiredParams, out string errorMessage)
        {
            var aliasMapping = new Dictionary<string, string[]>
            {
                { "filePath", new[] { "path", "file", "filename", "file_path", "sourcePath", "destinationPath" } },
                { "sourcePath", new[] { "source", "source_path", "from", "src", "path" } },
                { "destinationPath", new[] { "destination", "destination_path", "to", "dest", "targetPath", "target" } },
                { "query", new[] { "search", "keyword", "text", "q" } },
                { "content", new[] { "text", "data", "body", "newContent" } },
                { "newContent", new[] { "content", "text", "data" } }
            };

            var missing = new List<string>();
            foreach (var p in requiredParams)
            {
                if (!args.ContainsKey(p) || args[p] == null || string.IsNullOrWhiteSpace(args[p].ToString()))
                {
                    bool fixedViaAlias = false;
                    if (aliasMapping.ContainsKey(p))
                    {
                        foreach (var alias in aliasMapping[p])
                        {
                            if (args.ContainsKey(alias) && args[alias] != null && !string.IsNullOrWhiteSpace(args[alias].ToString()))
                            {
                                args[p] = args[alias]; // 自動修復參數名稱
                                fixedViaAlias = true;
                                break;
                            }
                        }
                    }

                    if (!fixedViaAlias)
                    {
                        missing.Add(p);
                    }
                }
            }

            if (missing.Count > 0)
            {
                errorMessage = $"System Error: Missing or empty required argument(s): {string.Join(", ", missing)}. ACTION REQUIRED: You must immediately use the tool calling syntax to invoke this tool again with the corrected parameters. Do not apologize in plain text.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// 便利方法：從 Tool Declaration 的自動快取中取出 required 參數並檢查。
        /// 如果沒有傳入所需參數或者為空時回傳錯誤訊息字串。全部通過則回傳 null。
        /// </summary>
        protected string CheckRequiredArgs(string funcName, Dictionary<string, object> args)
        {
            if (_requiredArgsCache == null || !_requiredArgsCache.TryGetValue(funcName, out string[] requiredParams))
            {
                // 如果沒有快取資訊，則無法檢查，直接回傳 null (允許通過)
                return null;
            }

            if (requiredParams.Length == 0) return null;

            if (!ValidateRequiredArgs(args, requiredParams, out string error))
            {
                return error;
            }
            return null;
        }
    }
}
