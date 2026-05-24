namespace GotaSix.Api.Models
{
    public class Usuario
    {
        public int Id { get; set; } // Chave Primária
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cep { get; set; }
        public string Rua { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string TipoImovel { get; set; }
        public int QuantidadeMoradores { get; set; }
        
        // Não guardamos a senha em texto limpo, guardamos um Hash por segurança
        public string SenhaHash { get; set; } 
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Relacionamento: Um usuário possui vários registros de consumo
        public List<HistoricoConsumo> Historicos { get; set; } = new();
    }
}