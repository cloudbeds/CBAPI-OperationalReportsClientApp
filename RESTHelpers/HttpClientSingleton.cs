using System.Net.Http;

internal static class HttpRequestsSingleton
{
    static HttpClient s_httpClient = new HttpClient();

    public static HttpClient Client
    {
        get {
            return s_httpClient;
        }
    }
}
