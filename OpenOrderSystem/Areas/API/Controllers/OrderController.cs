using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenOrderSystem.Data;
using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.Services;
using OpenOrderSystem.Services.Interfaces;
using PizzaPartry.tools;

namespace OpenOrderSystem.Areas.API.Controllers
{
    [Area("API")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StaffTerminalMonitoringService _staffTMS;
        private readonly ConfigurationService _config;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public OrderController(ApplicationDbContext context, StaffTerminalMonitoringService staffTMS,
            ConfigurationService config, IEmailService emailService, ISmsService smsService)
        {
            _context = context;
            _staffTMS = staffTMS;
            _config = config;
            _emailService = emailService;
            _smsService = smsService;
        }

        [HttpGet]
        [Route("API/CheckOrder/{orderId}")]
        public IActionResult CheckStatus(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound($"failed to locate order#:{orderId}");
            }

            return new JsonResult(new
            {
                //get times
                orderRecievedTime = order.OrderPlaced,
                orderInProgressTime = order.OrderInprogress,
                orderReadyTime = order.OrderReady,
                orderCompleteTime = order.OrderComplete,

                //check stages
                orderInProgress = order.OrderInprogress != null,
                orderReady = order.OrderReady != null,
                orderComplete = order.OrderComplete != null
            });
        }

        [HttpGet]
        public bool IsOpen() =>
            _staffTMS.TerminalActive &&         //verifys the staff terminal hasn't lost connection
            _config.Settings.AcceptingOrders;   //verifys time within scheduled ordering hours

        [HttpGet]
        [Authorize]
        [Route("/API/Staff/Orders/Detail/{id}")]
        public IResult Detail(int id)
        {
            _context.MenuItems
                .Include(mi => mi.Ingredients)
                .Load();
            _context.MenuItemVarients.Load();
            _context.Ingredients.Load();
            _context.OrderLines
                .Include(ol => ol.Ingredients)
                .Load();

            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.LineItems)
                .FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                var fancyAssPhoneNumber = "(";
                var phone = order.Customer?.Phone ?? "Error Retrieving Phone";
                for (var k = 0; k < phone.Length; ++k)
                {
                    var d = phone[k];
                    if (k == 2)
                        fancyAssPhoneNumber += $"{d})";
                    else if (k == 5)
                        fancyAssPhoneNumber += $"{d}-";
                    else
                        fancyAssPhoneNumber += d;
                }

                var details = new
                {
                    orderNum = order.Id,
                    customerName = order.Customer?.Name ?? string.Empty,
                    customerPhone = fancyAssPhoneNumber ?? string.Empty,
                    subtotal = order.Subtotal.ToString("C"),
                    tax = order.Tax.ToString("C"),
                    total = order.Total.ToString("C"),
                    lineItems = new List<object>()
                };

                foreach (var item in order.LineItems)
                {
                    var additions = new List<object>();
                    foreach (var add in item.AddedIngredients)
                        additions.Add(new
                        {
                            name = add.Name,
                            price = add.Price
                        });

                    var subtractions = new List<object>();
                    foreach (var sub in item.RemovedIngredients)
                        subtractions.Add(new
                        {
                            name = sub.Name,
                            price = sub.Price
                        });

                    var algorithm = new CheckDigitCalc.WeightingFactor[]
                    {
                        CheckDigitCalc.WeightingFactor.TwoMinus,
                        CheckDigitCalc.WeightingFactor.TwoMinus,
                        CheckDigitCalc.WeightingFactor.Three,
                        CheckDigitCalc.WeightingFactor.FiveMinus
                    };

                    var barcodePrice = CheckDigitCalc.Create(item.LinePrice
                        .ToString("C")
                        .Replace("$", "")
                        .Replace(".", "")
                        .Replace(" ", "")
                        .PadLeft(4, '0'), algorithm)
                        .GetResult();

                    details.lineItems.Add(new
                    {
                        name = item.MenuItem?.Name ?? string.Empty,
                        varient = item.MenuItem?.MenuItemVarients?[item.MenuItemVarient]?.Descriptor ?? string.Empty,
                        additions,
                        subtractions,
                        modified = additions.Any() || subtractions.Any(),
                        comments = item.LineComments ?? string.Empty,
                        price = item.LinePrice,
                        plu = item.MenuItem?.MenuItemVarients?[item.MenuItemVarient]?.Upc ?? "00000",
                        upc = "2" + (item.MenuItem?.MenuItemVarients?[item.MenuItemVarient]?.Upc ?? "00000") + barcodePrice
                    });
                }

                return Results.Ok(details);
            }

            return Results.NotFound();
        }

        [HttpPut]
        [Authorize]
        public IActionResult UpdateStatus(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound($"failed to locate order#:{orderId}");
            }

            order.CompleteStage();
            _context.Orders.Update(order);
            _context.SaveChanges();

            if (order.Customer != null)
                AlertCustomer(order.Customer, order.Stage);

            return new JsonResult(new
            {
                message = $"Order advanced to stage {order.Stage}."
            });
        }

        [HttpPut]
        [Authorize]
        public void TerminalCheckin()
        {
            _staffTMS.RegisterCheckin();
            var ordersInProgress = _context.Orders
                .Include(o => o.Customer)
                .AsEnumerable()
                .Where(o => o.OrderPlaced.Date == DateTime.UtcNow.Date
                    && o.Stage == OrderStage.InProgress)
                .ToList();

            foreach (var order in ordersInProgress)
            {
                if (order.CheckTimer() == TimerStatus.TimeUp)
                {
                    order.CompleteStage();
                    _context.Update(order);
                    _context.SaveChanges();

                    if (order.Customer != null)
                        AlertCustomer(order.Customer, order.Stage);
                }
                if (order.CheckTimer() == TimerStatus.LessThanTwo)
                {
                    _staffTMS.TriggerOrderTimerAlert();
                }
            }
        }

        [HttpPut]
        [Authorize]
        public void TerminalClose() => _staffTMS.CloseTerminal();

        private void AlertCustomer(Customer customer, OrderStage stage)
        {
            switch (stage)
            {
                case OrderStage.Ready:
                    if (customer.EmailUpdates)
                    {
                        _emailService.Send(
                            customer.Email,
                            "Village Market Pizza Order",
                            "Your order is ready for pickup. Thank you for ordering from Village Market.");
                    }

                    if (customer.SMSUpdates)
                    {
                        var phone = _smsService.ConvertPhone(customer.Phone);
                        _smsService.SendSMS(phone, "Your order is ready for pickup. Thank " +
                            "you for ordering from Village Market Rapid City.");
                    }
                    break;
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("API/Order/CancelOrder")]
        public IResult CancelOrder(int orderNumber)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderNumber);
            if (order == null)
                return Results.NotFound(new
                {
                    OrderId = orderNumber,
                    Message = $"Unable to locate order #{orderNumber}."
                });

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return Results.Ok(new
            {
                Message = $"Order #{orderNumber} successsfully canceled"
            });
        }
    }
}
