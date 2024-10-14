using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.Text;
using jejames.api.ApiFactura.Repositories;
using jejames.api.ApiFactura.Services;
using jejames.api.ApiFactura.Middleware;

var logger = LogManager.Setup().LoadConfigurationFromFile("Logging/nlog.config").GetCurrentClassLogger();
try
{
    logger.Debug("Iniciando aplicación");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar NLog
    builder.Logging.ClearProviders(); // Limpiar los proveedores de logging por defecto
    builder.Host.UseNLog(); // Usar NLog como el logger

    // Configurar servicios
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            policy =>
            {
                policy.WithOrigins("https://tu-frontend.com") // Reemplaza con la URL de tu frontend
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
    });

    // Configurar JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Opcional: reduce el margen de tiempo para la expiración del token
        };
    });

    // Registrar servicios y repositorios
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();

    var app = builder.Build();

    // Configurar el pipeline de la solicitud HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseCors("AllowSpecificOrigin");

    // app.UseHttpsRedirection(); // Descomenta si deseas habilitar HTTPS

    app.UseAuthentication(); // Agregar autenticación
    app.UseAuthorization();  // Agregar autorización

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // Capturar cualquier excepción durante la configuración
    logger.Error(ex, "Error al iniciar la aplicación");
    throw;
}
finally
{
    LogManager.Shutdown(); // Asegurar que todos los logs se escriban antes de cerrar la aplicación
}
