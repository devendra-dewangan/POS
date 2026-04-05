using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using POS.Models;

namespace POS.Services.Import
{
    public interface IImportService
    {
        Task<bool> ImportPurchaseDataAsync(string file);
        Task<bool> ImportSaleDataAsync(string file);
        Task<bool> DeleteImportDataAsync(int importId);
    }
}
