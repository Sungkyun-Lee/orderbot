namespace OrderBot.Services;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;               // System.Web.HttpUtility NuGet 필요
using OrderBot.Models;

public sealed class CoupangOrderClient
{
    private readonly HttpClient _http;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _vendorId;
    private const string Host = "https://api-gateway.coupang.com";

    public CoupangOrderClient(IHttpClientFactory factory, IConfiguration cfg)
    {
        _http = factory.CreateClient();
        _accessKey = ReadSecret("COUPANG_ACCESS_KEY");
        _secretKey = ReadSecret("COUPANG_SECRET_KEY");
        _vendorId = cfg["COUPANG_VENDOR_ID"]!;
    }

    public async Task<OrderSheetResponse?> GetOrdersAsync(
        DateTime from, DateTime to, string status = "ACCEPT", int max = 50)
    {
        //string path = $"/v2/providers/openapi/apis/api/v5/vendors/{_vendorId}/ordersheets";
        string path = $"/v2/providers/openapi/apis/api/v4/vendors/{_vendorId}/ordersheets";
        var query = HttpUtility.ParseQueryString(string.Empty);
        //query["createdAtFrom"] = $"{from:yyyy-MM-dd}+09:00";
        //query["createdAtTo"] = $"{to:yyyy-MM-dd}+09:00";
        const string fmt = "yyyy-MM-dd'T'HH:mm:sszzz";  // 2025-08-04T09:30:00+09:00
        query["createdAtFrom"] = from.ToString(fmt);
        query["createdAtTo"] = to.ToString(fmt);
        query["status"] = status;
        query["maxPerPage"] = max.ToString();

        var uri = new UriBuilder($"{Host}{path}") { Query = query.ToString() }.Uri;
        var req = new HttpRequestMessage(HttpMethod.Get, uri);
        req.Headers.Add("Authorization", BuildHmac("GET", path, query.ToString()));

        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();

        await using var stream = await res.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<OrderSheetResponse>(stream);
    }

    private string BuildHmac(string method, string path, string query)
    {
        string datetime = DateTime.UtcNow.ToString("yyMMddTHHmmssZ");
        string message = $"{datetime}{method}{path}{query}";
        using var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_secretKey));
        string sig = Convert.ToHexString(hmac.ComputeHash(Encoding.ASCII.GetBytes(message))).ToLower();
        return $"CEA algorithm=HmacSHA256, access-key={_accessKey}, signed-date={datetime}, signature={sig}";
    }

    private static string ReadSecret(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? File.ReadAllText($"/etc/secrets/{key.ToLower()}").Trim();
}