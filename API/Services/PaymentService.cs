using API.Entities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
    var service = new PaymentIntentService();
    PaymentIntent intent;

    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)basket.Items.Sum(item => item.Quantity * item.Product.Price * 100),
        Currency = "usd",
        PaymentMethodTypes = new List<string> { "card" },
        PaymentMethod = "pm_card_visa"  // Varsayılan bir ödeme yöntemi ekleyin
    };

    if (string.IsNullOrEmpty(basket.PaymentIntentId))
    {
        intent = await service.CreateAsync(options);
    }
    else
    {
        var updateOptions = new PaymentIntentUpdateOptions
        {
            Amount = options.Amount,
            PaymentMethod = "pm_card_visa"  // Varsayılan bir ödeme yöntemi ekleyin
        };
        intent = await service.UpdateAsync(basket.PaymentIntentId, updateOptions);
    }

    return intent;
}
            }
        }
