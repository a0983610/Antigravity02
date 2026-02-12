using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Antigravity02.Agents;

namespace Antigravity02
{
    public static class CommandManager
    {
        public static bool TryHandleCommand(string input, BaseAgent agent, out bool shouldExit)
        {
            shouldExit = false;
            if (string.IsNullOrEmpty(input)) return false;

            // 只處理以 / 開頭的指令
            if (!input.StartsWith("/")) return false;

            string cmd = input.Trim();

            if (cmd.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                shouldExit = true;
                return true;
            }

            if (cmd.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp();
                return true;
            }

            if (cmd.Equals("/new", StringComparison.OrdinalIgnoreCase))
            {
                agent.ClearChatHistory();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[System] 對話紀錄已清除，開始新的對話。");
                Console.ResetColor();
                return true;
            }

            if (cmd.Equals("/save", StringComparison.OrdinalIgnoreCase) || cmd.StartsWith("/save ", StringComparison.OrdinalIgnoreCase))
            {
                string path = "chat_history.json";
                if (cmd.Length > 6)
                {
                    string arg = cmd.Substring(6).Trim();
                    if (!string.IsNullOrEmpty(arg)) path = arg;
                }
                if (agent.SaveChatHistory(path))
                {
                    Console.WriteLine($"[System] Chat history saved to {path}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[System] Failed to save chat history to {path}");
                    Console.ResetColor();
                }
                return true;
            }

            if (cmd.Equals("/load", StringComparison.OrdinalIgnoreCase) || cmd.StartsWith("/load ", StringComparison.OrdinalIgnoreCase))
            {
                string path = "chat_history.json";
                if (cmd.Length > 6)
                {
                    string arg = cmd.Substring(6).Trim();
                    if (!string.IsNullOrEmpty(arg)) path = arg;
                }
                if (agent.LoadChatHistory(path))
                {
                    Console.WriteLine($"[System] Chat history loaded from {path}");
                    DisplayChatHistory(agent.GetChatHistory());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[System] Failed to load chat history from {path}");
                    Console.ResetColor();
                }
                return true;
            }

            // 如果是 / 開頭但未知的指令，提示使用者
            Console.WriteLine($"[System] Unknown command: {cmd}. Type /help for list of commands.");
            return true;
        }

        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Available Commands ===");
            Console.WriteLine("  /new          : Start a new conversation (clear chat history)");
            Console.WriteLine("  /save [path]  : Save chat history to file (default: chat_history.json)");
            Console.WriteLine("  /load [path]  : Load chat history from file (default: chat_history.json)");
            Console.WriteLine("  /help         : Show this help message");
            Console.WriteLine("  /exit         : Exit the program");
            Console.WriteLine("==========================\n");
            Console.ResetColor();
        }

        /// <summary>
        /// 將對話紀錄以摘要方式顯示在畫面上，讓使用者了解對話脈絡
        /// </summary>
        private static void DisplayChatHistory(ReadOnlyCollection<object> chatHistory)
        {
            if (chatHistory == null || chatHistory.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[System] 對話紀錄為空。");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n=== 載入的對話紀錄 ===");
            Console.ResetColor();

            foreach (var entry in chatHistory)
            {
                var dict = entry as Dictionary<string, object>;
                if (dict == null) continue;

                string role = dict.ContainsKey("role") ? dict["role"]?.ToString() : null;
                var parts = dict.ContainsKey("parts") ? dict["parts"] as ArrayList : null;
                if (parts == null) continue;

                if (role == "user")
                {
                    foreach (Dictionary<string, object> part in parts)
                    {
                        if (part.ContainsKey("text"))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\nUser: {part["text"]}");
                            Console.ResetColor();
                        }
                    }
                }
                else if (role == "model")
                {
                    foreach (Dictionary<string, object> part in parts)
                    {
                        if (part.ContainsKey("text"))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"\nAI: {part["text"]}");
                            Console.ResetColor();
                        }
                        if (part.ContainsKey("functionCall"))
                        {
                            var call = part["functionCall"] as Dictionary<string, object>;
                            if (call != null)
                            {
                                string funcName = call.ContainsKey("name") ? call["name"]?.ToString() : "?";
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"  [Tool Call] {funcName}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
                else if (role == "function")
                {
                    foreach (Dictionary<string, object> part in parts)
                    {
                        if (part.ContainsKey("functionResponse"))
                        {
                            var resp = part["functionResponse"] as Dictionary<string, object>;
                            if (resp != null)
                            {
                                string funcName = resp.ContainsKey("name") ? resp["name"]?.ToString() : "?";
                                string content = "";
                                if (resp.ContainsKey("response"))
                                {
                                    var respBody = resp["response"] as Dictionary<string, object>;
                                    if (respBody != null && respBody.ContainsKey("content"))
                                    {
                                        content = respBody["content"]?.ToString() ?? "";
                                    }
                                }
                                string summary = content.Length > 80 ? content.Substring(0, 80) + "..." : content;
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine($"  [Tool Result] {funcName}: {summary}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=== 對話紀錄結束 ===\n");
            Console.ResetColor();
        }
    }
}
