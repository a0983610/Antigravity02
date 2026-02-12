using System;
using System.Threading.Tasks;

namespace Antigravity02.UI
{
    public class ConsoleUI : IAgentUI
    {
        public void ReportThinking(int iteration, string modelName)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[Thinking Iteration {iteration} ({modelName})] ...");
            Console.ResetColor();
        }

        public void ReportToolCall(string toolName, string args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Action: {toolName}");
            Console.ResetColor();
        }

        public void ReportToolResult(string resultSummary)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            string text = resultSummary ?? "(no result)";
            string summary = text.Length > 100 ? text.Substring(0, 100) + "..." : text;
            Console.WriteLine($"Result: {summary}");
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
            Console.WriteLine($"\nError: {message}");
            Console.ResetColor();
        }

        public void ReportInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public Task<bool> PromptContinueAsync(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n[PROMPT] {message} (Y/N): ");
            Console.ResetColor();
            string input = Console.ReadLine()?.Trim().ToLower();
            return Task.FromResult(input == "y" || input == "yes");
        }
    }
}
