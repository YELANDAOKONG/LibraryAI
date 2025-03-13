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
            new ApiKeyCredential("sk-"),
            options
        );

        var models = service.GetOpenAIModelClient().GetModels();
        Console.WriteLine(models.Value.Count);
        foreach (var model in models.Value)
        {
            Console.WriteLine($"{model.Id}, {model.CreatedAt}, {model.OwnedBy}");
        }
        
        // var chat = service.GetChatClient("deepseek-chat");
        // // var data = chat.CompleteChat(new AssistantChatMessage("You are a helpful robot."), new UserChatMessage("Hello, World!"));
        // // Console.WriteLine(data.Value.Content[data.Value.Content.Count - 1].Text);
        // var data = chat.CompleteChatStreaming(
        //     new AssistantChatMessage("You are a helpful robot."),
        //     new UserChatMessage("你好！你是谁？"),
        //     new AssistantChatMessage("你好！我是一个人工智能助手，旨在帮助你解答问题、提供信息或进行对话。你可以问我任何问题，我会尽力提供帮助。有什么我可以为你做的吗？"),
        //     new UserChatMessage("好的！请帮我写一段代码：\n在Ubuntu 24.04LTS上安装所有LTS JDK版本和JDK 23")
        // );
        //
        // foreach (var update in data)
        // {
        //     Console.Write(update.ContentUpdate[update.ContentUpdate.Count - 1].Text);
        // }
        
        

    }
}