namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IPhoneNumberService
{
    string? Normalize(string phoneNumber);
}