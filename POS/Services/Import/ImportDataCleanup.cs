using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POS.Data;
using POS.Models;
using Microsoft.EntityFrameworkCore;

namespace POS.Services.Import
{
    public class ImportDataCleanup
    {
        private readonly AppDbContext _context;

        public ImportDataCleanup(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> DeleteImportDataAsync(Guid importId)
        {
            var records = await _context.ImportPurchaseTemp
                .Where(t => t.ImportId == importId)
                .ToListAsync();

            if (!records.Any())
            {
                return false;
            }

            _context.ImportPurchaseTemp.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}