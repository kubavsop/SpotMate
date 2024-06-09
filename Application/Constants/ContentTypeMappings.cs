namespace SpotMate.Application.Constants;

public static class ContentTypeMappings
{
    public static Dictionary<string, string> TypeMappings { get; } = new Dictionary<string, string>
    {
        { ".jpe", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".jpg", "image/jpeg" },
        { ".png", "image/png" }
    };
    
    public static Dictionary<string, string> ReverseTypeMappings { get; } = new Dictionary<string, string>
    {
        { "image/jpeg", ".jpe" },
        { "image/png", ".png" }
    };
}