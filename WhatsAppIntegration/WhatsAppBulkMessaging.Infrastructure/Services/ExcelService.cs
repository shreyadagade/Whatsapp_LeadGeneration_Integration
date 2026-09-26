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

using NPOI.SS.UserModel;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public async Task<List<RecipientDto>> ReadRecipientsAsync(
        Stream fileStream,
        string fileName)
    {
        var extension = Path.GetExtension(fileName)
            .ToLowerInvariant();

        return extension switch
        {
            ".xlsx" or ".xls" =>
                await ReadExcelFileAsync(fileStream),

            ".csv" =>
                await ReadDelimitedFileAsync(fileStream, ','),

            ".tsv" =>
                await ReadDelimitedFileAsync(fileStream, '\t'),

            _ =>
                throw new InvalidOperationException(
                    "Unsupported file format.")
        };
    }

    private Task<List<RecipientDto>> ReadExcelFileAsync(
        Stream fileStream)
    {
        var recipients = new List<RecipientDto>();

        using var workbook = WorkbookFactory.Create(fileStream);

        var sheet = workbook.GetSheetAt(0);

        if (sheet == null)
        {
            return Task.FromResult(recipients);
        }

        var headerRow = sheet.GetRow(0);

        if (headerRow == null)
        {
            return Task.FromResult(recipients);
        }

        var headerIndexes = GetHeaderIndexes(headerRow);

        if (!headerIndexes.ContainsKey("phonenumber"))
        {
            throw new InvalidOperationException(
                "PhoneNumber column is required.");
        }

        var phoneNumberIndex = headerIndexes["phonenumber"];

        headerIndexes.TryGetValue(
            "candidatename",
            out var candidateNameIndex);

        headerIndexes.TryGetValue(
            "serialnumber",
            out var serialNumberIndex);

        for (var rowIndex = 1;
             rowIndex <= sheet.LastRowNum;
             rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);

            if (row == null)
            {
                continue;
            }

            var phoneNumber =
                GetCellValue(row.GetCell(phoneNumberIndex));

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                continue;
            }

            var candidateName =
                candidateNameIndex >= 0
                    ? GetCellValue(row.GetCell(candidateNameIndex))
                    : string.Empty;

            var serialNumber =
                serialNumberIndex >= 0
                    ? GetCellValue(row.GetCell(serialNumberIndex))
                    : string.Empty;

            recipients.Add(new RecipientDto
            {
                RowNumber = rowIndex + 1,
                SerialNumber = serialNumber,
                CandidateName = candidateName,
                PhoneNumber = phoneNumber
            });
        }

        return Task.FromResult(recipients);
    }

    private async Task<List<RecipientDto>> ReadDelimitedFileAsync(
        Stream fileStream,
        char separator)
    {
        var recipients = new List<RecipientDto>();

        using var reader = new StreamReader(
            fileStream,
            leaveOpen: true);

        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return recipients;
        }

        var headers = ParseDelimitedLine(
            headerLine,
            separator);

        var headerIndexes = GetHeaderIndexes(headers);

        if (!headerIndexes.ContainsKey("phonenumber"))
        {
            throw new InvalidOperationException(
                "PhoneNumber column is required.");
        }

        var phoneNumberIndex = headerIndexes["phonenumber"];

        headerIndexes.TryGetValue(
            "candidatename",
            out var candidateNameIndex);

        headerIndexes.TryGetValue(
            "serialnumber",
            out var serialNumberIndex);

        var rowNumber = 1;

        while (await reader.ReadLineAsync() is { } line)
        {
            rowNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = ParseDelimitedLine(
                line,
                separator);

            if (phoneNumberIndex >= columns.Count)
            {
                continue;
            }

            var phoneNumber =
                columns[phoneNumberIndex].Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                continue;
            }

            var candidateName =
                candidateNameIndex >= 0 &&
                candidateNameIndex < columns.Count
                    ? columns[candidateNameIndex].Trim()
                    : string.Empty;

            var serialNumber =
                serialNumberIndex >= 0 &&
                serialNumberIndex < columns.Count
                    ? columns[serialNumberIndex].Trim()
                    : string.Empty;

            recipients.Add(new RecipientDto
            {
                RowNumber = rowNumber,
                SerialNumber = serialNumber,
                CandidateName = candidateName,
                PhoneNumber = phoneNumber
            });
        }

        return recipients;
    }

    private static Dictionary<string, int> GetHeaderIndexes(
        IRow headerRow)
    {
        var headers = new List<string>();

        for (var i = 0; i < headerRow.LastCellNum; i++)
        {
            headers.Add(
                GetCellValue(headerRow.GetCell(i)));
        }

        return GetHeaderIndexes(headers);
    }

    private static Dictionary<string, int> GetHeaderIndexes(
        List<string> headers)
    {
        var result = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < headers.Count; i++)
        {
            var normalizedHeader =
                NormalizeHeader(headers[i]);

            if (!string.IsNullOrWhiteSpace(normalizedHeader) &&
                !result.ContainsKey(normalizedHeader))
            {
                result[normalizedHeader] = i;
            }
        }

        return result;
    }

    private static string NormalizeHeader(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        return new string(
            header
                .Trim()
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
    }

    private static string GetCellValue(ICell? cell)
    {
        if (cell == null)
        {
            return string.Empty;
        }

        return cell.CellType switch
        {
            CellType.Numeric =>
                cell.NumericCellValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            CellType.Boolean =>
                cell.BooleanCellValue.ToString(),

            CellType.Formula =>
                cell.ToString()?.Trim() ?? string.Empty,

            _ =>
                cell.ToString()?.Trim() ?? string.Empty
        };
    }

    private static List<string> ParseDelimitedLine(string line,char separator)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }

                continue;
            }

            if (character == separator && !insideQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString());

        return values;
    }
}