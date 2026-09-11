namespace WhatsAppBulkMessaging.Application.Validators;

public static class PhoneNumberValidator
{
    public static bool IsValidIndianMobileNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var cleanNumber = phoneNumber
            .Replace("+", "")
            .Replace("-", "")
            .Replace(" ", "")
            .Trim();

        if (cleanNumber.Length != 12)
        {
            return false;
        }

        if (!cleanNumber.StartsWith("91"))
        {
            return false;
        }

        var mobileNumber = cleanNumber.Substring(2);

        if (!mobileNumber.All(char.IsDigit))
        {
            return false;
        }

        return mobileNumber.StartsWith("6") ||
               mobileNumber.StartsWith("7") ||
               mobileNumber.StartsWith("8") ||
               mobileNumber.StartsWith("9");
    }
}