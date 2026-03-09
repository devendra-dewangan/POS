using System.Text;
using ExcelDataReader;
using ExcelDataReader.Log;

public class ExcelReaderService
{
    public async Task ReadExcelAsync<T>(
        string filePath,
        int batchSize,
        Func<List<T>, int, Task> batchHandler,
        Action<ImportError> errorHandler,
        Action<ImportProgress> progressHandler,
        CancellationToken cancellationToken
    ) where T : new()
    {
        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);

        using var stream = File.OpenRead(filePath);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var properties = typeof(T).GetProperties();
        var buffer = new List<T>(batchSize);

        int rowNumber = 0;
        bool isHeader = true;

        do
        {   
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

                        await batchHandler(buffer, rowNumber);

                        buffer.Clear();
                        progressHandler?.Invoke(new ImportProgress
                        {
                            ProcessedRows = rowNumber,
                        });
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
            }
        }
        while (reader.NextResult());

        if (buffer.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await batchHandler(buffer, rowNumber);
            progressHandler?.Invoke(new ImportProgress
            {
                ProcessedRows = rowNumber,
            });
        }
    }

    public async Task<int> GetTotalRows(string filePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        int totalRows = 0;

        using var stream = File.OpenRead(filePath);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        do
        {
            while (reader.Read())
            {
                totalRows++;
            }
        }
        while (reader.NextResult());

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