namespace GotaSix.Api.Models
{
    public class HistoricoConsumo
    {
        public int Id { get; set; } // Chave Primária
        
        // Chave Estrangeira: Diz de qual usuário é este registro
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } 

        public DateTime DataLeitura { get; set; } = DateTime.Now;
        public double LeituraAtual { get; set; } // Valor lido no relógio (em m³)
        public double ConsumoPeriodo { get; set; } // Diferença entre a leitura atual e a anterior
        public bool AlertaVazamento { get; set; } // Flag para o sistema avisar o morador
    }
}