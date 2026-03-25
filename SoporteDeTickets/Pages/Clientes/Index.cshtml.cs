using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Clientes
{
    public class ClientesPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ClientesPageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista de clientes que se pasar�n a la vista
        public IList<Cliente> Clientes { get; set; }

        public async Task OnGetAsync()
        {
            Clientes = await _context.Clientes.ToListAsync();
        }

public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var parameter = new SqlParameter("@Id", id);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Clientes_Delete @Id",
            parameter
        );

        return RedirectToPage();
    }

}
}
