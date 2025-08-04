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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<CoupangOrderClient>();
//builder.Services.AddSingleton<KakaoTalkNotifier>();
//builder.Services.AddHostedService<OrderWatcher>();

var app = builder.Build();
app.MapGet("/health", () => "OK");

// 최근 3시간 내 최신 주문 1건 조회
app.MapGet("/orders/latest", async (CoupangOrderClient coupang) =>
{
    var now = DateTime.UtcNow;
    var res = await coupang.GetOrdersAsync(now.AddHours(-3), now, "ACCEPT");
    return res?.Data?.FirstOrDefault() is { } first
        ? Results.Ok(first)
        : Results.NotFound("최근 3시간 내 주문 없음");
});

app.Run();
