namespace GotaSix.Api.Models.DTOs
{
    // Esta classe serve apenas para receber os dados de login do Front-end
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}