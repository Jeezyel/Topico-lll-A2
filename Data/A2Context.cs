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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Veiculo -> Manutencao (1:N)
            modelBuilder.Entity<Manutencao>()
                .HasOne(m => m.Veiculo)
                .WithMany(v => v.Manutencoes)
                .HasForeignKey(m => m.VeiculoId)
                .OnDelete(DeleteBehavior.Cascade); // Se deletar o veículo, deleta as manutenções

            modelBuilder.Entity<Motorista>()
                .HasOne(m => m.Usuario)
                .WithOne()
                .HasForeignKey<Motorista>(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull); // Se deletar o usuário, motorista continua existindo (sem login)

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Role)            // Um Usuário TEM UMA Role
                .WithMany(r => r.Usuarios)      // Uma Role PODE TER MÚLTIPLOS Usuários
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veiculo>()
                .Property(v => v.CapacidadeCarga)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Veiculo>()
                .Property(v => v.CapacidadeVolume)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Manutencao>()
                .Property(m => m.Custo)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.PesoTotalKg)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.VolumeTotalM3) 
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.EnderecoEntrega)
                .WithMany()
                .HasForeignKey(p => p.EnderecoEntregaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ItemPedido>()
                .Property(ip => ip.PesoUnitarioKg) 
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ItemPedido>()
                .Property(ip => ip.VolumeUnitarioM3) 
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ItemPedido>()
                .HasOne(i => i.Pedido)
                .WithMany(p => p.ItensPedido) 
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rota>()
                .HasMany(r => r.Pedidos)
                .WithOne(p => p.Rota)
                .HasForeignKey(p => p.RotaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EnderecoCliente>()
                .HasOne(e => e.Cliente)
                .WithMany(c => c.Enderecos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnderecoCliente>()
                .Property(e => e.Latitude).HasColumnType("decimal(10,8)");

            modelBuilder.Entity<EnderecoCliente>()
                .Property(e => e.Longitude).HasColumnType("decimal(11,8)");
        }
    }
}