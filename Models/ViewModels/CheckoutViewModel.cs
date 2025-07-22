using System.ComponentModel.DataAnnotations;

namespace VMart.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Street address is required")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        public string PostalCode { get; set; }

        public List<Cart>? CartItems { get; set; } 
    }
}
