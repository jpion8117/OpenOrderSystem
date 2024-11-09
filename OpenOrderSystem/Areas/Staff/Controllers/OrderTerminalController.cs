using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OpenOrderSystem.Areas.Staff.ViewModels.OrderTerminal;
using OpenOrderSystem.Data;
using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.Services;
using OpenOrderSystem.Areas.API.Models;
using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;

namespace OpenOrderSystem.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize]
    public class OrderTerminalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly StaffTerminalMonitoringService _staffTMS;
        private readonly CartService _cartService;

        public OrderTerminalController(ApplicationDbContext context, SignInManager<IdentityUser> signInManager,
            StaffTerminalMonitoringService staffTMS, CartService cartService)
        {
            _context = context;
            _signInManager = signInManager;
            _staffTMS = staffTMS;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/Staff/OrderTerminal/FetchOrderList/{stage}")]
        public IActionResult FetchOrderList(int stage = 0)
        {
            var utcTime = DateTime.UtcNow;
            TimeZoneInfo.TryFindSystemTimeZoneById("Eastern Standard Time", out var localTimeZone);
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone ?? TimeZoneInfo.Local);

            var model = new OrderListVM();
            _context.OrderLines
                .Include(ol => ol.Ingredients)
                .Load();
            _context.MenuItems
                .Include(mi => mi.Ingredients)
                .Load();
            _context.MenuItemVarients.Load();
            _context.Ingredients.Load();
            var ordersToday = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.LineItems)

                .AsEnumerable()
                .Where(o => TimeZoneInfo.ConvertTimeFromUtc(
                    o.OrderPlaced, localTimeZone ?? TimeZoneInfo.Local).Date == currentTime.Date);

            if (ordersToday == null || !ordersToday.Any())
                ordersToday = new List<Order>().AsQueryable();

            switch (stage)
            {
                case 0:
                    model.Stage = OrderStage.Recieved;
                    model.Orders = ordersToday
                        .AsEnumerable()
                        .Where(o => o.Stage == OrderStage.Recieved)
                        .ToList();
                    model.EnabledButtons[OrderTerminalButtons.Info] = true;
                    model.EnabledButtons[OrderTerminalButtons.Next] = true;
                    model.EnabledButtons[OrderTerminalButtons.Cancel] = true;
                    model.ShowAllInfo = true;
                    model.NullMessage = "*** no orders waiting in queue ***";
                    break;

                case 1:
                    model.Stage = OrderStage.InProgress;
                    model.Orders = ordersToday
                        .AsEnumerable()
                        .Where(o => o.Stage == OrderStage.InProgress)
                        .ToList();
                    model.EnabledButtons[OrderTerminalButtons.Info] = true;
                    model.EnabledButtons[OrderTerminalButtons.Timer] = true;
                    model.EnabledButtons[OrderTerminalButtons.Next] = true;
                    model.NullMessage = "*** no orders in progress ***";
                    break;

                case 2:
                    model.Stage = OrderStage.Ready;
                    model.Orders = ordersToday
                        .AsEnumerable()
                        .Where(o => o.Stage == OrderStage.Ready)
                        .ToList();
                    model.EnabledButtons[OrderTerminalButtons.Info] = true;
                    model.EnabledButtons[OrderTerminalButtons.Print] = true;
                    model.EnabledButtons[OrderTerminalButtons.Done] = true;
                    model.NullMessage = "*** no orders ready for pickup ***";
                    break;

                case 3:
                default:
                    model.Stage = OrderStage.Complete;
                    model.Orders = ordersToday
                        .AsEnumerable()
                        .Where(o => o.Stage == OrderStage.Complete)
                        .ToList();
                    model.EnabledButtons[OrderTerminalButtons.Info] = true;
                    model.EnabledButtons[OrderTerminalButtons.Print] = true;
                    model.NullMessage = "*** no completed orders ***";
                    break;
            }

            return PartialView("_OrdersListPartial", model);
        }

        [HttpGet]
        public IActionResult FetchTerminalHeader()
        {
            var utcTime = DateTime.UtcNow;
            TimeZoneInfo.TryFindSystemTimeZoneById("Eastern Standard Time", out var localTimeZone);
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone ?? TimeZoneInfo.Local);

            var model = new OrderHeaderVM();

            model.Recieved = _context.Orders
                .AsEnumerable()
                .Where(o => TimeZoneInfo.ConvertTimeFromUtc(
                    o.OrderPlaced, localTimeZone ?? TimeZoneInfo.Local).Date ==
                    currentTime.Date && o.Stage == OrderStage.Recieved)
                .ToList()
                .Count;

            model.InProgress = _context.Orders
                .AsEnumerable()
                .Where(o => TimeZoneInfo.ConvertTimeFromUtc(
                    o.OrderPlaced, localTimeZone ?? TimeZoneInfo.Local).Date ==
                    currentTime.Date && o.Stage == OrderStage.InProgress)
                .ToList()
                .Count;

            model.Ready = _context.Orders
                .AsEnumerable()
                .Where(o => TimeZoneInfo.ConvertTimeFromUtc(
                    o.OrderPlaced, localTimeZone ?? TimeZoneInfo.Local).Date ==
                    currentTime.Date && o.Stage == OrderStage.Ready)
                .ToList()
                .Count;

            model.Complete = _context.Orders
                .AsEnumerable()
                .Where(o => TimeZoneInfo.ConvertTimeFromUtc(
                    o.OrderPlaced, localTimeZone ?? TimeZoneInfo.Local).Date ==
                    currentTime.Date && o.Stage == OrderStage.Complete)
                .ToList()
                .Count;

            return PartialView("_OrderHeaderPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> PrincessLogout()
        {
            _staffTMS.CloseTerminal();
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> WriteTicket(string? cartId = null)
        {
            await _context.Ingredients.LoadAsync();
            await _context.ProductCategories.LoadAsync();

            var menu = await _context.MenuItems
                .Include(mi => mi.MenuItemVarients)
                .Include(mi => mi.Ingredients)
                .ToListAsync();

            cartId = cartId ?? _cartService.ProvisionCart();

            return View(new WriteTicketVM
            {
                CartId = cartId,
                Cart = _cartService.GetCart(cartId),
                Menu = menu
            });
        }

        [HttpPost]
        public async Task<IActionResult> WriteTicket(WriteTicketVM model)
        {
            var customerDetailModel = new CartCustomer
            {
                CartId = model.CartId,
                Name = model.CustomerName,
                Phone = model.CustomerPhone,
                Email = "null@void.example",
                EmailUpdates = false,
                SmsUpdates = false
            };

            using (var client = new HttpClient())
            {
                var host = HttpContext.Request.Host.ToString();
                client.BaseAddress = new Uri($"https://{host}");

                var response = await client.PutAsync("/API/Cart/Customer", JsonContent.Create(customerDetailModel));

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, response.Content);

                response = await client.PostAsync($"/API/Cart/Submit?cartId={model.CartId}", JsonContent.Create(new { dumdum = "some data" }));

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, response.Content);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddItemToTicket(string cartId, int itemId, int varient)
        {
            var addItemModel = new CartAddItemModel
            {
                CartId = cartId,
                ItemId = itemId,
                Varient = varient
            };

            var client = new HttpClient();
            var host = HttpContext.Request.Host.ToString();
            client.BaseAddress = new Uri($"https://{host}");
            var response = await client.PutAsync("/API/Cart/AddItem", JsonContent.Create(addItemModel));

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, response.Content);

            return RedirectToAction(nameof(WriteTicket), new { cartId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItemFromTicket(string cartId, int position)
        {
            var removeItemModel = new CartUpdateItemModel
            {
                CartId = cartId,
                Index = position
            };

            using (var client = new HttpClient())
            {
                var host = HttpContext.Request.Host.ToString();
                client.BaseAddress = new Uri($"https://{host}");

                var response = await client.PutAsync("/API/Cart/RemoveItem", JsonContent.Create(removeItemModel));

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, response.Content);
            }

            return RedirectToAction(nameof(WriteTicket), new { cartId });
        }

        [HttpPost]
        public async Task<IActionResult> ModifyTicketItem(string cartId, int position, int varient, string comments, int[] ingredients)
        {
            var updateItemModel = new CartUpdateItemModel
            {
                CartId = cartId,
                Index = position,
                IngredientIds = ingredients,
                VarientIndex = varient,
                LineComments = comments
            };

            using (var client = new HttpClient())
            {
                var host = HttpContext.Request.Host.ToString();
                client.BaseAddress = new Uri($"https://{host}");

                var response = await client.PutAsync("/API/Cart/UpdateItem", JsonContent.Create(updateItemModel));

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, response.Content);
            }

            return RedirectToAction(nameof(WriteTicket), new { cartId });
        }

    }
}
