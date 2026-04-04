namespace POS.Services
{
    public interface IIdentityService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<AuthResponse> LoginAsync(LoginDto dto);
        Task AssignRoleAsync(string userId, string role);
    }

    public class LoginDto
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterDto
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}