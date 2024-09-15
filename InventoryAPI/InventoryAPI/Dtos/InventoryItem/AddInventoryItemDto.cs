namespace InventoryAPI.Dtos.InventoryItem
{
    public class AddInventoryItemDto
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; }

        public string Location { get; set; }
    }
}