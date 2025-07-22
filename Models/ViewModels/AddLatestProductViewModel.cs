namespace VMart.Models.ViewModels
{
    public class AddLatestProductViewModel
    {
        public LatestProduct NewProduct { get; set; } = new LatestProduct();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
