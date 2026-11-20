using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Auth;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly ICommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService, ICommonService commonService, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
        }
        private void SetTokenCookies(string accessToken, string refreshToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expiresAt,
            };
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response != null)
            {
                response.Cookies.Append("access_token", accessToken, cookieOptions);
                response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
            }
        }
        private void ClearTokenCookies()
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if(response != null)
            {
                response.Cookies.Delete("access_token", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });
                response.Cookies.Delete("refresh_token", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });
            }
        }
        
        public async Task<ApiResponse> LoginAsync(LoginRequestDto dto, string? ipAddress, string? userAgent)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            // Tài khoản đã xoá hoặc bị khoá trả cùng thông báo, để không lộ trạng thái tài khoản
            if(user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)
                || user.DeleteFlg || !user.IsActive)
            {
                return ApiResponse.Response(400, "Sai tên đăng nhập hoặc mật khẩu!");
            }
            using var transaction = await _userRepository.BeginTransactionAsync();
            try
            {
                var refreshTokenModel = _tokenService.GenerateRefreshToken();
                var newRefreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = refreshTokenModel.TokenHash,
                    ExpiresAt = refreshTokenModel.ExpiresAt,
                    IsRevoked = false,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };
                await _refreshTokenRepository.CreateAsync(newRefreshToken);
                await _refreshTokenRepository.SaveChangesAsync();
                string refreshTokenId = newRefreshToken.Id.ToString();
                var acessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, refreshTokenId);
                await transaction.CommitAsync();
                SetTokenCookies(acessToken, refreshTokenModel.RawToken, refreshTokenModel.ExpiresAt);
                var result = new LoginResponseDto
                {
                    user_id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    positions = user.Positions.Select(p => p.Name).ToArray(),
                    expires_at = refreshTokenModel.ExpiresAt,
                    isFirstLogin =user.IsFirstLogin,
                };
                return ApiResponse.Response(200, "Đăng nhập thành công!", result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Exception innerEx = ex;
                while (innerEx.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                return ApiResponse.Response(500, "Đăng nhập thất bại! Lỗi: " + innerEx.Message);
            }
        }

        public async Task<ApiResponse> LogoutAsync()
        {
            var rawRefreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(rawRefreshToken))
            {
                ClearTokenCookies();
                return ApiResponse.Response(200, "Đã đăng xuất (Cookie trống)!");
            }
            string tokenHash = _tokenService.ComputeSha256Hash(rawRefreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (existingToken != null && existingToken.UserId == _commonService.GetUserId())
            {
                existingToken.IsRevoked = true;
                existingToken.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepository.SaveChangesAsync();
            }
            ClearTokenCookies() ;
            return ApiResponse.Response(200, "Đăng xuất thành công!");
        }

        public async Task<ApiResponse> RefreshTokenAsync(string? ipAddress, string? userAgent)
        {
            var rawRefreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(rawRefreshToken))
            {
                return ApiResponse.Response(404, "Không tìm thấy phiên đăng nhập để gia hạn.");
            }
            string tokenHash = _tokenService.ComputeSha256Hash(rawRefreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (existingToken == null)
            {
                return ApiResponse.Response(404, "Thẻ gia hạn không tồn tại hoặc đã bị thu hồi!");
            }
            if (existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                return ApiResponse.Response(403, "Thẻ gia hạn đã hết hạn! Vui lòng đăng nhập lại.");
            }
            if (existingToken.User.DeleteFlg || !existingToken.User.IsActive)
            {
                return ApiResponse.Response(403, "Tài khoản của bạn đã bị khóa!");
            }
            using var transaction = await _refreshTokenRepository.BeginTransactionAsync();
            try
            {
                existingToken.IsRevoked = true;
                existingToken.RevokedAt = DateTime.UtcNow;
                var refreshTokenModel = _tokenService.GenerateRefreshToken();
                var newRefreshToken = new RefreshToken
                {
                    UserId = existingToken.UserId,
                    TokenHash = refreshTokenModel.TokenHash,
                    ExpiresAt = refreshTokenModel.ExpiresAt,
                    IsRevoked = false,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };
                await _refreshTokenRepository.CreateAsync(newRefreshToken);
                await _refreshTokenRepository.SaveChangesAsync();
                var newAcessToken = _tokenService.GenerateAccessToken(existingToken.UserId, existingToken.User.Name, newRefreshToken.Id.ToString());
                await transaction.CommitAsync();
                SetTokenCookies(newAcessToken, refreshTokenModel.RawToken, refreshTokenModel.ExpiresAt);
                var result = new RefreshTokenResponseDto
                {
                    user_id = existingToken.UserId,
                    name = existingToken.User.Name,
                    email = existingToken.User.Email,
                    
                };
                return ApiResponse.Response(200, "Gia hạn token thành công!", result);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse.Response(500, "Lỗi hệ thống khi gia hạn!");
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if(user == null)
            {
                return ApiResponse.Response(404, "Không tìm thấy người dùng!");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.IsFirstLogin = false;
            await _userRepository.UpdateUserAsync(user);
            return ApiResponse.Response(200, "Cập nhật mật khẩu thành công!");
        }

        public async Task<ApiResponse> RevokeAllSessionsAsync()
        {
            var userId = _commonService.GetUserId();
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
            return ApiResponse.Response(200, "Đã thu hồi tất cả phiên đăng nhập của bạn!");
        }
    }
}
