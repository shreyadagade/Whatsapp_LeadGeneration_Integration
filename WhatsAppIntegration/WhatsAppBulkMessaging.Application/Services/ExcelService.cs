using ClosedXML.Excel;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public async Task<List<RecipientDto>> ReadRecipientsAsync(
        Stream fileStream)
    {
        var recipients = new List<RecipientDto>();

        using var workbook = new XLWorkbook(fileStream);

        var worksheet = workbook.Worksheet(1);

        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var name = row.Cell(1).GetString().Trim();
            var phoneNumber = row.Cell(2).GetString().Trim();

            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(phoneNumber))
            {
                continue;
            }

            recipients.Add(new RecipientDto
            {
                Name = name,
                PhoneNumber = phoneNumber
            });
        }

        return await Task.FromResult(recipients);
    }
}