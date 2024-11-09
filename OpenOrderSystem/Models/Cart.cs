using OpenOrderSystem.Data.DataModels;

namespace OpenOrderSystem.Models
{
    public class Cart
    {
        public Cart()
        {
            Id = Guid.NewGuid().ToString();
            CartLastActive = DateTime.UtcNow;
            Order = new Order();
        }

        /// <summary>
        /// Unique GUID used to identify this cart
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Tracks when the cart was opened so abandoned carts can be purged.
        /// </summary>
        public DateTime CartLastActive { get; set; }

        /// <summary>
        /// Order associated with cart
        /// </summary>
        public Order Order { get; set; }

        public Customer? Customer
        {
            get => Order.Customer;
            set => Order.Customer = value;
        }

        public List<OrderLine> LineItems
        {
            get => Order.LineItems;
            set => Order.LineItems = value;
        }

        /// <summary>
        /// Checks for expired carts, returns true if cart hasn't been used in over 60 minutes.
        /// </summary>
        public bool Expired { get => DateTime.UtcNow > CartLastActive.AddMinutes(60); }
    }
}
