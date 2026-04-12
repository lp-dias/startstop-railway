using Microsoft.EntityFrameworkCore;
using StartStop.Models;
using System;

namespace StartStop.Data
{
    public class StartStopContext : DbContext
    {
        public StartStopContext(DbContextOptions<StartStopContext> options) : base(options) { }

        // DbSets
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Motorista> Motoristas { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Manutencao> Manutencoes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamento: Reserva -> Usuario (muitos para um)
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento: Reserva -> Veiculo (muitos para um)
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Veiculo)
                .WithMany(v => v.Reservas)
                .HasForeignKey(r => r.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // Função auxiliar para gerar hash da senha
        private string GerarHash(string senha)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }
    }
}
