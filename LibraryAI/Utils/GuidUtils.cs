namespace LibraryAI.Utils;

public class GuidUtils
{
    public static string GetFormattedGuid()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 14).ToUpper().Insert(4, "-").Insert(9, "-");
    }
}