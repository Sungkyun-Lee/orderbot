using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Coupang ������ "code" �ʵ尡
/// �� ����(200) �Ǵ� ���ڿ�("ERROR_SIGNATURE")�� �� �� �����Ƿ�  
/// �׻� string ���� �о� ���̱� ���� ������
/// </summary>
public sealed class CodeStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader,
                                Type typeToConvert,
                                JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString()!,      // "ERROR_SIGNATURE"
            JsonTokenType.Number => reader.GetInt32().ToString(), // 200 �� "200"
            _ => throw new JsonException($"Unexpected token {reader.TokenType} for code")
        };
    }

    public override void Write(Utf8JsonWriter writer,
                               string value,
                               JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}


public sealed class DateWithOffsetConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader,
                                         Type typeToConvert,
                                         JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            return string.IsNullOrWhiteSpace(s)
                ? null
                : DateTimeOffset.Parse(s);          // "2025-08-07+09:00"
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer,
                               DateTimeOffset? value,
                               JsonSerializerOptions options)
    {
        if (value is { } dto)
            writer.WriteStringValue(dto.ToString("yyyy-MM-ddzzz")); // 2025-08-07+09:00
        else
            writer.WriteNullValue();
    }
}