namespace SoporteDeTickets.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public int EstadoId { get; set; }
        public Estado? Estado { get; set; }

        public byte Prioridad { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
