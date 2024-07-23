namespace backend.DTO.User
{
    public class LoginRequestDTO
    {
        //  Properties
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
    }
}