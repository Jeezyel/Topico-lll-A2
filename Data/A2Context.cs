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

        public DbSet<WeatherForces> WeatherForces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- RELACIONAMENTOS E CONFIGURAÇÕES DE ENTIDADES ---

            // WeatherForces
            modelBuilder.Entity<WeatherForces>().HasKey(w => w.Id);

            // Veiculo
            modelBuilder.Entity<Veiculo>().Property(v => v.CapacidadeCarga).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Veiculo>().Property(v => v.CapacidadeVolume).HasColumnType("decimal(10,2)");

            // Manutencao
            modelBuilder.Entity<Manutencao>().Property(m => m.Custo).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Manutencao>().HasOne(m => m.Veiculo).WithMany(v => v.Manutencoes).HasForeignKey(m => m.VeiculoId).OnDelete(DeleteBehavior.Cascade);

            // Motorista
            modelBuilder.Entity<Motorista>().HasOne(m => m.Usuario).WithOne().HasForeignKey<Motorista>(m => m.UsuarioId).OnDelete(DeleteBehavior.SetNull);

            // Cliente
            modelBuilder.Entity<Cliente>().HasOne(c => c.Usuario).WithOne().HasForeignKey<Cliente>(c => c.UsuarioId).OnDelete(DeleteBehavior.SetNull);

            // Usuario
            modelBuilder.Entity<Usuario>().HasOne(u => u.Role).WithMany(r => r.Usuarios).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

            // Pedido
            modelBuilder.Entity<Pedido>().Property(p => p.PesoTotalKg).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Pedido>().Property(p => p.VolumeTotalM3).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Pedido>().HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Pedido>().HasOne(p => p.EnderecoEntrega).WithMany().HasForeignKey(p => p.EnderecoEntregaId).OnDelete(DeleteBehavior.Restrict);

            // ItemPedido
            modelBuilder.Entity<ItemPedido>().Property(ip => ip.PesoUnitarioKg).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<ItemPedido>().Property(ip => ip.VolumeUnitarioM3).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<ItemPedido>().HasOne(i => i.Pedido).WithMany(p => p.ItensPedido).HasForeignKey(i => i.PedidoId).OnDelete(DeleteBehavior.Cascade);

            // RotaPedido (Chave Composta)
            modelBuilder.Entity<RotaPedido>().HasKey(rp => new { rp.RotaId, rp.PedidoId });
            modelBuilder.Entity<RotaPedido>().HasOne(rp => rp.Rota).WithMany(r => r.RotaPedidos).HasForeignKey(rp => rp.RotaId);
            modelBuilder.Entity<RotaPedido>().HasOne(rp => rp.Pedido).WithMany(p => p.RotaPedidos).HasForeignKey(rp => rp.PedidoId);

            // EnderecoCliente
            modelBuilder.Entity<EnderecoCliente>().HasOne(e => e.Cliente).WithMany(c => c.Enderecos).HasForeignKey(e => e.ClienteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EnderecoCliente>().Property(e => e.Latitude).HasColumnType("decimal(10,8)");
            modelBuilder.Entity<EnderecoCliente>().Property(e => e.Longitude).HasColumnType("decimal(11,8)");

            // AlertaClimatico
            modelBuilder.Entity<AlertaClimatico>().HasOne(a => a.Rota).WithMany(r => r.AlertasClimaticos).HasForeignKey(a => a.RotaId).OnDelete(DeleteBehavior.Cascade);

            // IncidenciaRota
            modelBuilder.Entity<IncidenciaRota>().HasOne(i => i.Rota).WithMany(r => r.Incidencias).HasForeignKey(i => i.RotaId).OnDelete(DeleteBehavior.Restrict);

            // JanelaHorario
            modelBuilder.Entity<JanelaHorario>().HasOne(j => j.EnderecoCliente).WithMany().HasForeignKey(j => j.EnderecoClienteId).OnDelete(DeleteBehavior.Cascade);

            // PontoDeParada
            modelBuilder.Entity<PontoDeParada>().HasOne(p => p.Rota).WithMany().HasForeignKey(p => p.RotaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PontoDeParada>().Property(p => p.Latitude).HasColumnType("decimal(10,8)");
            modelBuilder.Entity<PontoDeParada>().Property(p => p.Longitude).HasColumnType("decimal(11,8)");

            // --- SEED DATA ---

            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Nome = "Administrador" },
                new Role { Id = 2, Nome = "Motorista" },
                new Role { Id = 3, Nome = "Cliente" }
            );

            // ConfiguracoesSistema
            modelBuilder.Entity<ConfiguracaoSistema>().HasData(
                new ConfiguracaoSistema { Id = 1, ApiNome = "OpenWeatherMap", Chave = "SUA_CHAVE_NOS_USER_SECRETS", Endpoint = "https://api.openweathermap.org/data/2.5/", Valor = "Configuração Padrão" }
            );

            // Usuarios (Senha para todos é '123456', será resetada pelo Program.cs na inicialização)
            string passHashPlaceholder = "needs_reset"; 
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, Email = "admin@logifleet.com", Nome = "Admin Sistema", RoleId = 1, SenhaHash = passHashPlaceholder },
                new Usuario { Id = 2, Email = "carlos.mot@logifleet.com", Nome = "Carlos Motorista", RoleId = 2, SenhaHash = passHashPlaceholder },
                new Usuario { Id = 3, Email = "ana.mot@logifleet.com", Nome = "Ana Motorista", RoleId = 2, SenhaHash = passHashPlaceholder },
                new Usuario { Id = 4, Email = "roberto.cli@techstore.com", Nome = "Roberto Cliente", RoleId = 3, SenhaHash = passHashPlaceholder },
                new Usuario { Id = 5, Email = "maria.cli@example.com", Nome = "Maria Cliente", RoleId = 3, SenhaHash = passHashPlaceholder },
                new Usuario { Id = 6, Email = "pedro.cli@example.com", Nome = "Pedro Cliente", RoleId = 3, SenhaHash = passHashPlaceholder },
                new Usuario { Id = 7, Email = "joana.cli@example.com", Nome = "Joana Cliente", RoleId = 3, SenhaHash = passHashPlaceholder }
            );
            
            // Motoristas
            modelBuilder.Entity<Motorista>().HasData(
                new Motorista { Id = 1, CNH = "11111111111", CPF = "12345678901", Email = "carlos.mot@logifleet.com", Nome = "Carlos Oliveira", Telefone = "11999998888", UsuarioId = 2 },
                new Motorista { Id = 2, CNH = "22222222222", CPF = "98765432109", Email = "ana.mot@logifleet.com", Nome = "Ana Souza", Telefone = "11977776666", UsuarioId = 3 }
            );

            // Clientes
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, CNPJ = "12345678000199", Email = "compras@techstore.com.br", NomeContato = "Roberto Lima", NomeEmpresa = "TechStore Eletrônicos", Telefone = "1130304040", UsuarioId = 4 },
                new Cliente { Id = 2, CNPJ = "98765432000155", Email = "gerencia@supermercado.com", NomeContato = "Fernanda Costa", NomeEmpresa = "Supermercado Preço Bom", Telefone = "1132325050", UsuarioId = 7 },
                new Cliente { Id = 3, CNPJ = "11223344000100", Email = "maria@lojadamaria.com.br", NomeContato = "Maria Silva", NomeEmpresa = "Loja da Maria", Telefone = "1199887766", UsuarioId = 5 },
                new Cliente { Id = 4, CNPJ = "55443322000111", Email = "pedro@mercadopedro.com.br", NomeContato = "Pedro Mendes", NomeEmpresa = "Mercado do Pedro", Telefone = "1196655443", UsuarioId = 6 }
            );

            // EnderecosClientes
            modelBuilder.Entity<EnderecoCliente>().HasData(
                new EnderecoCliente { Id = 1, ClienteId = 1, CEP = "01310100", Logradouro = "Avenida Paulista", Numero = "1000", Bairro = "Bela Vista", Cidade = "São Paulo", UF = "SP", Latitude = -23.561496m, Longitude = -46.655967m, Complemento = "Conjunto 101" },
                new EnderecoCliente { Id = 2, ClienteId = 2, CEP = "01001000", Logradouro = "Praça da Sé", Numero = "10", Bairro = "Centro", Cidade = "São Paulo", UF = "SP", Latitude = -23.5489m, Longitude = -46.6388m, Complemento = "Loja Térrea" }
            );

            // Veiculos
            modelBuilder.Entity<Veiculo>().HasData(
                 new Veiculo { Id = 1, AnoFabricacao = 2022, CapacidadeCarga = 8000.00m, CapacidadeVolume = 45.00m, DataProximaManutencao = new DateTime(2026, 3, 1), DataUltimaManutencao = new DateTime(2025, 9, 1), Marca = "Volvo", Modelo = "VM 270", Placa = "LOG-1001", Status = StatusVeiculo.Disponivel },
                 new Veiculo { Id = 2, AnoFabricacao = 2023, CapacidadeCarga = 1500.00m, CapacidadeVolume = 12.00m, DataProximaManutencao = new DateTime(2025, 12, 1), DataUltimaManutencao = new DateTime(2025, 6, 1), Marca = "Mercedes", Modelo = "Sprinter", Placa = "LOG-2002", Status = StatusVeiculo.EmManutencao },
                 new Veiculo { Id = 3, AnoFabricacao = 2021, CapacidadeCarga = 800.00m, CapacidadeVolume = 4.00m, DataProximaManutencao = new DateTime(2026, 1, 1), DataUltimaManutencao = new DateTime(2025, 10, 1), Marca = "Fiat", Modelo = "Fiorino", Placa = "LOG-3003", Status = StatusVeiculo.EmRota }
            );
        }
    }
}