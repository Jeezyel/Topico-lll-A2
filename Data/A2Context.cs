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

            // Configuração opcional para garantir que o 'Id' seja a chave primária
            modelBuilder.Entity<WeatherForces>()
                .HasKey(w => w.Id);

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

            // Cliente -> Usuario (1:1)
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull); // Se deletar o usuário, o cliente continua existindo (sem login)

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
                .WithMany(r => r.Incidencias)
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
        }
    }
}