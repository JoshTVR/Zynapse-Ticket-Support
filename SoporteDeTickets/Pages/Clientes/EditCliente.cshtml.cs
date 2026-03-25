using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Clientes
{
    public class EditClienteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditClienteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Cliente que vamos a editar
        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        // Cargar datos del cliente en el formulario
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            Cliente = cliente;
            return Page();
        }

        // Guardar cambios usando el SP sp_Clientes_Update
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var parameters = new[]
            {
                new SqlParameter("@Id", Cliente.Id),
                new SqlParameter("@Nombre",   (object?)Cliente.Nombre   ?? DBNull.Value),
                new SqlParameter("@Email",    (object?)Cliente.Email    ?? DBNull.Value),
                new SqlParameter("@Telefono", (object?)Cliente.Telefono ?? DBNull.Value),
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Clientes_Update @Id, @Nombre, @Email, @Telefono",
                parameters
            );

            return RedirectToPage("/Clientes/Index");
        }
    }
}
