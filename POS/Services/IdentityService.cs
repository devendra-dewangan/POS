using Microsoft.AspNetCore.Identity;
using POS.Models;
using POS.Repos;

namespace POS.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public IdentityService(UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _tokenService = new TokenService();
            _unitOfWork = unitOfWork;
        }
        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            // 🔥 Fetch roles HERE (correct place)
            var roles = await _userManager.GetRolesAsync(user);

            // 🔥 Pass roles to token service
            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            var tokenHash = _tokenService.HashToken(refreshToken);

            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
            });

            await _unitOfWork.CommitAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                EmployeeCode = dto.EmployeeCode
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return string.Join(",", result.Errors.Select(x => x.Description));

            return "Success";
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken)
        {
            var tokenHash = _tokenService.HashToken(refreshToken);

            var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(tokenHash);

            if (storedToken == null || storedToken.Revoked != null || storedToken.Expires < DateTime.UtcNow)
                return null;

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            var roles = await _userManager.GetRolesAsync(user!);

            // 🔥 revoke old token
            storedToken.Revoked = DateTime.UtcNow;

            // 🔥 generate new refresh token
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newHash = _tokenService.HashToken(newRefreshToken);

            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                TokenHash = newHash,
                UserId = user!.Id,
            });

            await _unitOfWork.CommitAsync();

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokenHash = _tokenService.HashToken(refreshToken);

            var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(tokenHash);

            if (token != null)
            {
                token.Revoked = DateTime.UtcNow;
                await _unitOfWork.CommitAsync();
            }
        }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}