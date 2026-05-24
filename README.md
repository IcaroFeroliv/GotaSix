# 💧 GotaSix (Gota6)

O **GotaSix** é uma plataforma Full-Stack moderna e responsiva voltada para o monitoramento inteligente do consumo de água residencial e comercial. O sistema permite que os usuários registrem suas leituras de hidrômetro, acompanhem o gasto diário de forma visual e recebam alertas automáticos sobre possíveis vazamentos ocultos, promovendo economia financeira e sustentabilidade.

## 🚀 Tecnologias Utilizadas

O projeto foi construído utilizando o ecossistema .NET, separando claramente as responsabilidades entre cliente e servidor:

* **Front-end:** Blazor WebAssembly (WASM) com layouts adaptativos em C# e HTML/CSS puro (sem dependência de frameworks CSS pesados).
* **Back-end:** ASP.NET Core Web API.
* **Banco de Dados:** SQLite integrado via Entity Framework Core (Code-First).
* **Segurança:** Hashing de senhas utilizando `BCrypt.Net-Next`.
* **Integrações Externas:** Consumo nativo da API ViaCEP para preenchimento de endereços.

## ✨ Funcionalidades Principais

O sistema possui um fluxo completo (CRUD) e inteligência de negócio aplicada:

* **🔐 Autenticação e Gestão de Perfil:** * Criação de conta, login e proteção de rotas (Session).
  * Painel de configurações para alteração de dados da residência e alteração segura de senha (com validação e confirmação visual).
  * Busca automática de Cidade e Estado ao digitar o CEP.
* **📊 Dashboard Inteligente:** * Identificação automática de primeiro acesso com redirecionamento para "Leitura Inicial".
  * Cálculo temporal preciso: a API mede os dias exatos entre as leituras para fornecer uma média real de Litros/Pessoa/Dia.
  * Sistema de alertas baseado nas metas recomendadas pela OMS (110 L/dia por pessoa).
* **📈 Histórico e Gráficos Dinâmicos:**
  * Renderização de gráficos de linha desenhados nativamente com SVG no Blazor (alta performance, sem bibliotecas externas).
  * Edição de registros antigos (com recálculo automático de consumo retroativo).
  * Exclusão de registros suspeitos ou errados.
* **📱 UX/UI Consistente:**
  * Sidebar responsiva para navegação.
  * Ícones padronizados e feedback visual (cores e caixas de mensagens) para ações de sucesso ou erro em todas as telas.

## 💻 Telas do Sistema

*(Adicione aqui os links para as imagens/prints do seu projeto rodando)*

* `[Print da Tela de Login e Cadastro]`
* `[Print da Dashboard Desktop]`
* `[Print do Histórico com o Gráfico em SVG]`
* `[Print da Tela de Configurações]`

## ⚙️ Como executar o projeto localmente

O sistema é dividido em duas camadas (Client e API). Você precisará de dois terminais para rodar a aplicação completa.

**1. Configurando o Banco de Dados (API)**
Abra o terminal na pasta `GotaSix.Api` e execute os comandos para criar e popular o banco de dados SQLite local:

    dotnet ef migrations add BancoInicial
    dotnet ef database update

**2. Rodando o Back-end (API)**
No mesmo terminal (`GotaSix.Api`), inicie o servidor da API:

    dotnet run

*(A API estará rodando e escutando na porta configurada, ex: http://localhost:5252)*

**3. Rodando o Front-end (Blazor)**
Abra um **segundo terminal** na pasta do projeto Front-end (`GotaSix`) e inicie a interface:

    dotnet watch

*(O navegador abrirá automaticamente a tela inicial do Gota6).*

---
Desenvolvido com dedicação por **Ícaro Ferreira de Oliveira**.
