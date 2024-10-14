using jejames.api.ApiFactura.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Threading.Tasks;
using BCrypt.Net;

namespace jejames.api.ApiFactura.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Obtener la contraseña hash almacenada para el usuario
            var hashedPassword = await GetHashedPassword(username);
            if (hashedPassword == null || !BCrypt.Net.BCrypt.Verify(password, hashedPassword))
            {
                return null; // Autenticación fallida
            }

            // Obtener información del usuario
            return await GetUserByUsername(username);
        }

        public async Task RegisterAsync(string username, string email, string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            await InsertUser(username, email, hashedPassword);
        }

        private async Task<string> GetHashedPassword(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("SELECT password FROM users WHERE username = @username", connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    return (string)await command.ExecuteScalarAsync();
                }
            }
        }

        private async Task<User> GetUserByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("SELECT id, username, role FROM users WHERE username = @username", connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Role = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return null; // Usuario no encontrado
        }

        private async Task InsertUser(string username, string email, string hashedPassword)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "INSERT INTO users (username, email, password, role) VALUES (@username, @email, @password, 'user')";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    command.Parameters.AddWithValue("email", email);
                    command.Parameters.AddWithValue("password", hashedPassword);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
