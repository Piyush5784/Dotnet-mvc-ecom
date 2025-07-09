using System.ComponentModel.DataAnnotations;

namespace VMart.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(500)]
        public string Message { get; set; }
        
    }
}
