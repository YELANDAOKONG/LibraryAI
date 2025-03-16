using System.Text.Json;

namespace ProxyAI.Core;

[Serializable]
public class StandardResponse
{
    public int Code { get; set; } = 0;
    public string Message { get; set; } = "";
    public string Uuid { get; set; } = "";
    public long Time { get; set; } = 0;
    public object? Data { get; set; } = null;
    
    public static StandardResponse Create(int code, string message, object? data = null)
    {
        return new StandardResponse()
        {
            Code = code,
            Message = message,
            Data = data,
            Time = ConvertDateTime(DateTime.Now),
            Uuid = Guid.NewGuid().ToString()
        };
    }
    
    private static long ConvertDateTime(DateTime time)
    {
        DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1, 0, 0, 0));
        long t = (time.Ticks - startTime.Ticks) / 10000;
        return t;
    }

    public static string CreateJson(int code, string message, object? data = null)
    {
        return JsonSerializer.Serialize(Create(code, message, data));
    }

    public static void BuildResponseHeader(HttpResponse response)
    {
        response.Headers.ContentType = "application/json";
    }
}
