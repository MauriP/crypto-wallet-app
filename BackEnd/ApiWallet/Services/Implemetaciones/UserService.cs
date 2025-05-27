using ApiWallet.Data;
using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ApiWallet.Services.Implemetaciones
{
    public class UserService : IUserService
    {
        private readonly WalletDbContext _context;
        private readonly IConfiguration _configuration;
        public UserService(WalletDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

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
            return user.PasswordHash;
        }

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
