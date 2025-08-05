//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

//var app = WebApplication.Create();
//app.MapGet("/", () => "✅ Render test ok");
//app.MapGet("/orders/latest", async (CoupangOrderClient coupang) =>
//{
//    var now = DateTime.UtcNow;
//    var res = await coupang.GetOrdersAsync(now.AddHours(-3), now, "ACCEPT");
//    return res?.Data?.FirstOrDefault() is { } first
//        ? Results.Ok(first)
//        : Results.NotFound("최근 3시간 내 주문 없음");
//});
//// 쿠팡 Access키 확인
////app.MapGet("/test/ckey", () =>
////{
////    var key = File.ReadAllText("/etc/secrets/COUPANG_ACCESS_KEY").Trim();
////    return $"AK prefix: {key[..4]}*** ({key.Length} chars)";
////});
//var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";  // 기본 8080
//app.Run($"http://0.0.0.0:{port}");


using OrderBot.Services;
using OrderBot.Hosted;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// 1) 반드시 Microsoft.Extensions.Http 패키지가 포함돼 있어야 합니다.
builder.Services
       .AddHttpClient("default")                  // ← 이름 지정 (또는 .AddHttpClient<CoupangOrderClient>())
       .ConfigurePrimaryHttpMessageHandler(() =>
       {
           var handler = new HttpClientHandler
           {
               AutomaticDecompression =
                   DecompressionMethods.GZip | DecompressionMethods.Deflate
           };
           return handler;                        // HttpMessageHandler 반환
       });

// 2) 나머지 DI
builder.Services.AddSingleton<CoupangOrderClient>();
//builder.Services.AddSingleton<KakaoTalkNotifier>();
//builder.Services.AddHostedService<OrderWatcher>();

var app = builder.Build();
app.MapGet("/health", () => "OK");
app.MapGet("/orders/latest", async (
    CoupangOrderClient coupang, ILogger<Program> log) =>
{
    try
    {
        var now = DateTime.UtcNow;
        var res = await coupang.GetOrdersAsync(now.AddHours(-3), now, "ACCEPT");
        var first = res?.Data?.FirstOrDefault();

        return first is null
            ? Results.NotFound("최근 3시간 내 주문 없음")
            : Results.Ok(first);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        log.LogWarning("Coupang 403 – 인증 실패: {msg}", ex.Message);
        return Results.StatusCode(502);   // Bad Gateway
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        log.LogWarning("Coupang 404 – 경로/파라미터 재확인 필요");
        return Results.StatusCode(502);
    }
});

app.Run();
