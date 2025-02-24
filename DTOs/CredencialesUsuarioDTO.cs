using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class CredencialesUsuarioDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        public string? Password { get; set; }
    }
}
