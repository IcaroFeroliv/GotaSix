namespace GotaSix.Api.Models.DTOs
{
    // Esta classe serve apenas para receber os dados do Front-end
    public class RegisterRequestDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cep { get; set; }
        public string Rua { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string TipoImovel { get; set; }
        public int QuantidadeMoradores { get; set; }
        public string Password { get; set; }
        // Não precisamos do ConfirmPassword aqui, pois o Blazor já validou isso no Front-end!
    }
}