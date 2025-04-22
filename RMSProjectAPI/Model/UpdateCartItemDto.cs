namespace RMSProjectAPI.Model
{
    public class UpdateCartItemDto
    {
        public Guid UserId { get; set; }
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}
