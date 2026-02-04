namespace MyGolfAPI.Entities
{
    public class User
    {      
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
