using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Estados
{
    public class EstadosPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EstadosPageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ViewModel con los datos del SP
        public IList<EstadoClienteVM> Estados { get; set; } = new List<EstadoClienteVM>();

        public async Task OnGetAsync()
        {
            Estados = await _context.EstadosClientes
                .FromSqlRaw("EXEC sp_Estados_List")
                .ToListAsync();
        }
    }
}
