namespace jejames.api.ApiFactura.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        // Nota: No expongas la contraseña en las respuestas de la API
        public string Role { get; set; }
    }
}
