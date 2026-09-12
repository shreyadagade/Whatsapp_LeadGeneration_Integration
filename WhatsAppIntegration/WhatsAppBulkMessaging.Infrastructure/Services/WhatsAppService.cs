using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Infrastructure.Configuration;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;

    public WhatsAppService(HttpClient httpClient,WhatsAppSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<string> SendTemplateMessageAsync(
        string phoneNumber,
        string templateName)
    {
        var url =
            $"{_settings.BaseUrl}/{_settings.PhoneNumberId}/messages";

        var requestBody = new
        {
            messaging_product = "whatsapp",
            to = phoneNumber,
            type = "template",
            template = new
            {
                name = templateName,
                language = new
                {
                    code = _settings.LanguageCode
                },
                components = new[]
                {
                    new
                    {
                        type = "header",
                        parameters = new[]
                        {
                            new
                            {
                                type = "image",
                                image = new
                                {
                                    id = _settings.HeaderImageMediaId
                                }   
                             }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _settings.AccessToken);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"WhatsApp API request failed. Status: {(int)response.StatusCode}, Response: {responseContent}");
        }

        using var document =
            JsonDocument.Parse(responseContent);

        if (document.RootElement.TryGetProperty(
                "messages",
                out var messages) &&
            messages.GetArrayLength() > 0)
        {
            return messages[0]
                .GetProperty("id")
                .GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    public async Task<string> UploadMediaAsync(Stream fileStream,string fileName)
    {
        var url =
            $"{_settings.BaseUrl}/{_settings.PhoneNumberId}/media";

        using var content = new MultipartFormDataContent();

        content.Add(
            new StringContent("whatsapp"),
            "messaging_product");

        var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                Path.GetExtension(fileName)
                    .Equals(".png", StringComparison.OrdinalIgnoreCase)
                    ? "image/png"
                    : "image/jpeg");

        content.Add(
            fileContent,
            "file",
            fileName);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _settings.AccessToken);

        request.Content = content;

        var response =
            await _httpClient.SendAsync(request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"WhatsApp media upload failed. Status: {(int)response.StatusCode}, Response: {responseContent}");
        }

        using var document =
            JsonDocument.Parse(responseContent);

        if (document.RootElement.TryGetProperty(
                "id",
                out var id))
        {
            return id.GetString() ?? string.Empty;
        }

        throw new Exception(
            "WhatsApp media upload succeeded but media ID was not returned.");
    }
}