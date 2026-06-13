using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryDAL.Interfaces
{
    public interface IInvoiceDAL
    {
        Task<int> Add(InvoiceAddDTO invoice);
        Task<int> Add(InvoiceAddDTO invoice, SqlConnection con, SqlTransaction tran);
        Task<ReadAllInvoicesDTO> getAllInvoicesWithFilters(InvoicesFilterDTO Filter);
        Task<InvoiceSimpleReadDTO> getInvoiceSimpleInfo(int invoiceId, SqlConnection con);
        Task<InvoiceSimpleReadDTO> getInvoiceSimpleInfo(int invoiceId, SqlConnection con, SqlTransaction tran);
        Task<InvoiceReadDTO> getInvoiceWithDetails(int invoiceId);
        Task<InvoiceReadDTO> getInvoiceWithDetails(int invoiceId, SqlConnection con);
        Task<CashFlowReadDTO> getMoneyFlowBetween(DateTime start, DateTime end);
        Task<CashFlowReadDTO> getSalesMoneyFlowBetween(DateTime start, DateTime end);
        Task<InvoiceReadDTO> getInvoiceWithDetailsWithOutItems(int invoiceId, SqlConnection con);
    }
}