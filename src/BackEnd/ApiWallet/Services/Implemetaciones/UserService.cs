using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiWallet.Data;
using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiWallet.Services.Implemetaciones
{
    public class UserService : IUserService
    {
        private readonly WalletDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSecretKey;
        public UserService(WalletDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtSecretKey = configuration.GetSection("jwt").Get<JwtSettings>()!;
        }

        // Registrar un usuario, guardando su conntraseña hasheada
        public async Task<User> RegisterUserAsync(UserRegisterDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                CreatedAt = userDto.CreatedAt
            };
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, userDto.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Iniciar sesión de un usuario, verificando su contraseña hasheada y generando un token JWT
        public async Task<string?> LoginUserAsync(UserLoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (user == null)
            {
                throw new Exception("Usuario no encontrado");
            }
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userDto.PasswordHash);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Contraseña incorrecta");
            }

            // Crear claims (puedes agregar más)
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("UserId", user.Id.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSecretKey.Issuer,
                audience: _jwtSecretKey.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSecretKey.ExpiresInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Actualizar un usuario, permitiendo cambiar su nombre de usuario, correo electrónico y contraseña
        public async Task<bool> UpdateUserAsync(int userId, UserUpdateDto userDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("Usuario no encontrado");
            }
            user.Username = userDto.Username ?? user.Username;
            user.Email = userDto.Email ?? user.Email;
            if (!string.IsNullOrEmpty(userDto.ConfirmPassword))
            {
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, userDto.ConfirmPassword);
            }
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
