using API.Entities;
using Stripe;

namespace API.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _config;
        public PaymentService(IConfiguration config)
        {
            _config = config;
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
        }

       public async Task<PaymentIntent> CreateOrUpdatePaymentIntent(Basket basket)
{
    try
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)basket.Items.Sum(item => item.Quantity * item.Product.Price * 100),
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" },
        };

        var service = new PaymentIntentService();
        PaymentIntent intent;

        if (string.IsNullOrEmpty(basket.PaymentIntentId))
        {
            intent = await service.CreateAsync(options);
            Console.WriteLine("Payment Intent Created: " + intent.Id);
        }
        else
        {
            var updateOptions = new PaymentIntentUpdateOptions
            {
                Amount = options.Amount,
                Currency = options.Currency,
                PaymentMethodTypes = options.PaymentMethodTypes
            };
            intent = await service.UpdateAsync(basket.PaymentIntentId, updateOptions);
            Console.WriteLine("Payment Intent Updated: " + intent.Id);
        }

        return intent;
    }
    catch (StripeException stripeEx)
    {
        Console.WriteLine("Stripe Error: " + stripeEx.Message);
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating/updating payment intent: " + ex.Message);
        return null;
    }
}
