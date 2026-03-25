using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Tickets
{
    [IgnoreAntiforgeryToken]
    public class CreateTicketModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TicketsEfRepo _ticketsRepo;

        public CreateTicketModel(ApplicationDbContext context, TicketsEfRepo ticketsRepo)
        {
            _context = context;
            _ticketsRepo = ticketsRepo;
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

        public async Task OnGetAsync()
        {
            await CargarCombosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CargarCombosAsync();

            if (!ModelState.IsValid)
                return Page();

            Ticket.Prioridad = 1;
            Ticket.FechaCreacion = DateTime.Now;

            await _ticketsRepo.InsertAsync(Ticket);

            return RedirectToPage("/Tickets/Index");
        }
    }
}
