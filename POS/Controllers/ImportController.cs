using Microsoft.AspNetCore.Mvc;
using POS.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        // POST: api/import/purchase
        [HttpPost("purchase")]
        public async Task<ActionResult> ImportPurchaseExcel()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var result = await _importService.ImportPurchaseDataAsync(file);

                return Ok(new
                {
                    message = result ? "Purchase data imported successfully" : "Failed to import purchase data",
                    fileName = file.FileName,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing file: {ex.Message}");
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

                var result = await _importService.ImportSaleDataAsync(file);

                return Ok(new
                {
                    message = result ? "Sale data imported successfully" : "Failed to import sale data",
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