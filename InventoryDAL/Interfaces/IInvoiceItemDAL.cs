using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryDAL.Interfaces
{
    public interface IInvoiceItemDAL
    {
        Task<int> Add(InvoiceItemAddDTO invoiceItem, SqlConnection con, SqlTransaction tran);
        Task<List<InvoiceItemReadDTO>> getInvoiceItems(int invoiceId);
        Task<List<InvoiceItemReadDTO>> getInvoiceItems(int invoiceId, SqlConnection con);
        Task<List<InvoiceItemReadDTO>> getInvoiceItems(int invoiceId, SqlConnection con, SqlTransaction tran);
        Task<List<InvoiceItemSimpleReadDTO>> getRemainderOfItemsQuantityToReturn(int MainInvoiceID, SqlConnection con, SqlTransaction tran);
        Task<List<InvoiceItemReadDTO>> getRemainderOfItemsQuantityToReturnWithDetails(int MainInvoiceID, SqlConnection con);
    }
}