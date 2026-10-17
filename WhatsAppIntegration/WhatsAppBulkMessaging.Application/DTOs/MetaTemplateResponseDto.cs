
using System.Text.Json.Serialization;

namespace WhatsAppBulkMessaging.Application.DTOs;

public class MetaTemplateResponseDto
{
    [JsonPropertyName("data")]
    public List<MetaTemplateDto> Data { get; set; } = [];
}

public class MetaTemplateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    public int BodyParameterCount { get; set; }

    [JsonPropertyName("components")]
    public List<MetaTemplateComponentDto> Components { get; set; } = [];
}

public class MetaTemplateComponentDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("example")]
    public MetaTemplateExampleDto? Example { get; set; }
}

public class MetaTemplateExampleDto
{
    [JsonPropertyName("body_text")]
    public List<List<string>> Body_Text { get; set; } = [];

    [JsonPropertyName("header_handle")]
    public List<string> Header_Handle { get; set; } = [];

    [JsonPropertyName("header_text")]
    public List<string> Header_Text { get; set; } = [];
}
