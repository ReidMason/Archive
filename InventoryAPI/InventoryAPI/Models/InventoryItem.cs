using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryAPI.Models
{
    public class InventoryItem
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; }
        public string Location { get; set; }
        public int? Quantity { get; set; }
    }
}
