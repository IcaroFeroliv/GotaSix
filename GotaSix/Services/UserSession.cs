namespace GotaSix.Services
{
    // Esta classe guardará os dados do usuário logado na memória do navegador
    public class UserSession
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public bool EstaLogado => UsuarioId > 0;
    }
}