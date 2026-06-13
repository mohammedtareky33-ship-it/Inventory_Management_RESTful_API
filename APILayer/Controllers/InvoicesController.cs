using InventoryBL.Interfaces;
using InventoryBL.Services;
using InventoryShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagemetRESTFUL_API.Controllers
{
    [Authorize]
    [Authorize(Policy ="Permissions:Invoices")]
    [Route("api/Invoices")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        IInvoiceService _invoiceService;
        public InvoicesController(IInvoiceService invoiceService) { 
        _invoiceService = invoiceService;
        }
        [HttpPost("search",Name ="byFilters")]
        public async Task<ActionResult<ReadAllInvoicesDTO>> GetAllInvoices(InvoicesFilterDTO filter)
        {
            return Ok(await _invoiceService.getAllInvoicesWithFilters(filter));
        }
        [HttpGet("Types",Name ="GetInvoiceTypes")]
        public  ActionResult<Dictionary<InvoiceTypes.enInvoiceType,int>> GetInvoiceTypes()
        {
            return Ok(InvoiceTypes.TypesMap);
        }
        [HttpGet("{invoiceId}",Name ="byInvoiceId")]
        public async Task<ActionResult<InvoiceReadDTO>> getInvoice(int invoiceId)
        {
            return Ok(await _invoiceService.getInvoiceWithDetails(invoiceId));
        }
        [HttpGet("{invoiceId}/SaleInvoiceWithRemainingItems", Name = "SaleInvoiceWithRemainingItems")]
       public async Task<ActionResult<InvoiceReadDTO>> getInvoiceWithItemsQuantityAvilableForReturn(int invoiceId)
        {
            return Ok(await _invoiceService.getInvoiceWithItemsQuantityAvilableForReturn(invoiceId));
        }
        [HttpPost(Name = "Add Invoice")]
        public async Task<IActionResult> AddInvoice(InvoiceAddDTO invoice)
        {
            invoice.CreatedByUserId=int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            int invoiceId = await _invoiceService.Add(invoice);
            var newInvoice = new InvoiceSimpleReadDTO(invoiceId, invoice.TypeId, invoice.TotalPurchasePrice, invoice.TotalSalePrice);
            return CreatedAtRoute("byInvoiceId", new { invoiceId }, newInvoice);
        }
    }
}
