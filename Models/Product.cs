using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VMart.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "The Category field is required.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [Range(0, 5, ErrorMessage = "Ratings must be between 0 and 5")]
        public int Ratings { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
