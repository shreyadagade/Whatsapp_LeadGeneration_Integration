//using NPOI.SS.UserModel;
//using WhatsAppBulkMessaging.Application.DTOs;
//using WhatsAppBulkMessaging.Application.Interfaces;

//namespace WhatsAppBulkMessaging.Infrastructure.Services;

//public class ExcelService : IExcelService
//{
//    public async Task<List<RecipientDto>> ReadRecipientsAsync(Stream fileStream,
//        string fileName)
//    {
//        var extension = Path.GetExtension(fileName).ToLowerInvariant();

//        return extension switch
//        {
//            ".xlsx" or ".xls" => await ReadExcelFileAsync(fileStream),

//            ".csv" => await ReadDelimitedFileAsync(fileStream, ','),

//            ".tsv" => await ReadDelimitedFileAsync(fileStream, '\t'),

//            _ =>
//                throw new InvalidOperationException(
//                    "Unsupported file format.")
//        };
//    }

//    private Task<List<RecipientDto>> ReadExcelFileAsync(Stream fileStream)
//    {
//        var recipients = new List<RecipientDto>();

//        var workbook = WorkbookFactory.Create(fileStream);

//        try
//        {
//            var sheet = workbook.GetSheetAt(0);

//            if (sheet == null)
//                return Task.FromResult(recipients);

//            // First row = Header
//            for (int rowIndex = 1;
//                 rowIndex <= sheet.LastRowNum;
//                 rowIndex++)
//            {
//                var row = sheet.GetRow(rowIndex);

//                if (row == null)
//                    continue;

//                // Column 1 = Serial Number → Ignore
//                // Column 2 = Candidate Name
//                // Column 3 = Phone Number

//                var candidateName = row.GetCell(1)?.ToString()?.Trim() ?? string.Empty;

//                var phoneNumber = row.GetCell(2)?.ToString()?.Trim() ?? string.Empty;

//                // CandidateName is optional.
//                // PhoneNumber is required.
//                if (string.IsNullOrWhiteSpace(phoneNumber))
//                    continue;

//                recipients.Add(new RecipientDto
//                {
//                    CandidateName = candidateName,
//                    PhoneNumber = phoneNumber
//                });
//            }
//        }
//        finally
//        {
//            workbook.Close();
//        }

//        return Task.FromResult(recipients);
//    }

//    private async Task<List<RecipientDto>> ReadDelimitedFileAsync(Stream fileStream,
//        char separator)
//    {
//        var recipients = new List<RecipientDto>();

//        using var reader = new StreamReader(fileStream,leaveOpen: true);

//        // First row = Header
//        await reader.ReadLineAsync();

//        while (await reader.ReadLineAsync() is { } line)
//        {
//            if (string.IsNullOrWhiteSpace(line))
//                continue;

//            var columns = line.Split(separator);

//            // We need at least Column 3 = PhoneNumber
//            if (columns.Length < 3)
//                continue;

//            // Column 1 = Serial Number → Ignore
//            // Column 2 = Candidate Name
//            // Column 3 = Phone Number

//            var candidateName = columns[1].Trim();

//            var phoneNumber = columns[2].Trim();

//            // CandidateName is optional.
//            // PhoneNumber is required.
//            if (string.IsNullOrWhiteSpace(phoneNumber))
//                continue;

//            recipients.Add(new RecipientDto
//            {
//                CandidateName = candidateName,
//                PhoneNumber = phoneNumber
//            });
//        }

//        return recipients;
//    }
//}