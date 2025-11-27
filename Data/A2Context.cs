using A2.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace A2.Data
{
    public class A2Context: DbContext
    {
        public A2Context(DbContextOptions<A2Context> options): base(options)
        {
        }

        // --- Tabelas do Banco de Dados ---

        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Manutencao> Manutencoes { get; set; }

        public DbSet<Motorista> Motoristas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<EnderecoCliente> EnderecosClientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }
        public DbSet<Rota> Rotas { get; set; }
        public DbSet<RotaPedido> RotaPedidos { get; set; }
        public DbSet<AlertaClimatico> AlertasClimaticos { get; set; }
        public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; }
        public DbSet<IncidenciaRota> IncidenciasRota { get; set; }
        public DbSet<JanelaHorario> JanelasHorarias { get; set; }
        public DbSet<LogIntegracao> LogsIntegracao { get; set; }
        public DbSet<PontoDeParada> PontosDeParada { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // CONFIGURAÇÕES DE RELACIONAMENTOS E TIPOS DE COLUNAS (SEU CÓDIGO)
            // =================================================================

            // Veiculo -> Manutencao (1:N)
            modelBuilder.Entity<Manutencao>()
                .HasOne(m => m.Veiculo)
                .WithMany(v => v.Manutencoes)
                .HasForeignKey(m => m.VeiculoId)
                .OnDelete(DeleteBehavior.Cascade); // Se deletar o veículo, deleta as manutenções

            // Motorista -> Usuario (1:1)
            modelBuilder.Entity<Motorista>()
                .HasOne(m => m.Usuario)
                .WithOne()
                .HasForeignKey<Motorista>(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull); // Se deletar o usuário, motorista continua existindo (sem login)

            // Usuario -> Role (N:1)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tipos de Coluna para Veiculo
            modelBuilder.Entity<Veiculo>()
                .Property(v => v.CapacidadeCarga)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Veiculo>()
                .Property(v => v.CapacidadeVolume)
                .HasColumnType("decimal(10,2)");

            // Tipo de Coluna para Manutencao
            modelBuilder.Entity<Manutencao>()
                .Property(m => m.Custo)
                .HasColumnType("decimal(10,2)");

            // Tipos de Coluna para Pedido
            modelBuilder.Entity<Pedido>()
                .Property(p => p.PesoTotalKg)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.VolumeTotalM3)
                .HasColumnType("decimal(10,2)");

            // Pedido -> Cliente (N:1, Restrict)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido -> EnderecoEntrega (N:1, Restrict)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.EnderecoEntrega)
                .WithMany()
                .HasForeignKey(p => p.EnderecoEntregaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tipos de Coluna para ItemPedido
            modelBuilder.Entity<ItemPedido>()
                .Property(ip => ip.PesoUnitarioKg)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ItemPedido>()
                .Property(ip => ip.VolumeUnitarioM3)
                .HasColumnType("decimal(10,2)");

            // ItemPedido -> Pedido (N:1, Cascade)
            modelBuilder.Entity<ItemPedido>()
                .HasOne(i => i.Pedido)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // RotaPedido (Chave Composta e Relacionamentos)
            modelBuilder.Entity<RotaPedido>()
                .HasKey(rp => new { rp.RotaId, rp.PedidoId });

            modelBuilder.Entity<RotaPedido>()
                .HasOne(rp => rp.Rota)
                .WithMany(r => r.RotaPedidos)
                .HasForeignKey(rp => rp.RotaId);

            modelBuilder.Entity<RotaPedido>()
                .HasOne(rp => rp.Pedido)
                .WithMany(p => p.RotaPedidos)
                .HasForeignKey(rp => rp.PedidoId);

            // EnderecoCliente -> Cliente (N:1, Cascade)
            modelBuilder.Entity<EnderecoCliente>()
                .HasOne(e => e.Cliente)
                .WithMany(c => c.Enderecos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tipos de Coluna para EnderecoCliente (Coordenadas)
            modelBuilder.Entity<EnderecoCliente>()
                .Property(e => e.Latitude).HasColumnType("decimal(10,8)");

            modelBuilder.Entity<EnderecoCliente>()
                .Property(e => e.Longitude).HasColumnType("decimal(11,8)");

            // AlertaClimatico -> Rota (N:1, Cascade)
            modelBuilder.Entity<AlertaClimatico>()
                .HasOne(a => a.Rota)
                .WithMany(r => r.AlertasClimaticos)
                .HasForeignKey(a => a.RotaId)
                .OnDelete(DeleteBehavior.Cascade);

            // IncidenciaRota -> Rota (N:1, Restrict)
            modelBuilder.Entity<IncidenciaRota>()
                .HasOne(i => i.Rota)
                .WithMany()
                .HasForeignKey(i => i.RotaId)
                .OnDelete(DeleteBehavior.Restrict);

            // JanelaHorario -> EnderecoCliente (N:1, Cascade)
            modelBuilder.Entity<JanelaHorario>()
                .HasOne(j => j.EnderecoCliente)
                .WithMany()
                .HasForeignKey(j => j.EnderecoClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // PontoDeParada -> Rota (N:1, Cascade)
            modelBuilder.Entity<PontoDeParada>()
                .HasOne(p => p.Rota)
                .WithMany()
                .HasForeignKey(p => p.RotaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tipos de Coluna para PontoDeParada (Coordenadas)
            modelBuilder.Entity<PontoDeParada>()
                .Property(p => p.Latitude).HasColumnType("decimal(10,8)");

            modelBuilder.Entity<PontoDeParada>()
                .Property(p => p.Longitude).HasColumnType("decimal(11,8)");

            // =================================================================
            // SEED DATA (POPULAÇÃO INICIAL DE DADOS)
            // =================================================================

            // --- 1. ROLES (Papéis) ---
            modelBuilder.Entity<Role>().HasData(
                new { Id = 1, Nome = "Administrador" },
                new { Id = 2, Nome = "Motorista" },
                new { Id = 3, Nome = "Cliente" }
            );

            // --- 2. USUARIOS (Usuários) ---
            modelBuilder.Entity<Usuario>().HasData(
                new { Id = 1, Nome = "Admin Sistema", Email = "admin@empresa.com", SenhaHash = "hashed_admin_password", RoleId = 1 },
                new { Id = 2, Nome = "João Motorista", Email = "joao@empresa.com", SenhaHash = "hashed_joao_password", RoleId = 2 }
            );

            // --- 3. MOTORISTAS ---
            modelBuilder.Entity<Motorista>().HasData(
                new { Id = 1, Nome = "João Motorista", CPF = "11122233344", CNH = "12345678901", Telefone = "999887766", Email = "joao@empresa.com", UsuarioId = 2 }
            );

            // --- 4. VEICULOS (Veículos) ---
            modelBuilder.Entity<Veiculo>().HasData(
                new
                {
                    Id = 1,
                    Placa = "ABC1234",
                    Marca = "Ford",
                    Modelo = "Cargo 816",
                    AnoFabricacao = 2020,
                    CapacidadeCarga = 4500.00m,
                    CapacidadeVolume = 25.00m,
                    Status = (A2.Models.StatusVeiculo)1,
                    DataUltimaManutencao = new DateTime(2025, 10, 01),
                    DataProximaManutencao = new DateTime(2026, 04, 01)
                }
            );

            // --- 5. CLIENTES ---
            modelBuilder.Entity<Cliente>().HasData(
                new
                {
                    Id = 1,
                    NomeEmpresa = "Distribuidora Alfa",
                    CNPJ = "00000000000000",
                    NomeContato = "Maria Silva",
                    Telefone = "1133445566",
                    Email = "contato@alfa.com"
                }
            );

            // --- 6. ENDERECO CLIENTE ---
            modelBuilder.Entity<EnderecoCliente>().HasData(
                new
                {
                    Id = 1,
                    ClienteId = 1,
                    CEP = "01001000",
                    Logradouro = "Rua Principal",
                    Numero = "100",
                    Bairro = "Centro",
                    Cidade = "São Paulo",
                    UF = "SP",
                    Complemento = "Galpão A",
                    Latitude = -23.550520m,
                    Longitude = -46.633308m
                }
            );

            // --- 7. PEDIDOS ---
            modelBuilder.Entity<Pedido>().HasData(
    new
    {
        Id = 1,
        ClienteId = 1,
        EnderecoEntregaId = 1,
        DataCriacao = new DateTime(2025, 11, 26, 10, 0, 0),
        DataLimiteEntrega = new DateTime(2025, 12, 05),
        PesoTotalKg = 500.00m,
        VolumeTotalM3 = 5.00m,
        // **CORREÇÃO AQUI:** Faça o cast explícito para o tipo enum.
        Status = (A2.Models.StatusPedido)0 // Ou (A2.Models.StatusPedido)0
    }
);

            // --- 8. ITENS PEDIDO ---
            modelBuilder.Entity<ItemPedido>().HasData(
                new { Id = 1, PedidoId = 1, CodigoProduto = "PROD001", Descricao = "Caixa de Eletrônicos", Quantidade = 10, PesoUnitarioKg = 40.00m, VolumeUnitarioM3 = 0.40m },
                new { Id = 2, PedidoId = 1, CodigoProduto = "PROD005", Descricao = "Pallet de Roupas", Quantidade = 5, PesoUnitarioKg = 20.00m, VolumeUnitarioM3 = 0.20m }
            );

            // --- 9. ROTAS ---
            modelBuilder.Entity<Rota>().HasData(
                new
                {
                    Id = 1,
                    MotoristaId = 1,
                    VeiculoId = 1,
                    DataRota = new DateTime(2025, 11, 28),
                    Status = (A2.Models.StatusRota)0
                }
            );

            // --- 10. ROTA PEDIDO (Associação N:M) ---
            modelBuilder.Entity<RotaPedido>().HasData(
                new { RotaId = 1, PedidoId = 1, OrdemEntrega = 1, StatusEntrega = "Aguardando" }
            );

            // --- 11. PONTO DE PARADA ---
            modelBuilder.Entity<PontoDeParada>().HasData(
                new
                {
                    Id = 1,
                    RotaId = 1,
                    EnderecoClienteId = 1,
                    OrdemParada = 1,
                    Latitude = -23.550520m,
                    Longitude = -46.633308m
                }
            );

            // --- 12. CONFIGURAÇÕES SISTEMA ---
            modelBuilder.Entity<ConfiguracaoSistema>().HasData(
                new { Id = 1, ApiNome = "WeatherAPI", Endpoint = "https://api.clima.com/v1", Chave = "WEATHER_API_KEY", Valor = "xyz123abc" }
            );
        }
    }
}