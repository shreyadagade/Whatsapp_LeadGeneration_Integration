using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class PhoneNumberService : IPhoneNumberService
{
    public string? Normalize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var value = phoneNumber.Trim();

        // Remove common formatting characters
        value = value
            .Replace("+", "")
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        // India: 10-digit mobile number
        if (value.Length == 10 &&
            value.All(char.IsDigit))
        {
            return $"91{value}";
        }

        // Already in international format
        if (value.Length >= 11 &&
            value.Length <= 15 &&
            value.All(char.IsDigit))
        {
            return value;
        }

        return null;
    }
}