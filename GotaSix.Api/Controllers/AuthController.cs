using Microsoft.AspNetCore.Mvc;
using GotaSix.Api.Data;
using GotaSix.Api.Models;
using GotaSix.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GotaSix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Injeção de dependência do Banco de Dados
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // 1. Verifica se o e-mail já existe no banco
            if (_context.Usuarios.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { mensagem = "Este e-mail já está cadastrado." });
            }

            // 2. Criptografa a senha usando o BCrypt
            string senhaCriptografada = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Monta o objeto Usuário para salvar no banco
            var novoUsuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                Cep = request.Cep,
                Rua = request.Rua,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Estado = request.Estado,
                TipoImovel = request.TipoImovel,
                QuantidadeMoradores = request.QuantidadeMoradores,
                SenhaHash = senhaCriptografada
                // DataCadastro já vai com a data atual por padrão, conforme configuramos no Model
            };

            // 4. Salva no banco de dados
            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            // 5. Retorna sucesso!
            return Ok(new { mensagem = "Conta criada com sucesso!" });
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            // 1. Busca o usuário pelo e-mail
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == request.Email);

            if (usuario == null)
            {
                return BadRequest(new { mensagem = "E-mail ou senha incorretos." });
            }

            // 2. Verifica se a senha digitada bate com o Hash do banco usando o BCrypt
            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.SenhaHash);

            if (!senhaValida)
            {
                return BadRequest(new { mensagem = "E-mail ou senha incorretos." });
            }

            // 3. Login com sucesso! 
            // No futuro usaremos Tokens JWT aqui. Por enquanto, retornamos os dados básicos.
            return Ok(new { 
                mensagem = "Login realizado com sucesso!", 
                usuarioId = usuario.Id,
                nome = usuario.Nome 
            });
        }
        // 1. Rota para buscar os dados dinâmicos da Dashboard e verificar o primeiro acesso
        [HttpGet("dashboard/{usuarioId}")]
        public async Task<IActionResult> GetDashboardData(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Historicos)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null) return NotFound(new { mensagem = "Usuário não encontrado." });

            if (!usuario.Historicos.Any())
            {
                return Ok(new { primeiroAcesso = true, nome = usuario.Nome, cidade = usuario.Cidade, estado = usuario.Estado });
            }

            // Pega o histórico em ordem cronológica para achar a última e penúltima leitura
            var historicos = usuario.Historicos.OrderBy(h => h.DataLeitura).ToList();
            var ultima = historicos.Last();
            var penultima = historicos.Count > 1 ? historicos[historicos.Count - 2] : null;

            double diasPassados = 1;
            if (penultima != null)
            {
                // Calcula a diferença exata de dias
                diasPassados = (ultima.DataLeitura.Date - penultima.DataLeitura.Date).TotalDays;
                if (diasPassados < 1) diasPassados = 1; // Previne divisão por zero
            }

            return Ok(new {
                primeiroAcesso = false,
                nome = usuario.Nome,
                cidade = usuario.Cidade,
                estado = usuario.Estado,
                quantidadeMoradores = usuario.QuantidadeMoradores,
                leituraAtual = ultima.LeituraAtual,
                consumoPeriodo = ultima.ConsumoPeriodo,
                alertaVazamento = ultima.AlertaVazamento,
                dataLeitura = ultima.DataLeitura.ToString("dd de MMM, yyyy"),
                diasPassados = diasPassados // Nova propriedade enviada para a tela
            });
        }

        // 2. Rota para salvar a primeira (ou novas) leituras do hidrômetro
        [HttpPost("nova-leitura")]
        public async Task<IActionResult> RegistrarLeitura([FromBody] NovaLeituraDto request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Historicos)
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId);

            if (usuario == null) return BadRequest(new { mensagem = "Usuário inválido." });

            double consumoCalculado = 0;
            bool sinalizarVazamento = false;

            // Se já existirem leituras anteriores, calculamos a diferença de consumo
            var leituraAnterior = usuario.Historicos.OrderByDescending(h => h.DataLeitura).FirstOrDefault();
            if (leituraAnterior != null)
            {
                consumoCalculado = request.ValorLeitura - leituraAnterior.LeituraAtual;
                
                // CORREÇÃO: Pega a diferença de dias até a data de hoje
                double diasPassados = (DateTime.Now.Date - leituraAnterior.DataLeitura.Date).TotalDays;
                if (diasPassados < 1) diasPassados = 1;

                // Cálculo correto: Litros totais / Dias / Moradores
                double consumoPorPessoaDia = (consumoCalculado * 1000 / diasPassados) / usuario.QuantidadeMoradores; 
                
                if (consumoPorPessoaDia > 150) sinalizarVazamento = true;
            }

            var novoRegistro = new HistoricoConsumo
            {
                UsuarioId = request.UsuarioId,
                LeituraAtual = request.ValorLeitura,
                ConsumoPeriodo = consumoCalculado,
                AlertaVazamento = sinalizarVazamento,
                DataLeitura = DateTime.Now
            };

            _context.Historicos.Add(novoRegistro);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Leitura registrada com sucesso!" });
        }

        // Classe DTO necessária para receber a nova leitura
        public class NovaLeituraDto
        {
            public int UsuarioId { get; set; }
            public double ValorLeitura { get; set; }
        }
        [HttpGet("historico/{usuarioId}")]
        public async Task<IActionResult> GetHistorico(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Historicos)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null) return NotFound(new { mensagem = "Usuário não encontrado." });

            // Ordena do mais antigo para o mais novo para conseguirmos olhar o registro "anterior"
            var historicosOrdenados = usuario.Historicos.OrderBy(h => h.DataLeitura).ToList();
            var resultado = new List<object>();

            for (int i = 0; i < historicosOrdenados.Count; i++)
            {
                var h = historicosOrdenados[i];
                double litrosPorPessoaDia = 0;

                if (i > 0)
                {
                    var anterior = historicosOrdenados[i - 1];
                    
                    // Calcula os dias passados entre esta leitura e a anterior
                    double diasPassados = (h.DataLeitura.Date - anterior.DataLeitura.Date).TotalDays;
                    if (diasPassados < 1) diasPassados = 1;

                    litrosPorPessoaDia = ((h.ConsumoPeriodo * 1000) / diasPassados) / usuario.QuantidadeMoradores;
                }

                resultado.Add(new 
                {
                    Id = h.Id,
                    DataFormatada = h.DataLeitura.ToString("dd 'de' MMM, yyyy"),
                    DataCurta = h.DataLeitura.ToString("dd/MM"),
                    DataLeituraReal = h.DataLeitura,
                    LeituraAtual = h.LeituraAtual,
                    ConsumoPeriodo = h.ConsumoPeriodo,
                    ConsumoLitrosPorPessoa = litrosPorPessoaDia,
                    AlertaVazamento = h.AlertaVazamento
                });
            }

            // Inverte a lista para o Front-end receber do mais novo para o mais velho
            resultado.Reverse();
            return Ok(resultado);
        }

        
        [HttpDelete("historico/{id}")]
        public async Task<IActionResult> ExcluirLeitura(int id)
        {
            // 1. Busca a leitura pelo ID recebido
            var leitura = await _context.Historicos.FindAsync(id);
            
            if (leitura == null)
            {
                return NotFound(new { mensagem = "Registro de leitura não encontrado." });
            }

            // 2. Remove o registro do banco de dados
            _context.Historicos.Remove(leitura);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Leitura excluída com sucesso!" });
        }
        // 1. DTO para receber o novo valor
        public class EdicaoLeituraDto
        {
            public double ValorLeitura { get; set; }
            public DateTime DataLeitura { get; set; } 
        }

        // 2. Método PUT para atualizar a leitura
       [HttpPut("historico/{id}")]
        public async Task<IActionResult> EditarLeitura(int id, [FromBody] EdicaoLeituraDto request)
        {
            var leitura = await _context.Historicos.FindAsync(id);
            if (leitura == null) return NotFound(new { mensagem = "Registro não encontrado." });

            // Busca a leitura anterior considerando a NOVA data informada
            var leituraAnterior = await _context.Historicos
                .Where(h => h.UsuarioId == leitura.UsuarioId && h.Id != id && h.DataLeitura <= request.DataLeitura)
                .OrderByDescending(h => h.DataLeitura)
                .FirstOrDefaultAsync();

            double consumoCalculado = 0;
            bool sinalizarVazamento = false;

            if (leituraAnterior != null)
            {
                consumoCalculado = request.ValorLeitura - leituraAnterior.LeituraAtual;
                if (consumoCalculado < 0) 
                    return BadRequest(new { mensagem = $"O valor não pode ser menor que a leitura anterior." });

                // CORREÇÃO: Pega a diferença de dias usando a nova data que o usuário digitou
                double diasPassados = (request.DataLeitura.Date - leituraAnterior.DataLeitura.Date).TotalDays;
                if (diasPassados < 1) diasPassados = 1;

                var usuario = await _context.Usuarios.FindAsync(leitura.UsuarioId);
                if (usuario != null)
                {
                    double consumoPorPessoaDia = (consumoCalculado * 1000 / diasPassados) / usuario.QuantidadeMoradores;
                    if (consumoPorPessoaDia > 150) sinalizarVazamento = true;
                }
            }

            // Atualiza o valor E a data
            leitura.LeituraAtual = request.ValorLeitura;
            leitura.DataLeitura = request.DataLeitura; // <-- Atualiza a data no banco
            leitura.ConsumoPeriodo = consumoCalculado;
            leitura.AlertaVazamento = sinalizarVazamento;

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Leitura atualizada com sucesso!" });
        }
        
        // 1. Rota para buscar os dados atuais do usuário para preencher a tela
        [HttpGet("usuario/{id}")]
        public async Task<IActionResult> GetUsuarioPerfil(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensagem = "Usuário não encontrado." });

            return Ok(new {
                nome = usuario.Nome,
                email = usuario.Email,
                cep = usuario.Cep,
                cidade = usuario.Cidade,
                estado = usuario.Estado,
                quantidadeMoradores = usuario.QuantidadeMoradores
            });
        }

        // 2. Rota para salvar as alterações (incluindo a troca de senha opcional)
        [HttpPut("usuario/{id}")]
        public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] AtualizarUsuarioDto request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensagem = "Usuário não encontrado." });

            // Atualiza os dados básicos
            usuario.Nome = request.Nome;
            usuario.Cep = request.Cep;
            usuario.Cidade = request.Cidade;
            usuario.Estado = request.Estado;
            usuario.QuantidadeMoradores = request.QuantidadeMoradores;

            // Se o usuário digitou uma senha nova, nós geramos um novo Hash para ela
            if (!string.IsNullOrWhiteSpace(request.NovaSenha))
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
        }

        // 3. DTO necessário para receber os dados do Front-end
        public class AtualizarUsuarioDto
        {
            public string Nome { get; set; }
            public string Cep { get; set; }
            public string Cidade { get; set; }
            public string Estado { get; set; }
            public int QuantidadeMoradores { get; set; }
            public string NovaSenha { get; set; } 
        }
    }
}