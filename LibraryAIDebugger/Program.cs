using System.ClientModel;
using LibraryAI.Core;
using LibraryAI.Tools;
using LibraryAIDebugger.Demos;
using OpenAI;
using OpenAI.Chat;

namespace LibraryAIDebugger;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        // var service = ClientBuilder.Build(
        //     "https://api.siliconflow.cn/v1/",
        //     Environment.GetEnvironmentVariable("KEY")!
        // );

        // var models = service.GetOpenAIModelClient().GetModels();
        // Console.WriteLine($"Got Models: {models.Value.Count}");
        // foreach (var model in models.Value)
        // {
        //     Console.WriteLine($"{model.Id}, {model.CreatedAt}, {model.OwnedBy}");
        // }
        
        // TestChunkTools();
        // VecDbDemo.Run();
        // ChunkDemo.Run();
        VecDbLocalDemo.Run();
        
        // var chat = service.GetChatClient("Qwen/Qwen2.5-7B-Instruct");
        // // var data = chat.CompleteChat(new AssistantChatMessage("You are a helpful robot."), new UserChatMessage("Hello, World!"));
        // // Console.WriteLine(data.Value.Content[data.Value.Content.Count - 1].Text);
        // var data = chat.CompleteChatStreaming(
        //     new AssistantChatMessage("You are a helpful robot."),
        //     new UserChatMessage("你好！你是谁？"),
        //     new AssistantChatMessage("你好！我是一个人工智能助手，旨在帮助你解答问题、提供信息或进行对话。你可以问我任何问题，我会尽力提供帮助。有什么我可以为你做的吗？"),
        //     new UserChatMessage("好的！请帮我写一段代码：\n在Ubuntu 22.04LTS上安装所有LTS JDK版本")
        // );
        //
        // foreach (var update in data)
        // {
        //     Console.Write(update.ContentUpdate[^1].Text);
        // }

        // var embed = service.GetEmbeddingClient("BAAI/bge-m3");
        // var result = embed.GenerateEmbedding("Hello World");
        // Console.WriteLine(result.Value.Index);
        // foreach (var data in result.Value.ToFloats().ToArray())
        // {
        //     Console.WriteLine(data);
        // }


    }
    
    
    public static void TestChunkTools()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file == null)
        {
            // throw new Exception("FILE environment variable not set");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("% FILE environment variable not set");
            Console.ResetColor();
            return;
        }

        int chunks = 0;
        
        void PrintChunk(string chunk)
        {
            chunks++;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"% Got chunk: ");
            Console.ResetColor();
            Console.WriteLine(chunk);
        }
        
        using var fs = File.OpenRead(file);
        var chunker = new ChunkTools(
            fs,
            chunkSize: 1024,
            chunkOverlap: 256,
            chunkCallback: chunk => PrintChunk(chunk)
        );
        chunker.Chunk();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"% Done: Got {chunks} Chunks");
        Console.ResetColor();
    }
}