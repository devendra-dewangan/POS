using Microsoft.AspNetCore.Mvc;
using POS.Model;
using POS.Services;

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
        {
            var purchaseDraftId = await _purchaseService.AddPurchaseAsync(request.SupplierId);
            return Ok(new CreatePurchaseResponse { PurchaseDraftId = purchaseDraftId });
        }

        [HttpPost("{purchaseCartId}/complete")]
        public async Task<ActionResult> CompletePurchase(int purchaseCartId)
        {
            var purchase = await _purchaseService.CompletePurchaseAsync(purchaseCartId);
            return Ok(new CompletePurchaseResponse(
                purchase.InvoiceNumber,
                purchase.PurchaseDate,
                purchase.PurchaseItems.Sum(i => i.Quantity * i.PurchaseRate),
                [.. purchase.PurchaseItems.Select(i => new PurchaseItemDto(
                    i.ProductId,
                    i.Product!.ProductCode,
                    i.Product.ProductName,
                    i.PurchaseRate,
                    i.Quantity,
                    i.Quantity * i.PurchaseRate
                ))]
            ));
        }

        [HttpPost("{purchaseCartId}/items")]
        public async Task<ActionResult> AddPurchaseItem(int purchaseCartId, [FromBody] AddPurchaseItemRequestDto request)
        {
            // Implementation for adding a purchase item to a purchase cart
            return Ok();
        }

        [HttpPost("{purchaseCartId}/batches")]
        public async Task<ActionResult> AddBatch(int purchaseCartId, [FromBody] AddBatchRequestDto request)
        {
            // Implementation for adding a batch to a purchase cart
            return Ok();
        }


        [HttpGet("invoice/{invoiceNumber}")]
        public async Task<ActionResult> GetPurchaseByInvoiceNumber(string invoiceNumber)
        {
            var purchase = await _purchaseService.GetPurchaseByInvoiceAsync(invoiceNumber);
            if (purchase == null)
            {
                return NotFound();
            }
            return Ok(purchase);
        }
    }
}