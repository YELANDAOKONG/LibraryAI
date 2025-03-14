namespace LibraryAI.Utils;

public class UuidUtils
{
    public static string GetFormattedUuid()
    {
        return Guid.NewGuid().ToString("N");
    }
}