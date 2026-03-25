namespace SoporteDeTickets.Models
{
    public class EstadoClienteVM
    {
        public int Id { get; set; }              // Id del ticket (folio)
        public string Cliente { get; set; } = ""; // Nombre del cliente
        public string Titulo { get; set; } = "";  // Título del ticket
        public string Estado { get; set; } = "";  // Nombre del estado
    }
}
