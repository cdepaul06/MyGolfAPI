using System.ComponentModel.DataAnnotations;

namespace MyGolfAPI.Entities
{
    public class User
    {
        public int Id { get; set; }

        // Auth0 unique user id (JWT "sub")
        [Required]
        public string Auth0Sub { get; set; } = null!;

        public string Username { get; set; } = null!;
        public string NormalizedUsername { get; set; } = null!;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
