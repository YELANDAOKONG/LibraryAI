using System.ClientModel;
using OpenAI;

namespace LibraryAI.Core;

public class ClientBuilder
{
    public static OpenAIClient Build(string endpoint, string key)
    {
        var options = new OpenAIClientOptions();
        options.Endpoint = new Uri(endpoint);
        return new OpenAIClient(new ApiKeyCredential(key), options);
    } 
}