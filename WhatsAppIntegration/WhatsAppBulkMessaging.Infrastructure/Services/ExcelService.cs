using ClosedXML.Excel;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public Task<List<RecipientDto>> ReadRecipientsAsync(Stream fileStream)
    {
        var recipients = new List<RecipientDto>();

        using var workbook = new XLWorkbook(fileStream);

        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            return Task.FromResult(recipients);
        }

        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var phoneNumber = row.Cell(1).GetString().Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                continue;
            }

            recipients.Add(new RecipientDto
            {
                PhoneNumber = phoneNumber
            });
        }

        return Task.FromResult(recipients);
    }
}