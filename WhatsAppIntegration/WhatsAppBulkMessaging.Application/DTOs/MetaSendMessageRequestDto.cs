using System.Text.Json.Serialization;

namespace WhatsAppBulkMessaging.Application.DTOs;

public class MetaSendMessageRequestDto
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = "whatsapp";

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "template";

    [JsonPropertyName("template")]
    public MetaSendTemplateDto Template { get; set; } = new();
}

public class MetaSendTemplateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public MetaSendTemplateLanguageDto Language { get; set; } = new();

    [JsonPropertyName("components")]
    public List<MetaSendTemplateComponentDto> Components { get; set; } = [];
}

public class MetaSendTemplateLanguageDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public class MetaSendTemplateComponentDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public List<MetaSendTemplateParameterDto> Parameters { get; set; } = [];
}

public class MetaSendTemplateParameterDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("image")]
    public MetaSendTemplateImageDto? Image { get; set; }
}

public class MetaSendTemplateImageDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}