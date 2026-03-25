using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Clientes
{
    public class CreateClienteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateClienteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; }

        public async Task OnGetAsync()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var parameters = new[]
            {
                new SqlParameter("@Nombre", Cliente.Nombre),
                new SqlParameter("@Email", Cliente.Email),
                new SqlParameter("@Telefono", Cliente.Telefono)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_Clientes_Insert @Nombre, @Email, @Telefono", parameters);

            return RedirectToPage("/Clientes/Index");
        }
    }
}
