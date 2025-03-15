using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using LibraryAI.Core;
using OpenAI;
using OpenAI.Embeddings;

namespace LibraryAI.Tools;

public class VectorTools
{
    public static (float[]?, PipelineResponse?) Generate(
        string apiEndpoint,
        string key,
        string data, 
        string model,
        EmbeddingGenerationOptions? options = null, 
        bool throwExceptions = false, 
        bool compatibility = false)
    {
        return Generate(ClientBuilder.Build(apiEndpoint, key), data, model, options, throwExceptions, compatibility);
    }
    
    public static (float[]?, PipelineResponse?) Generate(
        OpenAIClient client, 
        string data, 
        string model,
        EmbeddingGenerationOptions? options = null, 
        bool throwExceptions = false, 
        bool compatibility = false)
    {
        return Generate(client.GetEmbeddingClient(model), data, model, options, throwExceptions, compatibility);
    }

    public static (float[]?, PipelineResponse?) Generate(
        EmbeddingClient client, 
        string data, 
        string model,
        EmbeddingGenerationOptions? options = null, 
        bool throwExceptions = false, 
        bool compatibility = false)
    {
        if (compatibility)
        {
            return GenerateEmbeddedCompatibility(client, data, model, options, throwExceptions);
        }
        return GenerateEmbedded(client, data, options, throwExceptions);
    }
    
    public static (float[]?, PipelineResponse?) GenerateEmbedded(EmbeddingClient client, string data, EmbeddingGenerationOptions? options = null, bool throwExceptions = false)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(data);
        
        try
        {
            var result = client.GenerateEmbedding(data, options);
            return (result.Value.ToFloats().ToArray(), result.GetRawResponse());
        }
        catch (Exception e)
        {
            if (throwExceptions)
            {
                throw;
            }
            return (null, null);
        }
    }
    
    public static (float[]?, PipelineResponse?) GenerateEmbeddedCompatibility(EmbeddingClient client, string data, string model, EmbeddingGenerationOptions? options = null, bool throwExceptions = false)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentException.ThrowIfNullOrEmpty(data);
            ArgumentException.ThrowIfNullOrEmpty(model);
            
            BinaryData input = BinaryData.FromObjectAsJson(new
            {
                model = model,
                input = data,
                encoding_format = "float"
            });
            using BinaryContent content = BinaryContent.Create(input);
            var result = client.GenerateEmbeddings(content);
            BinaryData output = result.GetRawResponse().Content;
            using JsonDocument outputAsJson = JsonDocument.Parse(output.ToString());
            JsonElement vector = outputAsJson.RootElement
                .GetProperty("data"u8)[0]
                .GetProperty("embedding"u8);
            
            var floats = new float[vector.GetArrayLength()];
            int floatsCounter = 0;
            foreach (JsonElement element in vector.EnumerateArray())
            {
                floats[floatsCounter] = (float)element.GetDouble();
                floatsCounter++;
            }
            return (floats, result.GetRawResponse());
        }
        catch (Exception e)
        {
            if (throwExceptions)
            {
                throw;
            }

            return (null, null);
        }
    }
}