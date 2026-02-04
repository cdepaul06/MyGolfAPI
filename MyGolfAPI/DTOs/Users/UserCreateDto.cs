using System.ComponentModel.DataAnnotations;

namespace MyGolfAPI.DTOs.Users
{
    public class UserCreateDto
    {
        [MaxLength(50)]
        public string? Username { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }
    }
}
