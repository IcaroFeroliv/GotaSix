using GotaSix.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GotaSix.Api.Data
{
    // O DbContext é a ponte entre o seu código e o banco de dados
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Estas propriedades (DbSets) representam as tabelas no banco
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<HistoricoConsumo> Historicos { get; set; }
    }
}