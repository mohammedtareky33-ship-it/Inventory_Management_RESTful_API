using InventoryBL.Interfaces;
using InventoryShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagemetRESTFUL_API.Controllers
{
    [Authorize]
    [Authorize(Policy = "Permissions:Reports")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        IInvoiceService _InvoiceService;
        public ReportsController(IInvoiceService service) {
        _InvoiceService = service;
        }
        [HttpGet("TotalMoneyFlow", Name = "TotalMoneyFlow")]
        public async Task<IActionResult> GetTotalMoneyFlow(DateTime from, DateTime to) {
       
          return Ok(await _InvoiceService.getMoneyFlowBetween(from,to));
          
        }
        [HttpGet("SalesMoneyFlow", Name = "SalesMoneyFlow")]
        public async Task<IActionResult> GetSalesMoneyFlow(DateTime from,DateTime to)
        {
            return Ok(await _InvoiceService.getSalesMoneyFlow(from,to));
        }
    }
}
