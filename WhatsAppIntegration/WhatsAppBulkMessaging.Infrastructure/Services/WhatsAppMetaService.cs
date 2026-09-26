using Microsoft.Extensions.Options;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class WhatsAppMetaService : IWhatsAppMetaService, IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;

    public WhatsAppMetaService(
        HttpClient httpClient,
        IOptions<WhatsAppOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<MetaTemplateResponseDto> GetTemplatesAsync()
    {
        var url =
            $"{_options.BaseUrl}/{_options.ApiVersion}/{_options.BusinessAccountId}/message_templates";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                _options.AccessToken);

        using var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var result = System.Text.Json.JsonSerializer.Deserialize<MetaTemplateResponseDto>(
            content,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result ?? new MetaTemplateResponseDto();
    }

    public async Task<string> SendTemplateMessageAsync(MetaSendMessageRequestDto request)
    {
        var url =
            $"{_options.BaseUrl}/{_options.ApiVersion}/{_options.PhoneNumberId}/messages";

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                _options.AccessToken);

        var json = System.Text.Json.JsonSerializer.Serialize(request);

        httpRequest.Content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(httpRequest);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Meta API error ({(int)response.StatusCode}): {content}");
        }

        using var document = System.Text.Json.JsonDocument.Parse(content);

        if (document.RootElement.TryGetProperty("messages", out var messages) &&
            messages.GetArrayLength() > 0)
        {
            return messages[0]
                .GetProperty("id")
                .GetString() ?? string.Empty;
        }

        return string.Empty;
    }

}