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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Veiculo -> Manutencao (1:N)
            modelBuilder.Entity<Manutencao>()
                .HasOne(m => m.Veiculo)
                .WithMany(v => v.Manutencoes)
                .HasForeignKey(m => m.VeiculoId)
                .OnDelete(DeleteBehavior.Cascade); // Se deletar o veículo, deleta as manutenções

            // Motorista -> Usuario (1:1 opcional)
            modelBuilder.Entity<Motorista>()
                .HasOne(m => m.Usuario)
                .WithOne()
                .HasForeignKey<Motorista>(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull); // Se deletar o usuário, motorista continua existindo (sem login)

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Role)            // Um Usuário TEM UMA Role
                .WithMany(r => r.Usuarios)      // Uma Role PODE TER MÚLTIPLOS Usuários
                .HasForeignKey(u => u.RoleId)   // A chave estrangeira está em Usuario.RoleId
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
        }
    }
}