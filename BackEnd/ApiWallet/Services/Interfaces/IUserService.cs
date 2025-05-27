using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;

namespace ApiWallet.Services.Interfaces
{
    public interface IUserService
    {
        public Task<User> RegisterUserAsync(UserRegisterDto userDto);
        Task<string?> LoginUserAsync(UserLoginDto userDto);
        Task<bool> UpdateUserAsync(int userId, UserUpdateDto userDto);
    }
}
