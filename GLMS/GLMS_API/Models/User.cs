namespace GLMS_API.Models
{
    public partial class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Role { get; set; } = null!;

        public int? ClientId { get; set; }

        public virtual Client? Client { get; set; }
    }
}