using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenOrderSystem.Areas.API.Models;
using OpenOrderSystem.Data;
using OpenOrderSystem.Services;

namespace OpenOrderSystem.Areas.API.Controllers.Staff
{
    [Area("API/Staff")]
    [ApiController]
    [Route("API/Staff/TerminalService/{action}")]

    [Authorize]
    public class TerminalServiceController : ControllerBase
    {
        private readonly StaffTerminalMonitoringService _staffTMS;
        private readonly ApplicationDbContext _context;

        public TerminalServiceController(ApplicationDbContext context, StaffTerminalMonitoringService staffTMS)
        {
            _context = context;
            _staffTMS = staffTMS;
        }

        [HttpGet]
        public IResult Alerts()
        {
            var newOrderAlert = _staffTMS.NewOrderAlert;
            var timeLowAlert = _staffTMS.OrderTimerAlert;
            var genericAlerts = new Dictionary<string, bool>();
            foreach (var alert in _staffTMS.GenericTriggers.Keys)
            {
                //copys generic alerts and clears them from the queue.
                genericAlerts[alert] = _staffTMS.CheckGenericTrigger(alert);
            }

            if (!newOrderAlert && !timeLowAlert && genericAlerts.Count == 0)
                return Results.NoContent();

            return Results.Ok(new
            {
                newOrderAlert,
                timeLowAlert,
                genericAlerts
            });
        }

        [HttpPut]
        public IResult AddOrderTime([FromForm] AddTimeModel model)
        {
            var orderId = model.OrderId;
            var time = model.Time;

            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                return Results.NotFound(new
                {
                    errorMessage = $"Failed to locate order#: {orderId}."
                });

            order.AddToTimer(time);
            _context.Orders.Update(order);
            _context.SaveChanges();

            return Results.Ok(new
            {
                orderId,
                timeAdded = time,
                errorMessage = $"SUCCESS: Successfully added {time} minutes to order#: {orderId}"
            });
        }
    }
}
