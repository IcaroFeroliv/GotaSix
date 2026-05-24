using System.ComponentModel.DataAnnotations;

namespace GotaSix.Models
{
    // Representa o dado necessário para iniciar a recuperação de senha
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "O e-mail é obrigatório para recuperar a senha.")]
        [EmailAddress(ErrorMessage = "Por favor, insira um formato de e-mail válido.")]
        public string Email { get; set; }
    }
}