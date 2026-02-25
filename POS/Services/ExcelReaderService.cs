using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace POS.Services
{
    public class ExcelReaderService
    {
        public async Task<List<Dictionary<string, string>>> ReadExcelFileAsync(IFormFile file)
        {
            var results = new List<Dictionary<string, string>>();
            
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            // Create a temporary file to work with
            var tempFilePath = Path.GetTempFileName();
            
            try
            {
                // Save uploaded file to temp location
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Read Excel file using compression and XML parsing
                results = ReadExcelFromTempFile(tempFilePath);
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return results;
        }

        private List<Dictionary<string, string>> ReadExcelFromTempFile(string filePath)
        {
            var results = new List<Dictionary<string, string>>();
            
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

                    while (reader.Read())
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
                                        results.Add(currentRow);
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return results;
        }
    }
}