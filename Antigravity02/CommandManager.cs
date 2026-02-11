using System;
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

            if (cmd.Equals("/save", StringComparison.OrdinalIgnoreCase) || cmd.StartsWith("/save ", StringComparison.OrdinalIgnoreCase))
            {
                string path = "chat_history.json";
                if (cmd.Length > 6)
                {
                    string arg = cmd.Substring(6).Trim();
                    if (!string.IsNullOrEmpty(arg)) path = arg;
                }
                agent.SaveChatHistory(path);
                Console.WriteLine($"[System] Chat history saved to {path}");
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
                agent.LoadChatHistory(path);
                Console.WriteLine($"[System] Chat history loaded from {path}");
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
            Console.WriteLine("  /save [path]  : Save chat history to file (default: chat_history.json)");
            Console.WriteLine("  /load [path]  : Load chat history from file (default: chat_history.json)");
            Console.WriteLine("  /help         : Show this help message");
            Console.WriteLine("  /exit         : Exit the program");
            Console.WriteLine("==========================\n");
            Console.ResetColor();
        }
    }
}
