namespace OrderBot.Models;

public class OrderSheetResponse
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public List<OrderSheet> Data { get; set; } = new();
}

public class OrderSheet
{
    public long ShipmentBoxId { get; set; }
    public long OrderId { get; set; }
    public string? Status { get; set; }
    public DateTime OrderedAt { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}

public class OrderItem
{
    public string? VendorItemName { get; set; }
    public int ShippingCount { get; set; }
}