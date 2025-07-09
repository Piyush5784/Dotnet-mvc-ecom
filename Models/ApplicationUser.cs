using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VMart.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public ICollection<Cart> Carts { get; set; }
        public ICollection<Order> OrderItems { get; set; }
    }
}
