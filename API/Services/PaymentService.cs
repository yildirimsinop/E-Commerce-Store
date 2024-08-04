using API.Entities;
using Microsoft.Extensions.Logging;
using Stripe;

namespace API.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IConfiguration config, ILogger<PaymentService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<PaymentIntent> CreateOrUpdatePaymentIntent(Basket basket)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var service = new PaymentIntentService();

            var intent = new PaymentIntent();
            var subtotal = basket.Items.Sum(i => i.Quantity * i.Product.Price);
            var deliveryFee = subtotal > 10000 ? 0 : 500;

            _logger.LogInformation("Creating or updating payment intent for basket: {BasketId}", basket.Id);

            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                _logger.LogInformation("Creating new payment intent for basket: {BasketId}", basket.Id);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = subtotal + deliveryFee,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };
                intent = await service.CreateAsync(options);

                _logger.LogInformation("Payment intent created: {PaymentIntentId}", intent.Id);
            }
            else
            {
                _logger.LogInformation("Updating existing payment intent: {PaymentIntentId} for basket: {BasketId}", basket.PaymentIntentId, basket.Id);

                var options = new PaymentIntentUpdateOptions
                {
                    Amount = subtotal + deliveryFee
                };
                await service.UpdateAsync(basket.PaymentIntentId, options);

                _logger.LogInformation("Payment intent updated: {PaymentIntentId}", basket.PaymentIntentId);
            }

            return intent;
        }
    }
}
