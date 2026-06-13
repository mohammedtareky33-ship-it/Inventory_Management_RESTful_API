using InventoryShared;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static InventoryShared.InvoiceTypes;
using static InventoryShared.ResonsOfStockMovementsClass;

using InventoryDAL.Repos;
using InventoryBL.Interfaces;
using InventoryDAL.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace InventoryBL.Services
{
    public class InvoiceService : IInvoiceService
    {
        IBatchServiceForInvoice _batchesService;
        IInvoiceDAL _invoiceDataAccess;
        IInvoiceItemDAL _invoiceItemDataAccess;
        public InvoiceService(IBatchServiceForInvoice batchesService, IInvoiceDAL invoiceDataAccess, IInvoiceItemDAL invoiceItemDataAccess)
        {
            _batchesService=batchesService;
            _invoiceDataAccess=invoiceDataAccess;
            _invoiceItemDataAccess=invoiceItemDataAccess;
        }

        void HandleDuplicateItemsInPurchaseInvoice(List<InvoiceItemAddDTO> items)
        {
            var HandledItems = items.GroupBy(n => new { n.ProductId, n.PurchasePrice }).Select(n => new InvoiceItemAddDTO(
                n.Key.ProductId, n.Key.PurchasePrice, n.Sum(x => x.Quantity)
                )).ToList();
            items.Clear();
            items.AddRange(HandledItems);
        }
        void HandleDuplicateItemsInReturnSaleInvoice(List<InvoiceItemAddDTO> items)
        {
            var HandledItems = items.GroupBy(n => new { n.BatchId, n.PurchasePrice }).Select(n => new InvoiceItemAddDTO(
                n.Key.BatchId, n.Key.PurchasePrice, null, n.Sum(x => x.Quantity)
                )).ToList();
            items.Clear();
            items.AddRange(HandledItems);
        }

        void HandleDuplicateItemsInSaleAndReturnPurchase(List<InvoiceItemAddDTO> items)
        {
            var HandledItems = items.GroupBy(n => new { n.BatchId, n.PurchasePrice, n.SalePrice }).Select(n => new InvoiceItemAddDTO(
               n.Key.BatchId, n.Key.PurchasePrice, n.Key.SalePrice, n.Sum(x => x.Quantity)
               )).ToList();

            items.Clear();
            items.AddRange(HandledItems);
        }
        bool isPurchaseInvoiceItemsValid(List<InvoiceItemAddDTO> invoiceItems)
        {
            if(invoiceItems==null||invoiceItems.Count == 0) return false;

            foreach (var item in invoiceItems)
            {

                if (item.SalePrice != null || item.Quantity <= 0 || item.ProductId <= 0 || item.PurchasePrice < 0)
                    return false;
            }
            return true;
        }
        bool isSaleInvoiceItemsValid(List<InvoiceItemAddDTO> invoiceItems)
        {

            if (invoiceItems==null||invoiceItems.Count == 0) return false;
            foreach (var item in invoiceItems)
            {

                if (item.SalePrice == null || item.Quantity <= 0 || item.PurchasePrice < 0 || item.BatchId <= 0)
                    return false;

            }
            return true;

        }
        bool isReturnSaleInvoiceItemsValid(List<InvoiceItemAddDTO> invoiceItems, List<InvoiceItemSimpleReadDTO> prevInvoiceItems)
        {
            if (invoiceItems==null||invoiceItems.Count == 0||prevInvoiceItems==null||prevInvoiceItems.Count==0) return false;

            foreach (var item in invoiceItems)
            {

                if (item.SalePrice != null || item.Quantity <= 0 || item.PurchasePrice < 0 || item.BatchId <= 0 || !prevInvoiceItems.Exists(n => n.Quantity >= item.Quantity && n.BatchId == item.BatchId))
                    return false;

            }
            return true;

        }
        bool isReturnPurchaseInvoiceItemsValid(List<InvoiceItemAddDTO> invoiceItems, List<InvoiceItemReadDTO> prevInvoiceItems)
        {
            if (invoiceItems==null||invoiceItems.Count == 0) return false;

            foreach (var item in invoiceItems)
            {

                if (item.SalePrice == null || item.Quantity <= 0 || item.PurchasePrice < 0 || item.BatchId <= 0 || !prevInvoiceItems.Exists(n => n.Quantity >= item.Quantity && n.BatchId == item.BatchId))
                    return false;

            }
            return true;

        }


        async Task WithdrawFromBatchesForSaleInvoice(List<InvoiceItemAddDTO> invoiceItems, int currentUserId, SqlConnection con, SqlTransaction tran)
        {


            foreach (var item in invoiceItems)
            {

                ;
                BatchSimpleReadDTO batch = await _batchesService.getBatchSimpleInfo(item.BatchId, con, tran);
                if (batch == null || !batch.IsActive || item.PurchasePrice != batch.PurchasePrice || item.SalePrice != batch.SalePrice || item.Quantity > batch.Quantity)
                    throw new ValidationException("some items added with invalid Batches or non avilable batches or quantity");
                if (!await _batchesService.UpdateQuantityForInvoices(new BatchUpdateQuantityDTO(item.BatchId, item.Quantity, currentUserId, DateTime.Now), enReasonOfMovement.Sale, con, tran))
                    throw new Exception("some atches withdraw failed");
            }

        }
        async Task WithdrawFromBatchesForReturnPurchaseInvoice(List<InvoiceItemAddDTO> invoiceItems, int currentUserId, SqlConnection con, SqlTransaction tran)
        {


            foreach (var item in invoiceItems)
            {

                ;
                BatchSimpleReadDTO batch = await _batchesService.getBatchSimpleInfo(item.BatchId, con, tran);
                if (batch == null || item.PurchasePrice != batch.PurchasePrice || item.Quantity > batch.Quantity)
                    throw new ValidationException("some items added with invalid Batches or non avilable batches or quantity");
                if (!await _batchesService.UpdateQuantityForInvoices(new BatchUpdateQuantityDTO(item.BatchId, item.Quantity, currentUserId, DateTime.Now), enReasonOfMovement.ReturnPurchase, con, tran))
                    throw new Exception("some atches withdraw failed");
            }

        }
        async Task IncreaseBatchesForReturnSaleInvoice(List<InvoiceItemAddDTO> invoiceItems, int currentUserId, SqlConnection con, SqlTransaction tran)
        {


            foreach (var item in invoiceItems)
            {



                if (!await _batchesService.UpdateQuantityForInvoices(new BatchUpdateQuantityDTO(item.BatchId, item.Quantity, currentUserId, DateTime.Now), enReasonOfMovement.ReturnSale, con, tran))
                    throw new Exception("some atches withdraw failed");
            }

        }
        async Task createBatchesInPurchaseInvoice(List<InvoiceItemAddDTO> invoiceItems, int createdByUserId, SqlConnection con, SqlTransaction tran)
        {
            foreach (var item in invoiceItems)
            {
                int batchId = await _batchesService.Add(new BatchAddDTO(item.ProductId, item.Quantity, item.PurchasePrice, createdByUserId), con, tran);
                if (batchId < 0)
                    throw new Exception("some batches Added failed");
                item.BatchId = batchId;

            }

        }
        async Task AddAllInvoiceItems(List<InvoiceItemAddDTO> invoiceItems, SqlConnection con, SqlTransaction tran)
        {
            foreach (var item in invoiceItems)
            {

                int id = await _invoiceItemDataAccess.Add(item, con, tran);
                if (id < 0)
                    throw new Exception("failed add some Invoice Items");
            }

        }

         async Task<int> AddPurchaseInvoice(InvoiceAddDTO invoice)
        {
            int invoiceId = -1;
            HandleDuplicateItemsInPurchaseInvoice(invoice.InvoiceItems);
            try
            {
                if (invoice.TypeId != (int)enInvoiceType.Purchase || invoice.PrevInvoiceId != null)
                    throw new ValidationException("this function for purchase invoices Only");
                if (!isPurchaseInvoiceItemsValid(invoice.InvoiceItems))
                    throw new ValidationException("not valid values in invoice items");
       
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlTransaction tran = con.BeginTransaction())
                    {


                        try
                        {

                            await createBatchesInPurchaseInvoice(invoice.InvoiceItems, invoice.CreatedByUserId, con, tran);

                            invoice.TotalPurchasePrice = invoice.InvoiceItems.Sum(n => n.PurchasePrice * n.Quantity);
                            invoice.TotalSalePrice = null;
                            invoiceId = await _invoiceDataAccess.Add(invoice, con, tran);
                            if (invoiceId < 0)
                                throw new Exception("failed add main Invoice");
                            invoice.InvoiceItems.ForEach(item => item.InvoiceId = invoiceId);
                            await AddAllInvoiceItems(invoice.InvoiceItems, con, tran);

                            tran.Commit();

                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw;


                        }



                    }


                }
            }
            catch (Exception e)
            {
                throw;
            }
            return invoiceId;
        }
         async Task<int> AddSaleInvoice(InvoiceAddDTO invoice)
        {
            int invoiceId = -1;
            HandleDuplicateItemsInSaleAndReturnPurchase(invoice.InvoiceItems);
           
                if (invoice.TypeId != (int)enInvoiceType.Sale || invoice.PrevInvoiceId != null)
                    throw new ValidationException("this function for Sales invoices Only");
                if (!isSaleInvoiceItemsValid(invoice.InvoiceItems))
                    throw new ValidationException("this items have invalid fields");
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            await WithdrawFromBatchesForSaleInvoice(invoice.InvoiceItems, invoice.CreatedByUserId, con, tran);
                            invoice.TotalPurchasePrice = invoice.InvoiceItems.Sum(n => n.PurchasePrice * n.Quantity);
                            invoice.TotalSalePrice = invoice.InvoiceItems.Sum(n => n.SalePrice * n.Quantity); ;
                            invoiceId = await _invoiceDataAccess.Add(invoice, con, tran);
                            if (invoiceId < 0)
                                throw new Exception("failed add main Invoice");
                            invoice.InvoiceItems.ForEach(item => item.InvoiceId = invoiceId);
                            await AddAllInvoiceItems(invoice.InvoiceItems, con, tran);
                            tran.Commit();

                        }
                        catch
                        {

                            tran.Rollback();
                            throw;

                        }





                    }



                }

            
           
            return invoiceId;
        }
        async Task<InvoiceSimpleReadDTO> getInvoiceSimpleInfo(int invoiceId, SqlConnection con, SqlTransaction tran)
        {

            InvoiceSimpleReadDTO invoice = await _invoiceDataAccess.getInvoiceSimpleInfo(invoiceId, con, tran);
            if (invoice == null)
                throw new NotFoundException(" Invoice not found");
            invoice.Items = await _invoiceItemDataAccess.getInvoiceItems(invoiceId, con, tran);
            if (invoice.Items == null)
                throw new ValidationException("this haven't any Items");
            return invoice;
        }
        async Task<(InvoiceSimpleReadDTO invoice, List<InvoiceItemSimpleReadDTO> items)> getInvoiceSimpleInfoWithAvilableQuantitiesForReturnSale(int invoiceId, SqlConnection con, SqlTransaction tran)
        {

            InvoiceSimpleReadDTO invoice = await _invoiceDataAccess.getInvoiceSimpleInfo(invoiceId, con, tran);
            if (invoice == null)
                throw new NotFoundException(" Invoice not found");
            List<InvoiceItemSimpleReadDTO> items = await _invoiceItemDataAccess.getRemainderOfItemsQuantityToReturn(invoiceId, con, tran);
            return (invoice, items);
        }
         async Task<int> AddReturnSaleInvoice(InvoiceAddDTO invoice)
        {

            HandleDuplicateItemsInReturnSaleInvoice(invoice.InvoiceItems);
            int invoiceId = -1;
            try
            {
                if (invoice.PrevInvoiceId == null || invoice.TypeId != (int)enInvoiceType.ReturnSale)
                    throw new ValidationException("this function for return sale invoices only");
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            (InvoiceSimpleReadDTO prevInvoice, List<InvoiceItemSimpleReadDTO> Items) prevInvoiceDetails = await getInvoiceSimpleInfoWithAvilableQuantitiesForReturnSale((int)invoice.PrevInvoiceId, con, tran);
                            if (prevInvoiceDetails.prevInvoice.TypeId != (int)enInvoiceType.Sale)
                                throw new ValidationException("the prev Invoice isnt sale");
                            if (!isReturnSaleInvoiceItemsValid(invoice.InvoiceItems, prevInvoiceDetails.Items))
                            {
                                throw new ValidationException("items is not valid");
                            }
                            invoice.TotalPurchasePrice = invoice.InvoiceItems.Sum(n => n.PurchasePrice * n.Quantity);
                            await IncreaseBatchesForReturnSaleInvoice(invoice.InvoiceItems, invoice.CreatedByUserId, con, tran);
                            invoiceId = await _invoiceDataAccess.Add(invoice, con, tran);
                            if (invoiceId < 0)
                                throw new Exception("failed add main Invoice");
                            invoice.InvoiceItems.ForEach(item => item.InvoiceId = invoiceId);
                            await AddAllInvoiceItems(invoice.InvoiceItems, con, tran);
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch { throw; }
            return invoiceId;
        }
         async Task<int> AddReturnPurchaseInvoice(InvoiceAddDTO invoice)
        {
            HandleDuplicateItemsInSaleAndReturnPurchase(invoice.InvoiceItems);
            int invoiceId = -1;
            try
            {

                if (invoice.PrevInvoiceId == null || invoice.TypeId != (int)enInvoiceType.ReturnPurchase)
                    throw new ValidationException("this function for return purchase invoices only");

                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();

                    using (SqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            InvoiceSimpleReadDTO prevInvoice = await getInvoiceSimpleInfo((int)invoice.PrevInvoiceId, con, tran);
                            if (prevInvoice.TypeId != (int)enInvoiceType.Purchase)
                                throw new ValidationException("the prev Invoice isnt purchase");
                            if (!isReturnPurchaseInvoiceItemsValid(invoice.InvoiceItems, prevInvoice.Items))
                                throw new ValidationException("items is not valid");
                            await WithdrawFromBatchesForReturnPurchaseInvoice(invoice.InvoiceItems, invoice.CreatedByUserId, con, tran);
                            invoice.TotalPurchasePrice = invoice.InvoiceItems.Sum(n => n.PurchasePrice * n.Quantity);
                            invoice.TotalSalePrice = invoice.InvoiceItems.Sum(n => n.SalePrice * n.Quantity);
                            invoiceId = await _invoiceDataAccess.Add(invoice, con, tran);
                            if (invoiceId < 0)
                                throw new Exception("failed add main Invoice");
                            invoice.InvoiceItems.ForEach(item => item.InvoiceId = invoiceId);
                            await AddAllInvoiceItems(invoice.InvoiceItems, con, tran);
                            tran.Commit();
                        }
                        catch (Exception ex) { tran.Rollback(); throw; }

                    }
                }

            }
            catch
            {
                throw;
            }

            return invoiceId;
        }
         async Task<int> AddExpenseInvoice(InvoiceAddDTO invoice)
        {
            int invoiceId = -1;
    
            if (invoice.TypeId != (int)enInvoiceType.Expenses || invoice.PrevInvoiceId != null || invoice.TotalSalePrice != null)
                throw new ValidationException("this function for Expense invoices Only");
            invoiceId = await _invoiceDataAccess.Add(invoice);

            return invoiceId;
        }
        public async Task<int> Add(InvoiceAddDTO invoice)
        {
            if (invoice.TypeId != (int)enInvoiceType.Expenses&&(invoice.InvoiceItems==null||invoice.InvoiceItems.Count==0)) throw new ValidationException("Invoice Item must be Added");
            switch (invoice.TypeId) {
                case (int)enInvoiceType.Purchase:
                    return await AddPurchaseInvoice(invoice);
                case (int)enInvoiceType.Sale:
                    return await AddSaleInvoice(invoice);
                case (int)enInvoiceType.ReturnPurchase:
                    return await AddReturnPurchaseInvoice(invoice);
                case (int)enInvoiceType.ReturnSale:
                    return await AddReturnSaleInvoice(invoice);
                case (int)enInvoiceType.Expenses:
                    return await AddExpenseInvoice(invoice);
                default:
                    throw new ValidationException($"there is not type with typeId={invoice.TypeId}");


            }
        }
        public async Task<InvoiceReadDTO> getInvoiceWithDetails(int invoiceId)
        {

            InvoiceReadDTO invoice = await _invoiceDataAccess.getInvoiceWithDetails(invoiceId);
            if (invoice == null) throw new NotFoundException("not found");
            return invoice;
        }
        public async Task<CashFlowReadDTO> getMoneyFlowBetween(DateTime start, DateTime end)
        {
         
            CashFlowReadDTO moneyFlow = await _invoiceDataAccess.getMoneyFlowBetween(start, end);
            if (moneyFlow == null)
                throw new NotFoundException("failed get money flow");
            return moneyFlow;
        }
        public async Task<CashFlowReadDTO> getSalesMoneyFlow(DateTime start, DateTime end)
        {

            CashFlowReadDTO moneyFlow = await _invoiceDataAccess.getSalesMoneyFlowBetween(start, end);
            if (moneyFlow == null)
                throw new NotFoundException("failed get money flow");
            return moneyFlow;
        }

        public async Task<ReadAllInvoicesDTO> getAllInvoicesWithFilters(InvoicesFilterDTO filter)
        {
            return await _invoiceDataAccess.getAllInvoicesWithFilters(filter);

        }
        public async Task<InvoiceReadDTO> getInvoiceWithItemsQuantityAvilableForReturn(int invoiceId)
        {
            InvoiceReadDTO invoice = null;

            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                invoice = await _invoiceDataAccess.getInvoiceWithDetailsWithOutItems(invoiceId, con);
                if (invoice == null)
                    throw new NotFoundException(" Invoice not found");
                invoice.Items.AddRange( await _invoiceItemDataAccess.getRemainderOfItemsQuantityToReturnWithDetails(invoiceId, con));
            }
            return invoice;

        }
       
    }
}
