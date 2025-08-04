//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

var app = WebApplication.Create();
app.MapGet("/", () => "✅ Render test ok");
app.MapGet("/test/ckey", () =>
{
    var key = File.ReadAllText("/etc/secrets/COUPANG_ACCESS_KEY").Trim();
    return $"AK prefix: {key[..4]}*** ({key.Length} chars)";
});
var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";  // 기본 8080
app.Run($"http://0.0.0.0:{port}");
