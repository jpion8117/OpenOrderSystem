using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenOrderSystem.Data;
using OpenOrderSystem.Services;
using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.Models;
using OpenOrderSystem.ViewModels.Home;
using OpenOrderSystem.ViewModels.Order;
using System.Diagnostics;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OpenOrderSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly ConfigurationService _config;
        private readonly StaffTerminalMonitoringService _staffTMS;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
            CartService cartService, ConfigurationService config, StaffTerminalMonitoringService staffTMS)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
            _config = config;
            _staffTMS = staffTMS;
        }

        // GET: MenuItems
        public async Task<IActionResult> Index(string error = "")
        {
            //ViewBag.Error = error;

            //var model = await LoadMenuModel();

            //if (!_config.Settings.AcceptingOrders)
            //    return View("Unavailable");
            //else if (!_staffTMS.TerminalActive)
            //    return View("Unavailable");

            return View();
        }

        //public IActionResult ViewCartModal(string cartId)
        //{
        //    var cart = _cartService.GetCart(cartId);
        //    if (cart == null)
        //    {
        //        return NotFound("Failed to locate cart");
        //    }

        //    return PartialView("_ViewCartPartial", cart);
        //}

        //public IActionResult EditItemModal(string cartId, int index)
        //{
        //    var model = new EditItemModelVM();

        //    model.CartId = cartId;
        //    model.Index = index;
        //    model.Cart = _cartService.GetCart(cartId);

        //    if (model.Cart == null)
        //    {
        //        return NotFound("Cart service failed to locate cart.");
        //    }

        //    model.CurrentIngredients = model.Cart?.LineItems?[model.Index].Ingredients ?? new List<Ingredient>();

        //    var categoryId = model.Cart?.LineItems?[model.Index].MenuItem?.ProductCategoryId ?? -1;
        //    if (categoryId == -1)
        //    {
        //        return NotFound("Invalid MenuItem.ProductCategoryId");
        //    }

        //    _context.Ingredients
        //        .Include(i => i.Category)
        //        .Load();

        //    var category = _context.ProductCategories
        //        .Include(pc => pc.Ingredients)
        //        .Include(pc => pc.MenuItems)
        //        .FirstOrDefault(pc => pc.Id == categoryId);

        //    List<Ingredient> menuItemIngredients = category?.Ingredients ?? new List<Ingredient>();

        //    foreach (Ingredient ingredient in menuItemIngredients)
        //    {
        //        if (ingredient.Category != null)
        //        {
        //            if (model.AvailableIngredients.ContainsKey(ingredient.Category))
        //            {
        //                model.AvailableIngredients[ingredient.Category].Add(ingredient);
        //            }
        //            else
        //            {
        //                model.AvailableIngredients[ingredient.Category] = new List<Ingredient> 
        //                {
        //                    ingredient
        //                };
        //            }
        //        }
        //    }

        //    return PartialView("_EditItemPartial", model);
        //}

        //[HttpGet]
        //public IActionResult CheckoutModal(string cartId)
        //{
        //    var model = new CheckoutVM();
        //    model.CartId = cartId;

        //    var cart = _cartService.GetCart(cartId);
        //    if (cart == null) return NotFound();

        //    model.Cart = cart;

        //    return PartialView("_CheckoutModalPartial", model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SubmitOrder(CheckoutVM model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return RedirectToAction("Index", new { error = "Please include name, valid phone number, and valid email on the checkout screen..." });
        //    }

        //    bool captchaSuccess = false;
        //    int orderId = -1;
        //    using (var client = new HttpClient())
        //    {
        //        var host = HttpContext.Request.Host.ToString();
        //        client.BaseAddress = new Uri($"https://{host}");

        //        var key = Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA") ?? "";
        //        var form = new MultipartFormDataContent
        //        {
        //            { new StringContent(key), "secret" },
        //            { new StringContent(model.CaptchaToken), "response" }
        //        };

        //        var result = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", form);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            var response = await result.Content.ReadAsStreamAsync();
        //            var json = await JsonNode.ParseAsync(response);
        //            captchaSuccess = (bool?)json?["success"] ?? false;
        //        }

        //        if (!captchaSuccess) return BadRequest();

        //        model.Cart = _cartService.GetCart(model.CartId);

        //        var data = JsonContent.Create(new
        //        {
        //            cartId = model.CartId,
        //            name = model.Name,
        //            phone = model.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", ""),
        //            email = model.Email,
        //            smsUpdates = model.TextUpdates,
        //            emailUpdates = false
        //        });

        //        result = await client.PutAsync("/API/Cart/Customer", data);
        //        if (!result.IsSuccessStatusCode) return BadRequest();

        //        result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/API/Cart/Submit?cartId={model.CartId}"));

        //        if (result.IsSuccessStatusCode)
        //        {
        //            var response = await result.Content.ReadAsStreamAsync();
        //            var json = await JsonNode.ParseAsync(response);
        //            orderId = (int?)json?["orderId"] ?? 0;
        //        }
        //        else
        //            return BadRequest("Unable to submit cart");
        //    }

        //    return RedirectToActionPermanent("Status", "Order", new {
        //        id = orderId
        //    });
        //}

        //[Route("/Offline/ViewMenu")]
        //public async Task<IActionResult> ViewOffline()
        //{
        //    var model = await LoadMenuModel(loadCart: false);

        //    return View("Index", model);
        //}
        //public bool TestOnline()
        //{
        //    return _config.Settings.AcceptingOrders && _staffTMS.TerminalActive;
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        //private async Task<HomeIndexVM> LoadMenuModel(bool loadCart = true)
        //{
        //    var model = new HomeIndexVM();
        //    if (loadCart)
        //    {
        //        const string COOKIE_KEY = "VMRC_ORDER_CART_ID";
        //        if (Request.Cookies.ContainsKey(COOKIE_KEY))
        //        {
        //            var cart = _cartService.GetCart(Request.Cookies[COOKIE_KEY] ?? "");
        //            if (cart == null)
        //            {
        //                model.CartId = _cartService.ProvisionCart();
        //                Response.Cookies.Append(COOKIE_KEY, model.CartId, new CookieOptions
        //                {
        //                    Expires = DateTimeOffset.UtcNow.AddHours(3)
        //                });
        //            }
        //            else
        //            {
        //                model.CartId = cart.Id;
        //            }
        //        }
        //        else
        //        {
        //            model.CartId = _cartService.ProvisionCart();
        //            Response.Cookies.Append(COOKIE_KEY, model.CartId, new CookieOptions
        //            {
        //                Expires = DateTimeOffset.UtcNow.AddHours(3)
        //            });
        //        }
        //    }
        //    else
        //        model.CartId = "OFFLINE_MODE";

        //    model.Menu = await _context.MenuItems
        //        .Include(mi => mi.MenuItemVarients)
        //        .Include(mi => mi.Ingredients)
        //        .Include(mi => mi.ProductCategory)
        //        .ToListAsync();

        //    model.Categories = await _context.ProductCategories
        //        .Include(pc => pc.MenuItems)
        //        .Include(pc => pc.Ingredients)
        //        .ToListAsync();

        //    return model;
        //}
    }
}
