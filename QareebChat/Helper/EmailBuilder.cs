namespace QareebChat.Helper;

public class EmailBuilder
{
    public static string GenerateEmailBody(string template, Dictionary<string, string> emailBody)
    {
        var teplatePath = $"{Directory.GetCurrentDirectory()}/Templates/{template}.html";
        var streamReader = new StreamReader(teplatePath) ;
        var body = streamReader.ReadToEnd() ;
        streamReader.Close() ;
        foreach ( var item in emailBody )
            body = body.Replace(item.Key, item.Value) ;
        return body ;
    }
}