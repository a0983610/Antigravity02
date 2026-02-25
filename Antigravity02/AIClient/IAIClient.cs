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
        System.Collections.ArrayList ExtractResponseParts(Dictionary<string, object> data, out Dictionary<string, object> modelContent);
        string ExtractTextFromResponseData(Dictionary<string, object> data);
    }
}
