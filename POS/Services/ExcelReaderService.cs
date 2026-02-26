using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace POS.Services
{
    public class ExcelReaderService
    {
        // Generic event for progress updates
        public event Action<int> ProgressChanged;
        
        // Generic event for row data emission - now accepts any type T
        public event Action<object> RowDataEmitted;
        
        // Generic event for completion
        public event Action<bool, string> ProcessingCompleted;

        /// <summary>
        /// Read Excel file and emit data as a stream of the specified type
        /// </summary>
        /// <typeparam name="T">The type to map Excel rows to</typeparam>
        /// <param name="file">The uploaded Excel file</param>
        /// <param name="columnMapping">Dictionary mapping column indices to property names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ReadExcelFileAsync<T>(IFormFile file, Dictionary<int, string> columnMapping, CancellationToken cancellationToken = default) where T : new()
        {
            if (file == null || file.Length == 0)
            {
                OnProcessingCompleted(false, "File is empty or null");
                return;
            }

            // Create a temporary file to work with
            var tempFilePath = Path.GetTempFileName();
            
            try
            {
                // Save uploaded file to temp location
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Read Excel file using compression and XML parsing
                await ReadExcelFromTempFileAsync<T>(tempFilePath, columnMapping, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                OnProcessingCompleted(false, "Processing was cancelled");
            }
            catch (Exception ex)
            {
                OnProcessingCompleted(false, $"Error processing file: {ex.Message}");
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// Read Excel file and emit data as a stream of the specified type with automatic mapping
        /// </summary>
        /// <typeparam name="T">The type to map Excel rows to</typeparam>
        /// <param name="file">The uploaded Excel file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ReadExcelFileAsync<T>(IFormFile file, CancellationToken cancellationToken = default) where T : new()
        {
            // Auto-generate column mapping based on property names
            var columnMapping = GenerateColumnMapping<T>();
            await ReadExcelFileAsync<T>(file, columnMapping, cancellationToken);
        }

        private async Task ReadExcelFromTempFileAsync<T>(string filePath, Dictionary<int, string> columnMapping, CancellationToken cancellationToken) where T : new()
        {
            var buffer = new List<Dictionary<string, string>>();
            var batchSize = 100; // Process rows in batches
            var totalRows = 0;
            var processedRows = 0;

            try
            {
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    // Find the worksheet file (usually in xl/worksheets/sheet1.xml)
                    var worksheetEntry = archive.Entries
                        .FirstOrDefault(e => e.FullName.StartsWith("xl/worksheets/") && e.FullName.EndsWith(".xml"));

                    if (worksheetEntry == null)
                    {
                        throw new InvalidOperationException("Could not find worksheet in Excel file");
                    }

                    using (var stream = worksheetEntry.Open())
                    using (var reader = XmlReader.Create(stream))
                    {
                        var currentRow = new Dictionary<string, string>();
                        var cellIndex = 0;
                        var inRow = false;
                        var inCell = false;
                        var cellValue = "";

                        // First pass: count total rows for progress tracking
                        totalRows = CountTotalRows(worksheetEntry);

                        while (reader.Read() && !cancellationToken.IsCancellationRequested)
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (reader.Name == "row")
                                    {
                                        inRow = true;
                                        currentRow = new Dictionary<string, string>();
                                        cellIndex = 0;
                                    }
                                    else if (reader.Name == "c" && inRow)
                                    {
                                        inCell = true;
                                        cellValue = "";
                                    }
                                    break;

                                case XmlNodeType.Text:
                                    if (inCell)
                                    {
                                        cellValue = reader.Value;
                                    }
                                    break;

                                case XmlNodeType.EndElement:
                                    if (reader.Name == "c" && inCell)
                                    {
                                        inCell = false;
                                        // Store cell value with column index as key
                                        currentRow[$"Column{cellIndex}"] = cellValue;
                                        cellIndex++;
                                    }
                                    else if (reader.Name == "row" && inRow)
                                    {
                                        inRow = false;
                                        if (currentRow.Count > 0)
                                        {
                                            buffer.Add(currentRow);
                                            processedRows++;

                                            // Emit row data when buffer reaches batch size
                                            if (buffer.Count >= batchSize)
                                            {
                                                await EmitBufferedData<T>(buffer, columnMapping, processedRows, totalRows);
                                                buffer.Clear();
                                            }

                                            // Update progress
                                            if (processedRows % 10 == 0) // Update progress every 10 rows
                                            {
                                                OnProgressChanged(processedRows);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        // Emit remaining data in buffer
                        if (buffer.Count > 0)
                        {
                            await EmitBufferedData<T>(buffer, columnMapping, processedRows, totalRows);
                        }

                        OnProcessingCompleted(true, "Excel file processed successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task EmitBufferedData<T>(List<Dictionary<string, string>> buffer, Dictionary<int, string> columnMapping, int processedRows, int totalRows) where T : new()
        {
            foreach (var row in buffer)
            {
                // Map Excel row to the specified type T
                var mappedObject = MapRowToObject<T>(row, columnMapping);
                OnRowDataEmitted(mappedObject);
                await Task.Yield(); // Allow other operations to run
            }
        }

        private T MapRowToObject<T>(Dictionary<string, string> row, Dictionary<int, string> columnMapping) where T : new()
        {
            var instance = new T();
            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var mapping in columnMapping)
            {
                var columnIndex = mapping.Key;
                var propertyName = mapping.Value;
                var columnKey = $"Column{columnIndex}";

                if (row.ContainsKey(columnKey))
                {
                    var cellValue = row[columnKey];
                    var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, System.StringComparison.OrdinalIgnoreCase));

                    if (property != null && property.CanWrite)
                    {
                        try
                        {
                            var convertedValue = ConvertValue(cellValue, property.PropertyType);
                            property.SetValue(instance, convertedValue);
                        }
                        catch
                        {
                            // Skip invalid values
                        }
                    }
                }
            }

            return instance;
        }

        private object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetDefaultValue(targetType);
            }

            try
            {
                if (targetType == typeof(string))
                {
                    return value;
                }
                else if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    return int.TryParse(value, out var result) ? result : GetDefaultValue(targetType);
                }
                else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    return decimal.TryParse(value, out var result) ? result : GetDefaultValue(targetType);
                }
                else if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    return double.TryParse(value, out var result) ? result : GetDefaultValue(targetType);
                }
                else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                {
                    return DateTime.TryParse(value, out var result) ? result : GetDefaultValue(targetType);
                }
                else if (targetType == typeof(bool) || targetType == typeof(bool?))
                {
                    return bool.TryParse(value, out var result) ? result : GetDefaultValue(targetType);
                }
                else
                {
                    return value; // Default to string conversion
                }
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private object GetDefaultValue(Type targetType)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
            {
                return Activator.CreateInstance(targetType);
            }
            return null;
        }

        private Dictionary<int, string> GenerateColumnMapping<T>() where T : new()
        {
            var mapping = new Dictionary<int, string>();
            var type = typeof(T);
            var properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                mapping[i] = properties[i].Name;
            }

            return mapping;
        }

        private int CountTotalRows(ZipArchiveEntry worksheetEntry)
        {
            var rowCount = 0;
            try
            {
                using (var stream = worksheetEntry.Open())
                using (var reader = XmlReader.Create(stream))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "row")
                        {
                            rowCount++;
                        }
                    }
                }
            }
            catch
            {
                // If counting fails, return 0 - progress will not be shown
                return 0;
            }
            return rowCount;
        }

        // Event invokers
        protected virtual void OnProgressChanged(int processedRows)
        {
            ProgressChanged?.Invoke(processedRows);
        }

        protected virtual void OnRowDataEmitted(object rowData)
        {
            RowDataEmitted?.Invoke(rowData);
        }

        protected virtual void OnProcessingCompleted(bool success, string message)
        {
            ProcessingCompleted?.Invoke(success, message);
        }
    }
}
