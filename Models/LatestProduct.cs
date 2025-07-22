namespace VMart.Models
{
    public class LatestProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int DisplayOrder { get; set; }

    }
}
