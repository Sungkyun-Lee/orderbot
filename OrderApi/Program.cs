//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

var app = WebApplication.Create();
app.MapGet("/", () => "✅ Render test ok");

// Render가 넣어주는 PORT 변수를 우선 사용, 없으면 5100
var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
app.Run($"http://0.0.0.0:{port}");
