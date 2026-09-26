namespace WhatsAppBulkMessaging.Application.DTOs;

public class MetaTemplateResponseDto
{
    public List<MetaTemplateDto> Data { get; set; } = [];
}

public class MetaTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public List<MetaTemplateComponentDto> Components { get; set; } = [];
}

public class MetaTemplateComponentDto
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public MetaTemplateExampleDto? Example { get; set; }
}

public class MetaTemplateExampleDto
{
    public List<List<string>> Body_Text { get; set; } = [];
}