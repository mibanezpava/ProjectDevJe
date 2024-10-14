using jejames.api.ApiFactura.Models;
using jejames.api.ApiFactura.DTOs;
using System.Threading.Tasks;

namespace jejames.api.ApiFactura.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task RegisterAsync(RegisterDto registerDto);
        Task<string> GenerateJwtTokenAsync(User user);
    }
}
