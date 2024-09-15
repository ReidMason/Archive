using InventoryAPI.Dtos.InventoryItem;
using InventoryAPI.Models;
using InventoryAPI.Services.InventoryService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryItemsController : ControllerBase
    {
        private readonly IInventoryItemService _inventoryItemService;

        public InventoryItemsController(IInventoryItemService inventoryItemService)
        {
            _inventoryItemService = inventoryItemService;
        }
        [EnableCors()]
        public async Task<IActionResult> Get()
        {
            return Ok(await _inventoryItemService.GetAllInventoryItems());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingle(int id)
        {
            ServiceResponse<GetInventoryItemDto> response = await _inventoryItemService.GetInventoryItemById(id);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
            
        }

        [HttpPost]
        public async Task<IActionResult> AddInventoryItem(AddInventoryItemDto newInventoryItem)
        {
            return Ok(await _inventoryItemService.AddInventoryItem(newInventoryItem));
        }

        [HttpDelete("DeleteInventoryItem/{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            return Ok(await _inventoryItemService.DeleteInventoryItem(id));
        }
    }
}