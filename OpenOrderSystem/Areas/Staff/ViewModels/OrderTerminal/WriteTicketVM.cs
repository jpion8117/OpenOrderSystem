using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.Models;

namespace OpenOrderSystem.Areas.Staff.ViewModels.OrderTerminal
{
    public class WriteTicketVM
    {
        public List<MenuItem> Menu { get; set; } = new List<MenuItem>();
        public Cart? Cart { get; set; }
        public string CartId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }
}
