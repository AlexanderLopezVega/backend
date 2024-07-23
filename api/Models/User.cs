namespace backend.Models
{
    public class User
    {
        //  Properties
        public int ID { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string PasswordSalt { get; set; }
    }
}