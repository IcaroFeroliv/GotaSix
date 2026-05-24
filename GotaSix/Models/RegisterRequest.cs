using System.ComponentModel.DataAnnotations;

namespace GotaSix.Models
{
    public class RegisterRequest
    {
        // --- DADOS PESSOAIS ---
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; }

        // --- DADOS DO IMÓVEL ---
        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Formato de CEP inválido. Use 00000-000.")]
        public string Cep { get; set; }

        public string Rua { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }

        [Required(ErrorMessage = "Informe o tipo de imóvel.")]
        public string TipoImovel { get; set; }

        [Required(ErrorMessage = "Informe a quantidade de moradores.")]
        [Range(1, 50, ErrorMessage = "A quantidade deve ser entre 1 e 50.")]
        public int QuantidadeMoradores { get; set; } = 1; // Valor padrão inicial

        // --- SEGURANÇA ---
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirme sua senha.")]
        [Compare(nameof(Password), ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }
    }
}