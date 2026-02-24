using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImportController(AppDbContext context)
        {
            _context = context;
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

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // For now, just return success and file path
                // You can process the Excel file later
                return Ok(new
                {
                    message = "Purchase file uploaded successfully",
                    fileName = file.FileName,
                    filePath = filePath,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
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

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // For now, just return success and file path
                // You can process the Excel file later
                return Ok(new
                {
                    message = "Sale file uploaded successfully",
                    fileName = file.FileName,
                    filePath = filePath,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
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