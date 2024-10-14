using jejames.api.ApiFactura.Models;
using System.Threading.Tasks;

namespace jejames.api.ApiFactura.Repositories
{
    public interface IUserRepository
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task RegisterAsync(string username, string email, string password);
    }
}
