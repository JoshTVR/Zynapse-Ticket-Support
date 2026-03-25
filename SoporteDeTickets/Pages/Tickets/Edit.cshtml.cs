using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Tickets
{
    public class EditTicketModel : PageModel
    {
        private readonly TicketsEfRepo _repo;
        private readonly ApplicationDbContext _context;

        public EditTicketModel(TicketsEfRepo repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [BindProperty]
        public Ticket Ticket { get; set; } = new();

        public List<Cliente> Clientes { get; set; } = new();
        public List<Estado> Estados { get; set; } = new();

        private async Task CargarCombosAsync()
        {
            Clientes = await _context.Clientes
                                     .OrderBy(c => c.Nombre)
                                     .ToListAsync();

            Estados = await _context.Estados
                                     .OrderBy(e => e.Nombre)
                                     .ToListAsync();
        }

        // GET /Tickets/Edit/5
        public async Task<IActionResult> OnGetAsync(int id)
        {
            await CargarCombosAsync();

            var ticket = await _repo.GetAsync(id);
            if (ticket == null)
                return NotFound();

            Ticket = ticket;
            // Por si viene null en DB
            if (Ticket.Prioridad == 0)
                Ticket.Prioridad = 1;

            return Page();
        }

        // POST /Tickets/Edit
        public async Task<IActionResult> OnPostAsync()
        {
            await CargarCombosAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Ticket.Prioridad == 0)
                Ticket.Prioridad = 1;

            await _repo.UpdateAsync(Ticket);

            return RedirectToPage("/Tickets/Index");
        }
    }
}
