using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SOL.Application.Common.Interfaces;
using Template.Domain.Primitives.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Template.Application.Common.Core.Response;
using Template.Domain.Exceptions.Http;

namespace Template.Infrastructe.Services;

public class AuthService<TUser> : IAuthService<TUser>
    where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IRepository _repository;
    private readonly ITokenCacheService _tokenCacheService;

    public AuthService(
        UserManager<TUser> userManager,
        IConfiguration configuration,
        IEmailService emailService, IRepository repository,
        ITokenCacheService tokenCacheService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _repository = repository;
        _tokenCacheService = tokenCacheService;
    }
    public async Task<OperationResponse<string>>ForgetPasswordAsync(TUser user)
    {
        
        var restToken = GenerateResetToken();
        user.SetResetPasswordToken(HashToken(restToken),DateTime.UtcNow.AddHours(1));
        await _userManager.UpdateAsync(user);
        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email!,
                restToken,
                user.UserName??""
            );
        }
        catch (Exception emailEx)
        {
            // Log email error but don't fail the request
            Console.WriteLine($"Failed to send email: {emailEx.Message}");
        }
        return restToken;
    }
    /*
    public async Task<AuthResponseDto> ForgetPasswordAsync(ForgetPasswordDto forgetPasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);
            
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "If the email exists, a password reset link has been sent"
                };
            }

            var resetToken = GenerateResetToken();
            user.SetResetPasswordToken(resetToken, DateTime.UtcNow.AddHours(1));

            await _userManager.UpdateAsync(user);

            // Send email with reset token
            try
            {
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email!,
                    resetToken,
                    user.FullName
                );
            }
            catch (Exception emailEx)
            {
                // Log email error but don't fail the request
                Console.WriteLine($"Failed to send email: {emailEx.Message}");
            }

            return new AuthResponseDto
            {
                Success = true,
                Message = "If the email exists, a password reset link has been sent",
                Data = new { ResetToken = resetToken } // For development/testing only
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            };
        }
    }
    */

    public async Task<OperationResponse<RefreshTokenResult>> GenerateRefreshToken(Guid? userId, string? oldRefreshToken = null, CancellationToken cancellationToken = default)
    {
        return await GenerateRefreshToken(userId, oldRefreshToken, null, null, cancellationToken);
    }

    public async Task<OperationResponse<RefreshTokenResult>> GenerateRefreshToken(Guid? userId, string? oldRefreshToken = null,
        string? deviceId = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var token = GenerateRefreshToken();
        RefreshToken? refreshToken = null;
        if (oldRefreshToken is not null)
        {
            var oldRefreshTokenHash = HashToken(oldRefreshToken ?? "");
            refreshToken = await _repository.TrackingQuery<RefreshToken>().FirstOrDefaultAsync(rt =>
                !rt.DateDeleted.HasValue && rt.RefreshTokenHash == oldRefreshTokenHash &&
                !rt.IsUsed && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken: cancellationToken);
            if (refreshToken is null)
                return new HttpMessage("Invalid RefreshToken", HttpStatusCode.BadRequest);
            refreshToken.Renew(HashToken(token), DateTime.UtcNow + TimeSpan.FromDays(15));
            if (deviceId is not null || ipAddress is not null)
            {
                refreshToken.UpdateDeviceInfo(deviceId, ipAddress);
            }
        }
        if (userId is not null)
        {
            var hashToken = HashToken(token);
            refreshToken = new RefreshToken(hashToken, DateTime.UtcNow + TimeSpan.FromDays(15), userId.Value);
            if (deviceId is not null || ipAddress is not null)
            {
                refreshToken.UpdateDeviceInfo(deviceId, ipAddress);
            }
            await _repository.AddAsync(refreshToken);
        }
        await _repository.SaveChangesAsync(cancellationToken);
        return new RefreshTokenResult
        {
            UserId = refreshToken!.UserId,
            RefreshToken = token,
            DeviceId = refreshToken.DeviceId
        };
    }
    

    public async Task<string> GenerateAccessToken(TUser user, string? deviceId = null)
    {
        var jti = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("IsOwner", user.UserClaims.FirstOrDefault(uc=>uc.ClaimType=="isOwner")?.ClaimValue??"false"),
            new Claim(ClaimTypes.Email, user.Email??""),
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        await _tokenCacheService.SaveAccessTokenAsync(
            user.Id.ToString(),
            jti,
            TimeSpan.FromDays(7),
            deviceId
        );

        return tokenString;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string GenerateResetToken()
    {
        byte[] bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
        int number = value % 1_000_000; 
        return number.ToString("D6");
    }


    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
