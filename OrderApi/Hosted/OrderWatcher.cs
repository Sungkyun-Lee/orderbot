namespace OrderBot.Hosted;

using OrderBot.Services;

public class OrderWatcher : BackgroundService
{
    private readonly CoupangOrderClient _coupang;
    private readonly KakaoTalkNotifier _kakao;
    private DateTime _lastChecked = DateTime.UtcNow.AddMinutes(-30);

    public OrderWatcher(CoupangOrderClient coupang, KakaoTalkNotifier kakao)
    {
        _coupang = coupang;
        _kakao = kakao;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var orders = await _coupang.GetOrdersAsync(_lastChecked, now, "ACCEPT");
            _lastChecked = now;

            if (orders?.Data?.Any() == true)
            {
                var first = orders.Data.First();
                string msg = $"새 주문 {orders.Data.Count}건! 첫 상품: {first.OrderItems.First().VendorItemName}";
                await _kakao.SendTextAsync(msg);
            }

            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        }
    }
}