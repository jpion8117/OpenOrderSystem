using OpenOrderSystem.Data.DataModels;

namespace OpenOrderSystem.ViewModels.Home
{
    public class HomeIndexVM
    {
        public string CartId { get; set; } = string.Empty;
        public List<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        public List<MenuItem> Menu { get; set; } = new List<MenuItem>();
    }
}
