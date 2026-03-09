using System.Text;
using ExcelDataReader;

public class ExcelReaderService
{
    public async Task ReadExcelAsync<T>(
        IFormFile file,
        int batchSize,
        Func<List<T>, int, Task> batchHandler,
        Action<ImportError> errorHandler,
        Action<ImportProgress> progressHandler,
        CancellationToken cancellationToken
    ) where T : new()
    {
        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);

        using var stream = file.OpenReadStream();
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var properties = typeof(T).GetProperties();
        var buffer = new List<T>(batchSize);

        int rowNumber = 0;
        bool isHeader = true;

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            rowNumber++;

            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            try
            {
                var obj = new T();

                for (int i = 0; i < properties.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var value = reader.GetValue(i);

                    if (value == null) continue;

                    var property = properties[i];

                    var converted = Convert.ChangeType(
                        value,
                        Nullable.GetUnderlyingType(property.PropertyType)
                        ?? property.PropertyType
                    );

                    property.SetValue(obj, converted);
                }

                buffer.Add(obj);

                if (buffer.Count >= batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await batchHandler(buffer,rowNumber);

                    buffer.Clear();
                }
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(new ImportError
                {
                    RowNumber = rowNumber,
                    Message = ex.Message
                });
            }

            progressHandler?.Invoke(new ImportProgress
            {
                ProcessedRows = rowNumber
            });
        }

        if (buffer.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await batchHandler(buffer,rowNumber);
        }
    }

    public int GetTotalRows(IFormFile file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        int totalRows = 0;

        using (var stream = file.OpenReadStream())
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            do
            {
                while (reader.Read())
                {
                    totalRows++;
                }
            }
            while (reader.NextResult()); // next sheet
        }

        return totalRows;
    }
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ImportProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
}