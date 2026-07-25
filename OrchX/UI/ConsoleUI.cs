using System;
using System.Threading.Tasks;

namespace OrchX.UI
{
    public class ConsoleUI : IAgentUI
    {
        private readonly bool _autoApprove;

        /// <param name="autoApprove">非互動環境 (輸入/輸出被重導向) 時是否自動同意確認提示，由 .env 的 AUTO_APPROVE 控制；預設 false = 無人回應時拒絕</param>
        public ConsoleUI(bool autoApprove = false)
        {
            _autoApprove = autoApprove;
        }

        public void ReportThinking(int iteration, string modelName)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"\n[Thinking] Iteration {iteration} ({modelName}) ...");
            Console.ResetColor();
        }

        public void ReportToolCall(string toolName, string args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[Tool Call] {toolName}");
            Console.ResetColor();
        }

        public void ReportToolResult(string resultSummary)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            string text = resultSummary ?? "(no result)";
            string summary = text.Length > 100 ? text.Substring(0, 100) + "..." : text;
            Console.WriteLine($"[Tool Result] {summary}");
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
            Console.WriteLine($"\n[Error] {message}");
            Console.ResetColor();
        }

        public void ReportInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public async Task<bool> PromptContinueAsync(string message)
        {
            int selection = await PromptSelectionAsync(message, "Yes", "No");
            return selection == 0;
        }

        public Task<int> PromptSelectionAsync(string message, params string[] options)
        {
            if (options == null || options.Length == 0)
            {
                return Task.FromResult(-1);
            }

            bool canUseInteractiveMenu = true;
            try 
            { 
                if (Console.IsOutputRedirected || Console.IsInputRedirected)
                    canUseInteractiveMenu = false;
            } 
            catch { canUseInteractiveMenu = false; }

            if (!canUseInteractiveMenu)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[PROMPT] {message}");
                Console.ResetColor();
                for (int i = 0; i < options.Length; i++)
                {
                    Console.WriteLine($" {i + 1}. {options[i]}");
                }

                if (_autoApprove)
                {
                    // 使用者已透過 AUTO_APPROVE 明確授權自動放行，留下紀錄供事後稽核
                    Console.WriteLine($"[Auto-Approve] AUTO_APPROVE 已啟用，自動選擇: 1. {options[0]}");
                    return Task.FromResult(0);
                }

                Console.Write("請輸入選項數字 (無有效回答視為拒絕): ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= options.Length)
                {
                    return Task.FromResult(choice - 1);
                }

                // 非互動且無有效回答：安全預設為「未同意」，避免無人看管時自動放行危險操作
                Console.WriteLine("[System] 未收到有效回答，視為拒絕。(自動化場景請在 .env 設定 AUTO_APPROVE=true)");
                return Task.FromResult(-1);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[PROMPT] {message}");
            Console.ResetColor();

            // 確保底部有足夠空間繪製選單，避免繪製時視窗捲動導致 startTop 失效、重繪錯位
            try
            {
                int reservedLines = options.Length + 1;
                int maxTop = Console.BufferHeight - 1;
                if (Console.CursorTop + reservedLines > maxTop)
                {
                    int linesToPush = (Console.CursorTop + reservedLines) - maxTop;
                    for (int i = 0; i < linesToPush; i++)
                    {
                        Console.WriteLine();
                    }
                    Console.SetCursorPosition(0, Console.CursorTop - linesToPush);
                }
            }
            catch { }

            int selectedIndex = 0;
            int startTop = Console.CursorTop;
            bool cursorVisible = true;
            
            try { if (OperatingSystem.IsWindows()) { cursorVisible = Console.CursorVisible; Console.CursorVisible = false; } } catch { }

            int windowWidth = 80;
            try { windowWidth = Console.WindowWidth - 1; if (windowWidth < 1) windowWidth = 80; } catch { }

            try
            {
                while (true)
                {
                    try { Console.SetCursorPosition(0, startTop); } catch { }
                    for (int i = 0; i < options.Length; i++)
                    {
                        string prefix = (i == selectedIndex) ? " > " : "   ";
                        string line = $"{prefix}{i + 1}. {options[i]}";
                        if (line.Length < windowWidth) line = line.PadRight(windowWidth);

                        if (i == selectedIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.WriteLine(line);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine(line);
                        }
                    }

                    var keyInfo = Console.ReadKey(true);
                    var key = keyInfo.Key;

                    if (key == ConsoleKey.UpArrow)
                    {
                        selectedIndex--;
                        if (selectedIndex < 0) selectedIndex = options.Length - 1;
                    }
                    else if (key == ConsoleKey.DownArrow)
                    {
                        selectedIndex++;
                        if (selectedIndex >= options.Length) selectedIndex = 0;
                    }
                    else if (key >= ConsoleKey.D1 && key < ConsoleKey.D1 + options.Length)
                    {
                        selectedIndex = key - ConsoleKey.D1;
                        break;
                    }
                    else if (key >= ConsoleKey.NumPad1 && key < ConsoleKey.NumPad1 + options.Length)
                    {
                        selectedIndex = key - ConsoleKey.NumPad1;
                        break;
                    }
                    else if (key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }

                try { Console.SetCursorPosition(0, startTop); } catch { }
                for (int i = 0; i < options.Length; i++)
                {
                    Console.WriteLine(new string(' ', windowWidth));
                }
                try { Console.SetCursorPosition(0, startTop); } catch { }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"已選擇: {options[selectedIndex]}");
                Console.ResetColor();

                return Task.FromResult(selectedIndex);
            }
            finally
            {
                try { if (OperatingSystem.IsWindows()) { Console.CursorVisible = cursorVisible; } } catch { }
            }
        }
    }
}
