namespace VMart.Dto
{
    public class CartDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => ProductPrice * Quantity;
    }
}
