using System.Text.Json.Serialization;

namespace OrderBot.Models;

public class OrderSheetResponse
{
    [JsonPropertyName("code")]
    [JsonConverter(typeof(CodeStringConverter))]
    public string Code { get; set; } = "";

    [JsonPropertyName("message")]
    public string? Message
    {
        get; set;
    }

    [JsonPropertyName("data")]
    public List<OrderSheet> Data { get; set; } = new();

    [JsonPropertyName("nextToken")]
    public string? NextToken
    {
        get; set;
    }
}

public class OrderSheet
{
    [JsonPropertyName("shipmentBoxId")]
    public long ShipmentBoxId
    {
        get; set;
    }

    [JsonPropertyName("orderId")]
    public long OrderId
    {
        get; set;
    }

    [JsonPropertyName("status")]
    public string? Status
    {
        get; set;
    }

    [JsonPropertyName("orderedAt")]
    public DateTimeOffset OrderedAt
    {
        get; set;
    }

    // ✨ 새로 추가되는 부분
    [JsonPropertyName("receiver")]
    public Receiver? Receiver
    {
        get; set;
    }

    // 배송비·도서산간비
    [JsonPropertyName("shippingPrice")]
    public Money? ShippingPrice
    {
        get; set;
    }

    [JsonPropertyName("remotePrice")]
    public Money? RemotePrice
    {
        get; set;
    }

    // 주문 상품들
    [JsonPropertyName("orderItems")]
    public List<OrderItem> OrderItems { get; set; } = new();
}

public class OrderItem
{
    [JsonPropertyName("vendorItemId")]
    public long VendorItemId
    {
        get; set;
    }

    [JsonPropertyName("vendorItemName")]
    public string? VendorItemName
    {
        get; set;
    }

    [JsonPropertyName("productId")]
    public long ProductId
    {
        get; set;
    }          // 0이면 없는 값

    [JsonPropertyName("shippingCount")]
    public int ShippingCount
    {
        get; set;
    }

    // 가격 관련
    [JsonPropertyName("salesPrice")]
    public Money? SalesPrice
    {
        get; set;
    }

    [JsonPropertyName("orderPrice")]
    public Money? OrderPrice
    {
        get; set;
    }

    [JsonPropertyName("discountPrice")]
    public Money? DiscountPrice
    {
        get; set;
    }

    // 할인 세부 항목
    [JsonPropertyName("instantCouponDiscount")]
    public Money? InstantCouponDiscount
    {
        get; set;
    }

    [JsonPropertyName("downloadableCouponDiscount")]
    public Money? DownloadableCouponDiscount
    {
        get; set;
    }

    [JsonPropertyName("coupangDiscount")]
    public Money? CoupangDiscount
    {
        get; set;
    }

    // 취소·보류 수량
    [JsonPropertyName("cancelCount")]
    public int CancelCount
    {
        get; set;
    }

    [JsonPropertyName("holdCountForCancel")]
    public int HoldCountForCancel
    {
        get; set;
    }

    // 출고 예정일
    [JsonPropertyName("estimatedShippingDate")]
    [JsonConverter(typeof(DateWithOffsetConverter))]
    public DateTimeOffset? EstimatedShippingDate
    {
        get; set;
    }   // yyyy-MM-dd

    [JsonPropertyName("plannedShippingDate")]
    [JsonConverter(typeof(DateWithOffsetConverter))]
    public DateTimeOffset? PlannedShippingDate
    {
        get; set;
    }     // yyyy-MM-dd

    // 기타
    [JsonPropertyName("externalVendorSkuCode")]
    public string? ExternalVendorSkuCode
    {
        get; set;
    }

    [JsonPropertyName("sellerProductId")]
    public long SellerProductId
    {
        get; set;
    }

    [JsonPropertyName("sellerProductName")]
    public string? SellerProductName
    {
        get; set;
    }

    [JsonPropertyName("sellerProductItemName")]
    public string? SellerProductItemName
    {
        get; set;
    }

    [JsonPropertyName("canceled")]
    public bool Canceled
    {
        get; set;
    }
}

public class Money
{
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = "KRW";

    [JsonPropertyName("units")]
    public long Units
    {
        get; set;
    }

    [JsonPropertyName("nanos")]
    public int Nanos
    {
        get; set;
    }
}

public class Receiver
{
    [JsonPropertyName("name")]          // 수취인 이름
    public string? Name
    {
        get; set;
    }

    [JsonPropertyName("safeNumber")]    // 안심번호(050)
    public string? SafeNumber
    {
        get; set;
    }

    [JsonPropertyName("receiverNumber")]// 실전화번호 (nullable)
    public string? ReceiverNumber
    {
        get; set;
    }

    [JsonPropertyName("addr1")]         // 기본 주소
    public string? Addr1
    {
        get; set;
    }

    [JsonPropertyName("addr2")]         // 상세 주소
    public string? Addr2
    {
        get; set;
    }

    [JsonPropertyName("postCode")]      // 우편번호
    public string? PostCode
    {
        get; set;
    }
}