using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Models;

namespace SoporteDeTickets.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        // Definimos los DbSets para nuestras tablas
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Estado> Estados { get; set; }

        // 👇 DbSet para mapear el resultado del SP
        public DbSet<EstadoClienteVM> EstadosClientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EstadoClienteVM>().HasNoKey();
        }

        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Como es un ViewModel, le indicamos la clave
            modelBuilder.Entity<EstadoClienteVM>().HasKey(e => e.Id);
        }
        */
    }
}
