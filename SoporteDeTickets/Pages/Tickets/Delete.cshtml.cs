using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoporteDeTickets.Data;
using SoporteDeTickets.Models;
using System.Threading.Tasks;

namespace SoporteDeTickets.Pages.Tickets
{
    public class DeleteModel : PageModel
    {
        private readonly TicketsEfRepo _repo;

        public DeleteModel(TicketsEfRepo repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public Ticket Ticket { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ticket = await _repo.GetAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            Ticket = ticket;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _repo.DeleteAsync(id);   // 👈 aquí se usa el SP
            return RedirectToPage("./Index");
        }
    }
}
