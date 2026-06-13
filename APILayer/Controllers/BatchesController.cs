using InventoryBL.Interfaces;
using InventoryShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static InventoryShared.ResonsOfStockMovementsClass;
namespace InventoryManagemetRESTFUL_API.Controllers
{
    [Authorize]
    [Authorize(Policy ="Permissions:Inventory")]
    [Route("api/Batches")]
    [ApiController]
    public class BatchesController : ControllerBase
    {
        IBatchService _BatchService;
        public BatchesController(IBatchService batchService)
        {

            _BatchService = batchService;
        }
        [HttpGet( Name = "BatchesbyProductId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AllBatchesReadDTO>> GetBatchesForProduct(int productId, int pageNum, int pageSize)
        {
           

                return Ok(await _BatchService.getAllBatchesOfProduct(productId, pageNum, pageSize));
           
        }
        [HttpGet("{batchId}", Name = "byBatchId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchReadDTO>> GetByBatchId(int batchId)
        {
           
         

                return Ok(await _BatchService.getBatch(batchId));
            
          
             
            

        }
        [HttpGet("Active", Name = "ActiveBatchForProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchReadDTO>> GetSimpleInfoByBatchId(int productId)
        {
            
           
                return Ok(await _BatchService.getActiveBatchOfProduct(productId));     
          


        }
        [HttpPatch("{batchId}/Activate", Name = "ActiveBatch")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ActivateBatch(int batchId)
        {
            await _BatchService.ActivateBatchAndDeactivateOtherForProduct(batchId);
                return NoContent();


        }
        [HttpPatch("{batchId}/Deactivate", Name = "DeactiveBatch")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeactivateBatch(int batchId)
        {

            await _BatchService.DeactivateBatch(batchId);
                return NoContent();
      

        }
        [HttpPatch("DeactivateAll", Name = "DeactivateAll")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeactivateAllBatchesForProduct(int productId)
        {

            await _BatchService.DeactivateAllBatchesForProductId(productId);
                return NoContent();
          

        }
        [HttpPatch("{batchId}/DisposalStock", Name = "DisposalStock")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DisposalStock(int batchId,BatchUpdateQuantityDTO batch)
        {
     
            batch.BatchId = batchId;
            batch.LastUpdatedUserId=int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _BatchService.DisposalStock(batch);
                return NoContent();
        

        }
        [HttpPatch("{batchId}/UpdateSalePricaAndNotes", Name = "UpdateSalePricaAndNotes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateSalePricaAndNotes(int batchId, BatchUpdateSalePriceAndNotesDTO batch)
        {
            batch.BatchId = batchId;
            batch.LastUpdatedUserId=int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _BatchService.UpdateSalePriceAndNotes(batch);
             return NoContent();
           

        }
        [HttpPost("{batchId}/StockMovements", Name = "StockMovements")]
        public async Task<ActionResult<AllStockMovementsReadDTO>> getAllStockMovements(int batchId,StockMovementsFilter filter)
        {
            return Ok(await _BatchService.getAllMovementsForBatchIdFiltered(batchId, filter));
        }
        [HttpGet("StockMovements/StockMovementReasons",Name = "StockMovementReasons")]
        public ActionResult<Dictionary<enReasonOfMovement,int>> getReasonsOfStockMovements()
        {
            return Ok(ResonsOfStockMovementsClass.ReasonMovementsMap);
        }


    }
}
