using System.Collections.Generic;
using System.Threading.Tasks;

namespace Antigravity02.AIClient
{
    public interface IAIClient
    {
        string ModelName { get; }
        Task<string> GenerateContentAsync(GenerateContentRequest request);
        object CreateSimpleContents(string prompt);
        object[] DefineTools(params object[] functionDeclarations);
        object CreateFunctionDeclaration(string name, string description, object parameters);
    }
}
