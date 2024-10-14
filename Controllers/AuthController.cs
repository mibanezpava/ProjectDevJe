using Microsoft.AspNetCore.Mvc;
using jejames.api.ApiFactura.DTOs;
using jejames.api.ApiFactura.Services;
using NLog;
using System.Threading.Tasks;

namespace jejames.api.ApiFactura.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        /// <param name="registerDto">Datos de registro.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                Logger.Warn("Datos de registro inválidos.");
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.RegisterAsync(registerDto);
                Logger.Info($"Nuevo usuario registrado: {registerDto.Username}");
                return Ok(new { mensaje = "Usuario registrado exitosamente." });
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, $"Error al registrar el usuario: {registerDto.Username}");
                return StatusCode(500, new { mensaje = "Ocurrió un error al registrar el usuario." });
            }
        }

        /// <summary>
        /// Autentica un usuario y devuelve un token JWT.
        /// </summary>
        /// <param name="loginDto">Datos de login.</param>
        /// <returns>Token JWT.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                Logger.Warn("Datos de login inválidos.");
                return BadRequest(ModelState);
            }

            var user = await _userService.AuthenticateAsync(loginDto.Username, loginDto.Password);
            if (user == null)
            {
                Logger.Warn($"Intento de login fallido para el usuario: {loginDto.Username}");
                return Unauthorized(new { mensaje = "Credenciales inválidas." });
            }

            var token = await _userService.GenerateJwtTokenAsync(user);
            Logger.Info($"Usuario autenticado: {user.Username}");
            return Ok(new { token });
        }
    }
}
