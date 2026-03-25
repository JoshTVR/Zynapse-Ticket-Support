using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Models;

namespace SoporteDeTickets.Data
{
    public class TicketsEfRepo
    {
        private readonly ApplicationDbContext _context;

        public TicketsEfRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener lista de tickets
        public async Task<List<Ticket>> ListAsync()
        {
            return await _context.Tickets
                .Include(t => t.Cliente)  // Incluir Cliente
                .Include(t => t.Estado)   // Incluir Estado
                .ToListAsync();
        }

        // Obtener un ticket por ID
        public async Task<Ticket?> GetAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Cliente)
                .Include(t => t.Estado)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Insertar un nuevo ticket
        public async Task<int> InsertAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket.Id;
        }

        // Eliminar un ticket usando el SP sp_Tickets_Delete
        public async Task<int> DeleteAsync(int id)
        {
            var param = new SqlParameter("@Id", id);

            // Ejecuta el stored procedure
            var filasAfectadas = await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Tickets_Delete @Id",
                param
            );

            return filasAfectadas;   // >0 = se eliminó, 0 = no existía
        }


        public async Task<int> UpdateAsync(Ticket ticket)
        {
            var parameters = new[]
            {
        new SqlParameter("@Id",           ticket.Id),
        new SqlParameter("@Titulo",       ticket.Titulo),
        new SqlParameter("@Descripcion",  (object?)ticket.Descripcion ?? DBNull.Value),
        new SqlParameter("@ClienteId",    ticket.ClienteId),
        new SqlParameter("@EstadoId",     ticket.EstadoId),
        new SqlParameter("@Prioridad",    ticket.Prioridad)
    };

            // Ejecuta sp_Tickets_Update
            var filasAfectadas = await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Tickets_Update @Id, @Titulo, @Descripcion, @ClienteId, @EstadoId, @Prioridad",
                parameters
            );

            return filasAfectadas;
        }

    }
}
