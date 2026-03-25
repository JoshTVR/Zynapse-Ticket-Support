using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Tickets
{
    public class TicketsPageModel : PageModel
    {
        private readonly TicketsEfRepo _ticketsRepo;

        public TicketsPageModel(TicketsEfRepo ticketsRepo)
        {
            _ticketsRepo = ticketsRepo;
        }

        public IList<Ticket> TicketList { get; set; } = new List<Ticket>();

        // GET
        public async Task OnGetAsync()
        {
            TicketList = await _ticketsRepo.ListAsync();
        }

        // POST /Tickets?handler=Delete&id=123
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _ticketsRepo.DeleteAsync(id);
            return RedirectToPage(); // recarga la misma página
        }
    }
}
