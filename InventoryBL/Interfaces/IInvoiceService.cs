using InventoryShared;

namespace InventoryBL.Interfaces
{
    public interface IInvoiceService
    {
        Task<int> Add(InvoiceAddDTO dto);
        Task<ReadAllInvoicesDTO> getAllInvoicesWithFilters(InvoicesFilterDTO Filter);
   
        Task<InvoiceReadDTO> getInvoiceWithDetails(int invoiceId);
        Task<InvoiceReadDTO> getInvoiceWithItemsQuantityAvilableForReturn(int invoiceId);
        Task<CashFlowReadDTO> getMoneyFlowBetween(DateTime start, DateTime end);
        Task<CashFlowReadDTO> getSalesMoneyFlow(DateTime start, DateTime end);
    }
}