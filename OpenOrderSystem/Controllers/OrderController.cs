using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenOrderSystem.Data;
using OpenOrderSystem.ViewModels.Order;

namespace OpenOrderSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(IndexVM? model)
        {
            if (model != null && model.OrderId == null && model.Phone == null && model.Name == null)
                model = null;
            if (model != null && ModelState.IsValid)
            {
                if (model.OrderId != null)
                {
                    var order = _context.Orders
                        .FirstOrDefault(o => o.Id == model.OrderId);

                    if (order != null)
                    {
                        return RedirectToAction("Status", new
                        {
                            id = model.OrderId
                        });
                    }

                    ModelState.AddModelError("OrderId",
                        $"Unable to locate Order #{model.OrderId}, please double check your order number or search by name and phone number.");
                }
                else if (model.Phone != null)
                {
                    var orders = _context.Orders
                        .Include(o => o.Customer)
                        .Where(o => o.Customer != null
                            && o.Customer.Phone == model.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", ""))
                        .OrderByDescending(o => o.OrderPlaced)
                        .ToList();

                    if (orders.Any())
                    {
                        if (orders.Count() == 1)
                            return RedirectToAction("Status", new
                            {
                                id = orders[0].Id
                            });

                        model.MyOrders = orders;
                        return View(model);
                    }

                    ModelState.AddModelError("Phone",
                        $"No recent orders found under the phone number {model.Phone}. Please try again.");
                }
                else
                {
                    ModelState.AddModelError(model.Name == null ? "Name" : "Phone",
                        "Please include both the name and phone number on your order so we can locate it.");
                }
            }

            return View(model ?? new IndexVM());
        }

        public IActionResult Status(int id)
        {
            _context.OrderLines
                .Include(ol => ol.Ingredients)
                .Include(ol => ol.MenuItem)
                .Load();

            _context.MenuItems
                .Include(mi => mi.Ingredients)
                .Load();

            _context.MenuItemVarients.Load();
            _context.Ingredients.Load();

            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.LineItems)
                .FirstOrDefault(o => id == o.Id);

            if (order == null)
            {
                return NotFound($"The order number #{id} could not be located.");
            }

            var model = new StatusVM
            {
                Order = order
            };

            return View(model);
        }
    }
}
