using Microsoft.AspNetCore.Http;
using VMart.Models;

namespace VMart.Dto
{
    public class CreateProductDto
    {
        public Product Product { get; set; } = new();
        public IFormFile? Image { get; set; }
        public List<Category> Categories { get; set; } = new();
    }
}
