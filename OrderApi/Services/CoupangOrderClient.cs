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
    private readonly ILogger<CoupangOrderClient> _logger;   // ← 필드 추가
    private const string Host = "https://api-gateway.coupang.com";

    public CoupangOrderClient(HttpClient http, IConfiguration cfg, ILogger<CoupangOrderClient> logger)
    {
        _http = http;
        _accessKey = ReadSecret("COUPANG_ACCESS_KEY");
        _secretKey = ReadSecret("COUPANG_SECRET_KEY");
        _vendorId = cfg["COUPANG_VENDOR_ID"]!;
        _logger = logger;
    }

    public async Task<OrderSheetResponse?> GetOrdersAsync(
        DateTime from, DateTime to, string? status = null, int max = 50)
    {
        //string path = $"/v2/providers/openapi/apis/api/v5/vendors/{_vendorId}/ordersheets";
        string path = $"/v2/providers/openapi/apis/api/v5/vendors/{_vendorId}/ordersheets";
        var query = HttpUtility.ParseQueryString(string.Empty);
        //const string dateFmt = "yyyy-MM-dd%2B09:00";           // ← 날짜만
        ////const string dateFmt = "yyyy-MM-dd";           // ← 날짜만
        //query["createdAtFrom"] = from.ToString(dateFmt);   // 2025-08-05
        //query["createdAtTo"] = to.ToString(dateFmt);     // 2025-08-05
        //query["startTime"] = from.ToString("yyyy-MM-dd");
        //query["endTime"] = to.ToString("yyyy-MM-dd");

        const string fmt = "yyyy-MM-dd'+09:00'";          // ISO 날짜+타임존
        query["createdAtFrom"] = from.ToString(fmt);      // 2025-08-05+09:00
        query["createdAtTo"] = to.ToString(fmt);      // URL 빌더가 자동 인코딩 (%2B09:00)

        if (!string.IsNullOrEmpty(status))
            query["status"] = status;
        query["maxPerPage"] = max.ToString();

        var uri = new UriBuilder($"{Host}{path}") { Query = query.ToString() }.Uri;
        var req = new HttpRequestMessage(HttpMethod.Get, uri);
        req.Headers.Add("Authorization", BuildHmac("GET", path, query.ToString()));

        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            _logger.LogError("Coupang {Status}: {Body}", res.StatusCode, body);
            // 예) {"code":"ERROR_SIGNATURE","message":"signature mismatch"}
        }
        else
        {
            //var raw = await res.Content.ReadAsStringAsync();
            //_logger.LogInformation("Raw JSON = {0}", raw);  // ⚠️ 테스트 후 삭제
        }

        //res.EnsureSuccessStatusCode();

        //await using var stream = await res.Content.ReadAsStreamAsync();
        //return await JsonSerializer.DeserializeAsync<OrderSheetResponse>(stream);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)  // .NET 6+
        {
            PropertyNameCaseInsensitive = true
        };

        //await using var stream = await res.Content.ReadAsStreamAsync(); - 테스트를 위해 주석
        //var result = await JsonSerializer.DeserializeAsync<OrderSheetResponse>(stream, options);

        string wrappedJson = "{\r\n  \"code\": \"200\",\r\n  \"message\": null,\r\n  \"data\": [\r\n    {\r\n      \"shipmentBoxId\": 956418605652639700,\r\n      \"orderId\": 29100131441492,\r\n      \"status\": \"ACCEPT\",\r\n      \"orderedAt\": \"2025-08-06T12:17:47+09:00\",\r\n      \"shippingPrice\": {\r\n        \"currencyCode\": \"KRW\",\r\n        \"units\": 0,\r\n        \"nanos\": 0\r\n      },\r\n      \"remotePrice\": {\r\n        \"currencyCode\": \"KRW\",\r\n        \"units\": 0,\r\n        \"nanos\": 0\r\n      },\r\n      \"orderItems\": [\r\n        {\r\n          \"vendorItemId\": 92979706172,\r\n          \"vendorItemName\": \"오피뉴 수박칼 12조각 간편한 수박자르는칼, 1개\",\r\n          \"productId\": 8210095526,\r\n          \"shippingCount\": 1,\r\n          \"salesPrice\": {\r\n            \"currencyCode\": \"KRW\",\r\n            \"units\": 11000,\r\n            \"nanos\": 0\r\n          },\r\n          \"orderPrice\": {\r\n            \"currencyCode\": \"KRW\",\r\n            \"units\": 11000,\r\n            \"nanos\": 0\r\n          },\r\n          \"discountPrice\": {\r\n            \"currencyCode\": \"KRW\",\r\n            \"units\": 1400,\r\n            \"nanos\": 0\r\n          },\r\n          \"instantCouponDiscount\": {\r\n            \"currencyCode\": \"KRW\",\r\n            \"units\": 1400,\r\n            \"nanos\": 0\r\n          },\r\n          \"downloadableCouponDiscount\": {\r\n            \"currencyCode\": \"KRW\",\r\n            \"units\": 0,\r\n            \"nanos\": 0\r\n          },\r\n          \"coupangDiscount\": {\r\n            \"currencyCode\": \"KRW\",\r\n            \"units\": 0,\r\n            \"nanos\": 0\r\n          },\r\n          \"cancelCount\": 0,\r\n          \"holdCountForCancel\": 0,\r\n          \"estimatedShippingDate\": \"2025-08-07+09:00\",\r\n          \"plannedShippingDate\": null,\r\n          \"externalVendorSkuCode\": \"AZ01660695\",\r\n          \"sellerProductId\": 15640209635,\r\n          \"sellerProductName\": \"수박커팅 12조각 수박자르는칼, 1P\",\r\n          \"sellerProductItemName\": \"1개\",\r\n          \"canceled\": false\r\n        }\r\n      ]\r\n    }\r\n  ],\r\n  \"nextToken\": null\r\n}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(wrappedJson));

        var result = await JsonSerializer.DeserializeAsync<OrderSheetResponse>(stream, options);

        return result;
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