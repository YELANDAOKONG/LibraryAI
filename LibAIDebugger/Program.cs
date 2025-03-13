using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace LibAIDebugger;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var options = new OpenAIClientOptions();
        options.Endpoint = new Uri("https://api.deepseek.com/");
        var service = new OpenAIClient(
            new ApiKeyCredential(""),
            options
        );

        var chat = service.GetChatClient("deepseek-chat");
        var data = chat.CompleteChat(new AssistantChatMessage("You are a helpful robot."), new UserChatMessage("Hello, World!"));
        Console.WriteLine(data.Value.Content[data.Value.Content.Count - 1].Text);
        

    }
}