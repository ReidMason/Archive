using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryAPI.Models;

namespace InventoryAPI.Dtos.InventoryItem
{
    public class GetInventoryItemDto
    {
        // Remove properties you don't want displayed
        public int? Id { get;  set; } = null;
        public string Name { get; set; }
        public string Location { get; set; }
    }
}
