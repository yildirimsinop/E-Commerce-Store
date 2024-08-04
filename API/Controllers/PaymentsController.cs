using API.Data;
using API.DTOs;
using API.Entities.OrderAggregate;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private readonly PaymentService _paymentService;
        private readonly StoreContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(PaymentService paymentService, StoreContext context, IConfiguration config, ILogger<PaymentsController> logger)
        {
            _config = config;
            _context = context;
            _paymentService = paymentService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent()
        {
            _logger.LogInformation("CreateOrUpdatePaymentIntent started");

            var basket = await _context.Baskets
                .RetrieveBasketWithItems(User.Identity.Name)
                .FirstOrDefaultAsync();

            if (basket == null) 
            {
                _logger.LogWarning("Basket not found for user: {UserName}", User.Identity.Name);
                return NotFound();
            }

            var intent = await _paymentService.CreateOrUpdatePaymentIntent(basket);

            if (intent == null) 
            {
                _logger.LogError("Problem creating payment intent for basket: {BasketId}", basket.Id);
                return BadRequest(new ProblemDetails { Title = "Problem creating payment intent" });
            }

            basket.PaymentIntentId = basket.PaymentIntentId ?? intent.Id;
            basket.ClientSecret = basket.ClientSecret ?? intent.ClientSecret;

            _context.Update(basket);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) 
            {
                _logger.LogError("Problem updating basket with intent: {PaymentIntentId}", basket.PaymentIntentId);
                return BadRequest(new ProblemDetails { Title = "Problem updating basket with intent" });
            }

            _logger.LogInformation("Payment intent created or updated successfully for basket: {BasketId}, PaymentIntentId: {PaymentIntentId}", basket.Id, basket.PaymentIntentId);

            return basket.MapBasketToDto();
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(signatureHeader))
            {
                return BadRequest("Missing Stripe-Signature header");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _config["StripeSettings:WebhookSecret"]);
                _logger.LogInformation("Stripe Event Received: {EventType}", stripeEvent.Type);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation("PaymentIntent Succeeded: {PaymentIntentId}", paymentIntent.Id);

                    var order = await _context.Orders.FirstOrDefaultAsync(x => x.PaymentIntentId == paymentIntent.Id);
                    if (order != null)
                    {
                        order.OrderStatus = OrderStatus.PaymentReceived;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("Order not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe Exception: {Message}", e.Message);
                return BadRequest(new { error = e.Message });
            }
        }
    }
}
