
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
            $"{_options.BaseUrl.TrimEnd('/')}/{_options.ApiVersion}/" +
            $"{_options.BusinessAccountId}/message_templates";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<MetaTemplateResponseDto>(
            content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new MetaTemplateResponseDto();
    }

    public async Task<string> UploadMediaAsync(
        Stream fileStream,
        string fileName,
        string contentType)
    {
        var url =
            $"{_options.BaseUrl.TrimEnd('/')}/{_options.ApiVersion}/" +
            $"{_options.PhoneNumberId}/media";

        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(contentType);

        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent("whatsapp"), "messaging_product");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        request.Content = form;

        using var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Meta media upload error ({(int)response.StatusCode}): {content}");
        }

        using var document = JsonDocument.Parse(content);

        if (document.RootElement.TryGetProperty("id", out var id))
        {
            return id.GetString() ?? string.Empty;
        }

        throw new InvalidOperationException(
            "Meta media upload succeeded but media ID was not returned.");
    }

    public async Task<string> SendTemplateMessageAsync(
        MetaSendMessageRequestDto request)
    {
        var url =
            $"{_options.BaseUrl.TrimEnd('/')}/{_options.ApiVersion}/" +
            $"{_options.PhoneNumberId}/messages";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(httpRequest);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Meta API error ({(int)response.StatusCode}): {content}");
        }

        using var document = JsonDocument.Parse(content);

        if (document.RootElement.TryGetProperty("messages", out var messages) &&
            messages.GetArrayLength() > 0)
        {
            return messages[0].GetProperty("id").GetString() ?? string.Empty;
        }

        throw new InvalidOperationException(
            "Meta accepted the request but returned no message ID.");
    }

    public async Task<SendReplyResponseDto> SendReplyAsync(
        SendReplyRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Phone number is required.");

        if (string.IsNullOrWhiteSpace(request.MessageType))
            throw new ArgumentException("Message type is required.");

        var phoneNumber = new string(
            request.PhoneNumber.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("A valid phone number is required.");

        var messageType = request.MessageType.Trim().ToLowerInvariant();

        var result = new SendReplyResponseDto
        {
            PhoneNumber = phoneNumber,
            MessageType = messageType,
            Message = "Reply sent successfully."
        };

        // Text-only message
        if (messageType == "text")
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                throw new ArgumentException("Text message is required.");

            result.TextMessageId = await SendTextMessageAsync(
                phoneNumber,
                request.Text);

            return result;
        }

        if (messageType is not ("image" or "video" or "audio" or "document"))
        {
            throw new ArgumentException(
                "Supported types: text, image, video, audio, document.");
        }

        if (request.FileStream is null ||
            string.IsNullOrWhiteSpace(request.FileName) ||
            string.IsNullOrWhiteSpace(request.ContentType))
        {
            throw new ArgumentException(
                "File, filename and content type are required for media messages.");
        }

        if (!request.FileStream.CanRead)
            throw new ArgumentException("The uploaded file is not readable.");

        ValidateMediaContentType(messageType, request.ContentType);

        // Upload the media first.
        var mediaId = await UploadMediaAsync(
            request.FileStream,
            request.FileName,
            request.ContentType);

        // Send optional text as a separate WhatsApp message.
        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            result.TextMessageId = await SendTextMessageAsync(
                phoneNumber,
                request.Text);
        }

        var mediaPayload = BuildMediaPayload(
            phoneNumber,
            messageType,
            mediaId,
            request.FileName,
            request.Caption);

        result.MediaMessageId = await SendJsonMessageAsync(mediaPayload);

        return result;
    }

    private static void ValidateMediaContentType(
        string messageType,
        string contentType)
    {
        var type = contentType.Trim().ToLowerInvariant();

        var supported = messageType switch
        {
            "image" => new[] { "image/jpeg", "image/png" },

            "video" => new[] { "video/mp4", "video/3gpp" },

            "audio" => new[]
            {
                "audio/aac",
                "audio/mp4",
                "audio/mpeg",
                "audio/amr",
                "audio/ogg",
                "audio/opus"
            },

            "document" => new[]
            {
                "application/pdf",
                "text/plain",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            },

            _ => Array.Empty<string>()
        };

        if (!supported.Contains(type, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Unsupported content type '{contentType}' for {messageType}. " +
                "Convert the file to a supported format before uploading.");
        }
    }

    private static Dictionary<string, object> BuildMediaPayload(
        string phoneNumber,
        string messageType,
        string mediaId,
        string fileName,
        string? caption)
    {
        var media = new Dictionary<string, object>
        {
            ["id"] = mediaId
        };

        if (messageType is "image" or "video" or "document")
        {
            if (!string.IsNullOrWhiteSpace(caption))
                media["caption"] = caption;
        }

        if (messageType == "document")
            media["filename"] = fileName;

        return new Dictionary<string, object>
        {
            ["messaging_product"] = "whatsapp",
            ["recipient_type"] = "individual",
            ["to"] = phoneNumber,
            ["type"] = messageType,
            [messageType] = media
        };
    }

    private async Task<string> SendTextMessageAsync(
        string phoneNumber,
        string text)
    {
        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = phoneNumber,
            type = "text",
            text = new
            {
                preview_url = false,
                body = text
            }
        };

        return await SendJsonMessageAsync(payload);
    }

    private async Task<string> SendJsonMessageAsync(object payload)
    {
        var url =
            $"{_options.BaseUrl.TrimEnd('/')}/{_options.ApiVersion}/" +
            $"{_options.PhoneNumberId}/messages";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"WhatsApp message failed ({(int)response.StatusCode}): {content}");
        }

        using var document = JsonDocument.Parse(content);

        if (document.RootElement.TryGetProperty("messages", out var messages) &&
            messages.GetArrayLength() > 0 &&
            messages[0].TryGetProperty("id", out var messageId))
        {
            return messageId.GetString() ?? string.Empty;
        }

        throw new InvalidOperationException(
            "Meta accepted the request but returned no message ID.");
    }
}
