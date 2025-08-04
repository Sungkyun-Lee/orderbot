namespace OrderBot.Services;

using System.Net.Http.Headers;
using System.Text.Json;

public sealed class KakaoTalkNotifier
{
    private readonly HttpClient _http;
    private readonly string _token;

    public KakaoTalkNotifier(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
        _token = ReadSecret("KAKAO_USER_TOKEN");
    }

    public async Task SendTextAsync(string text)
    {
        var payload = new
        {
            object_type = "text",
            text,
            link = new
            {
                web_url = "https://wing.coupang.com",
                mobile_web_url = "https://wing.coupang.com"
            },
            button_title = "주문 확인"
        };

        var req = new HttpRequestMessage(
            HttpMethod.Post, "https://kapi.kakao.com/v2/api/talk/memo/default/send");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        req.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>(
                "template_object", JsonSerializer.Serialize(payload))
        });

        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    private static string ReadSecret(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? File.ReadAllText($"/etc/secrets/{key.ToLower()}").Trim();
}