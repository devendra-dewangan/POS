using Microsoft.AspNetCore.Mvc;
using POS.Services.Import;
using POS.Services.ImportModels;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ImportQueue _queue;

        public ImportController(ImportQueue queue)
        {
            _queue = queue;
        }

        // GET: api/import/purchase/columns
        [HttpGet("purchase/columns")]
        public ActionResult GetPurchaseColumnStructure()
        {
            var properties = typeof(PurchaseExcelRow).GetProperties();
            var columns = properties.Select((prop, index) => new
            {
                index,
                name = prop.Name,
                type = prop.PropertyType.Name
            });

            return Ok(columns);
        }

        // POST: api/import/purchase
        [HttpPost("purchase")]
        public async Task<ActionResult> ImportPurchaseExcel()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, Guid.NewGuid() + Path.GetExtension(file?.FileName));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // Start import process asynchronously
                await _queue.QueueAsync(new ImportJob
                {
                    FilePath = filePath
                });
                        // TODO: Implement notification mechanism (email, WebSocket, etc.)
                    

                return Ok(new
                {
                    message = "Purchase data processing started",
                    fileName = file.FileName,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error starting import process: {ex.Message}");
            }
        }

        // POST: api/import/sale
        [HttpPost("sale")]
        public async Task<ActionResult> ImportSaleExcel()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // var result = await _importService.ImportSaleDataAsync(file);

                return Ok(new
                {
                    // message = result ? "Sale data imported successfully" : "Failed to import sale data",
                    fileName = file.FileName,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing file: {ex.Message}");
            }
        }

        // GET: api/import/status
        [HttpGet("status")]
        public ActionResult GetImportStatus()
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                return Ok(new { files = new List<object>() });
            }

            var files = Directory.GetFiles(uploadsDir)
                .Select(f => new
                {
                    name = Path.GetFileName(f),
                    path = f,
                    size = new FileInfo(f).Length,
                    created = new FileInfo(f).CreationTime
                })
                .ToList();

            return Ok(new { files = files });
        }
    }
}